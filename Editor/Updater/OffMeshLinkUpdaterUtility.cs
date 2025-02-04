using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Unity.AI.Navigation.Updater
{
#pragma warning disable 618
    internal static class OffMeshLinkUpdaterUtility
    {
        /// <summary>
        /// A structure holding the information of failed conversions.
        /// </summary>
        public struct FailedConversion
        {
            public int itemIndex;
            public string failureMessage;
        }

        /// <summary>
        /// Find all prefabs and scenes that contain OffMeshLink components.
        /// This method also finds Prefab Variants which has removed OffMeshLink components.
        /// </summary>
        /// <param name="searchInFolders">Folders to search for prefabs and scenes. If null, the whole project is searched.</param>
        /// <returns>List of asset GUIDs to convert.</returns>
        public static List<string> FindObjectsToConvert(string[] searchInFolders = null)
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchInFolders);
            var objectsToConvert = new HashSet<string>();
            foreach (var guid in prefabGuids)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab.GetComponentsInChildren<OffMeshLink>(true).Length > 0)
                    objectsToConvert.Add(guid);
                var isPrefabVariant = PrefabUtility.IsPartOfVariantPrefab(prefab);
                if (isPrefabVariant)
                {
                    var removedComponents = PrefabUtility.GetRemovedComponents(prefab);
                    foreach (var removedComponent in removedComponents)
                    {
                        if (removedComponent.assetComponent is OffMeshLink)
                        {
                            objectsToConvert.Add(guid);
                            break;
                        }
                    }
                }
            }

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", searchInFolders);
            foreach (var guid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                var rootGameObjects = scene.GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects)
                {
                    var offMeshLinksInHierarchy = rootGameObject.GetComponentsInChildren<OffMeshLink>(true);
                    if (offMeshLinksInHierarchy.Length == 0)
                        continue;

                    var offMeshLinks = FindAllOffMeshLinksInHierarchy(rootGameObject);
                    if (offMeshLinks.Count > 0)
                    {
                        objectsToConvert.Add(guid);
                        break;
                    }
                }
                CloseScene(scene);
            }

            var returnList = new List<string>(objectsToConvert);
            return returnList;
        }

        static void CloseScene(Scene scene)
        {
            // EditorSceneManager does not support closing the last scene, so we open a new empty scene instead.
            if (EditorSceneManager.loadedSceneCount == 1)
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            else
                EditorSceneManager.CloseScene(scene, true);
        }

        /// <summary>
        /// Convert all objects in the list to use NavMeshLink instead of OffMeshLink.
        /// </summary>
        /// <param name="objectsToConvert">List of asset GUIDs to convert.</param>
        /// <param name="failedConversions">List of failed conversions.</param>
        public static void Convert(List<string> objectsToConvert, out List<FailedConversion> failedConversions)
        {
            failedConversions = new List<FailedConversion>();
            var offMeshLinkToNavMeshLink = new Dictionary<OffMeshLink, NavMeshLink>();
            var failedToConvert = new HashSet<string>();

            // Initial sorting of objects to convert.
            // We want to deal with source prefabs first, then their variants, so that the NavMeshLink components are created in the correct order.
            SortAssetsToConvert(objectsToConvert);

            // First convert, create NavMeshLink with OffMeshLink values
            foreach (var guid in objectsToConvert)
            {
                var pathToObject = AssetDatabase.GUIDToAssetPath(guid);
                if (!DoesAssetExistOnDisk(pathToObject))
                {
                    failedToConvert.Add(guid);
                    failedConversions.Add(new FailedConversion
                    {
                        itemIndex = objectsToConvert.IndexOf(guid),
                        failureMessage = $"Cannot find the asset at path {pathToObject}. Please make sure the file exists."
                    });
                    continue;
                }
                if (!CanWriteToAsset(pathToObject))
                {
                    failedToConvert.Add(guid);
                    failedConversions.Add(new FailedConversion
                    {
                        itemIndex = objectsToConvert.IndexOf(guid),
                        failureMessage = $"Cannot write to the asset at path {pathToObject}. Please make sure the file is not read-only."
                    });
                    continue;
                }

                if (TryGetPrefabFromPath(pathToObject, out var prefab))
                    ConvertPrefab(prefab, offMeshLinkToNavMeshLink);
                else if (TryGetSceneFromPath(pathToObject, out var scene))
                {
                    ConvertScene(scene, ref offMeshLinkToNavMeshLink);
                    CloseScene(scene);
                }
            }

            // Remove failed conversions
            foreach (var guid in failedToConvert)
                objectsToConvert.Remove(guid);

            // Second convert, apply prefab overrides to NavMeshLink
            foreach (var guid in objectsToConvert)
            {
                var pathToObject = AssetDatabase.GUIDToAssetPath(guid);
                if (TryGetPrefabFromPath(pathToObject, out var prefab))
                {
                    ApplyOverrideDataToPrefab(prefab, offMeshLinkToNavMeshLink);
                    SyncOverriddenRemovalsOfComponents(prefab, offMeshLinkToNavMeshLink);
                }
                else if (TryGetSceneFromPath(pathToObject, out var scene))
                {
                    ApplyOverrideDataToScene(scene, offMeshLinkToNavMeshLink);
                    CloseScene(scene);
                }
            }

            // Remove OffMeshLinks from the objects
            foreach (var guid in objectsToConvert)
            {
                var pathToObject = AssetDatabase.GUIDToAssetPath(guid);
                if (TryGetPrefabFromPath(pathToObject, out var prefab))
                {
                    var offMeshLinks = prefab.GetComponentsInChildren<OffMeshLink>(true);
                    foreach (var offMeshLink in offMeshLinks)
                        Object.DestroyImmediate(offMeshLink, true);

                    // Since we are removing components in source prefabs, we have to "revert removed components"
                    // on prefab instances, so that we don't store the removal data in the meta files.
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(prefab))
                        RevertRemovedComponent(prefab);

                    PrefabUtility.SavePrefabAsset(prefab);
                }
                else if (TryGetSceneFromPath(pathToObject, out var scene))
                {
                    var rootGameObjects = scene.GetRootGameObjects();
                    foreach (var rootGameObject in rootGameObjects)
                    {
                        var offMeshLinks = rootGameObject.GetComponentsInChildren<OffMeshLink>(true);
                        foreach (var offMeshLink in offMeshLinks)
                            Object.DestroyImmediate(offMeshLink, true);
                    }
                    EditorSceneManager.SaveScene(scene);
                    CloseScene(scene);
                }
            }
        }

        /// <summary>
        /// Sorts the list of objects to convert so that source prefabs are first.
        /// </summary>
        /// <param name="objectsToConvert">List of asset GUIDs to convert.</param>
        static void SortAssetsToConvert(List<string> objectsToConvert)
        {
            var sortedAssetGuids = new List<string>(objectsToConvert.Count);
            for (var i = 0; i < objectsToConvert.Count; i++)
            {
                var assetGuid = objectsToConvert[i];
                if (sortedAssetGuids.Contains(assetGuid))
                    continue;

                var prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuid));
                if (prefabGameObject == null)
                    sortedAssetGuids.Add(assetGuid);
                else
                    SortPrefabsToConvert(sortedAssetGuids, prefabGameObject, assetGuid);
            }

            objectsToConvert.Clear();
            objectsToConvert.AddRange(sortedAssetGuids);
        }

        static void SortPrefabsToConvert(List<string> objectsToConvert, GameObject prefabRoot, string prefabRootGuid)
        {
            if (objectsToConvert.Contains(prefabRootGuid))
                return;

            var source = PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
            if (source == null)
            {
                objectsToConvert.Add(prefabRootGuid);
                return;
            }

            var sourceGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
            SortPrefabsToConvert(objectsToConvert, source, sourceGuid);
            objectsToConvert.Add(prefabRootGuid);
        }

        static void RevertRemovedComponent(GameObject prefab)
        {
            var removedComponents = PrefabUtility.GetRemovedComponents(prefab);
            if (removedComponents == null)
                return;

            foreach (var removedComponent in removedComponents)
            {
                if (removedComponent.assetComponent is OffMeshLink)
                    PrefabUtility.RevertRemovedComponent(prefab, removedComponent.assetComponent, InteractionMode.AutomatedAction);
            }
        }

        static bool TryGetPrefabFromPath(string path, out GameObject prefab)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab);
        }

        static bool TryGetSceneFromPath(string path, out Scene scene)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (sceneAsset != null)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                return true;
            }
            scene = default;
            return false;
        }

        static List<OffMeshLink> FindAllOffMeshLinksInHierarchy(GameObject rootGameObject)
        {
            var arr = rootGameObject.GetComponentsInChildren<OffMeshLink>(true);
            var offMeshLinks = new List<OffMeshLink>(arr);
            for (var i = offMeshLinks.Count - 1; i >= 0; i--)
            {
                var gameObject = offMeshLinks[i].gameObject;
                if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) && gameObject != rootGameObject)
                    offMeshLinks.RemoveAt(i);
            }
            return offMeshLinks;
        }

        static void ConvertPrefab(GameObject prefabRoot, Dictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var offMeshLinks = FindAllOffMeshLinksInHierarchy(prefabRoot);
            var updatedPrefab = false;
            foreach (var offMeshLink in offMeshLinks)
            {
                var gameObject = offMeshLink.gameObject;
                var isPrefabVariant = PrefabUtility.IsPartOfVariantPrefab(gameObject);
                // If the prefab is a variant and the OffMeshLink is not an override,
                // only store the variant's OffMeshLink and NavMeshLink pair, and skip the rest of the conversion.
                if (isPrefabVariant && !PrefabUtility.IsAddedComponentOverride(offMeshLink))
                {
                    var sourceOml = PrefabUtility.GetCorrespondingObjectFromSource(offMeshLink);
                    var sourceNML = offMeshLinkToNavMeshLink[sourceOml];
                    var variantNML = GetCorrespondingVariantComponent(sourceNML, prefabRoot);
                    offMeshLinkToNavMeshLink.Add(offMeshLink, variantNML);
                    continue;
                }

                var navMeshLink = gameObject.AddComponent<NavMeshLink>();
                offMeshLinkToNavMeshLink.Add(offMeshLink, navMeshLink);

                // If the OffMeshLink is an override (created by a Prefab Variant), record the new NavMeshLink as an override.
                if (isPrefabVariant)
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);

                CopyValues(offMeshLink, navMeshLink);
                updatedPrefab = true;
            }

            if (updatedPrefab)
                PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        static void ConvertScene(Scene scene, ref Dictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var rootGameObject in rootGameObjects)
            {
                // Skip prefab instances, they are taken care of in the ConvertPrefab method.
                if (PrefabUtility.IsAnyPrefabInstanceRoot(rootGameObject))
                    continue;

                var offMeshLinks = FindAllOffMeshLinksInHierarchy(rootGameObject);
                foreach (var offMeshLink in offMeshLinks)
                {
                    var navMeshLink = offMeshLink.gameObject.AddComponent<NavMeshLink>();
                    offMeshLinkToNavMeshLink.Add(offMeshLink, navMeshLink);

                    CopyValues(offMeshLink, navMeshLink);
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        static bool CanWriteToAsset(string filePath)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            return !fileInfo.Attributes.HasFlag(System.IO.FileAttributes.ReadOnly);
        }

        static bool DoesAssetExistOnDisk(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath);
        }

        static void CopyValues(OffMeshLink offMeshLink, NavMeshLink navMeshLink)
        {
            navMeshLink.activated = offMeshLink.activated;
            navMeshLink.autoUpdate = offMeshLink.autoUpdatePositions;
            navMeshLink.bidirectional = offMeshLink.biDirectional;
            navMeshLink.costModifier = offMeshLink.costOverride;
            navMeshLink.startTransform = offMeshLink.startTransform;
            navMeshLink.endTransform = offMeshLink.endTransform;
        }

        /// <summary>
        /// Transfers any existing override data from OffMeshLink components to NavMeshLink components.
        /// </summary>
        /// <param name="outerPrefabRoot">The out-most prefab root</param>
        /// <param name="offMeshLinkToNavMeshLink">Dictionary storing the OffMeshLink and NavMeshLink pairs</param>
        static void ApplyOverrideDataToPrefab(GameObject outerPrefabRoot, Dictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var didUpdate = false;
            var updatedRoots = new HashSet<GameObject>();
            var offMeshLinks = outerPrefabRoot.GetComponentsInChildren<OffMeshLink>(true);
            foreach (var offMeshLink in offMeshLinks)
            {
                // Find the prefab root and check if we have already updated it.
                // A null root means that the root is the outerPrefabRoot.
                var rootGameObject = PrefabUtility.GetNearestPrefabInstanceRoot(offMeshLink.gameObject);
                rootGameObject = rootGameObject == null ? offMeshLink.gameObject : rootGameObject;
                if (updatedRoots.Contains(rootGameObject))
                    continue;

                var propertyModifications = PrefabUtility.GetPropertyModifications(rootGameObject);
                if (propertyModifications == null)
                    continue;

                UpdatePropertyModifications(ref propertyModifications, offMeshLinkToNavMeshLink);
                PrefabUtility.SetPropertyModifications(rootGameObject, propertyModifications);

                updatedRoots.Add(rootGameObject);
                didUpdate = true;
            }

            if (didUpdate)
                PrefabUtility.SavePrefabAsset(outerPrefabRoot);
        }

        /// <summary>
        /// Removes the NavMeshLink components that correspond to OffMeshLink components that were removed from the variant prefab.
        /// </summary>
        static void SyncOverriddenRemovalsOfComponents(GameObject prefabRoot, IReadOnlyDictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var isPrefabVariant = PrefabUtility.IsPartOfVariantPrefab(prefabRoot);
            if (!isPrefabVariant)
                return;

            var removedComponents = PrefabUtility.GetRemovedComponents(prefabRoot);
            var variantNavMeshLinks = prefabRoot.GetComponents<NavMeshLink>();
            foreach (var removedComponent in removedComponents)
            {
                if (removedComponent.assetComponent is not OffMeshLink removedOffMeshLink)
                    continue;

                var sourceNavMeshLink = offMeshLinkToNavMeshLink[removedOffMeshLink];
                foreach(var variantComponent in variantNavMeshLinks)
                {
                    var correspondingComponent = PrefabUtility.GetCorrespondingObjectFromSource(variantComponent);
                    if (correspondingComponent == sourceNavMeshLink)
                    {
                        Object.DestroyImmediate(variantComponent, true);
                        break;
                    }
                }
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(prefabRoot);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        static void ApplyOverrideDataToScene(Scene scene, Dictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            var updatedRoots = new HashSet<GameObject>();
            foreach (var rootGameObject in rootGameObjects)
            {
                var offMeshLinks = rootGameObject.GetComponentsInChildren<OffMeshLink>();
                foreach (var offMeshLink in offMeshLinks)
                {
                    // Non-prefab instances cannot have overrides, so continue.
                    if (!PrefabUtility.IsPartOfAnyPrefab(offMeshLink))
                        continue;
                    // Find the prefab root and check if it contains any overrides
                    var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(offMeshLink.gameObject);
                    if (!PrefabUtility.HasPrefabInstanceAnyOverrides(prefabRoot, false) && !updatedRoots.Contains(prefabRoot))
                        continue;

                    var propertyModifications = PrefabUtility.GetPropertyModifications(prefabRoot);

                    UpdatePropertyModifications(ref propertyModifications, offMeshLinkToNavMeshLink);
                    PrefabUtility.SetPropertyModifications(prefabRoot, propertyModifications);
                    updatedRoots.Add(prefabRoot);
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        static void UpdatePropertyModifications(ref PropertyModification[] propertyModifications, Dictionary<OffMeshLink, NavMeshLink> offMeshLinkToNavMeshLink)
        {
            var updatedModifications = new List<PropertyModification>(propertyModifications);
            for (var i = 0; i < propertyModifications.Length; ++i)
            {
                var modification = propertyModifications[i];
                // Skip if the target is not an OffMeshLink
                if (modification.target is not OffMeshLink oml)
                {
                    updatedModifications.Add(modification);
                    continue;
                }

                // Get the NavMeshLink that corresponds to the OffMeshLink.
                var navMeshLink = offMeshLinkToNavMeshLink[oml];
                var newMod = new PropertyModification
                {
                    target = navMeshLink,
                    value = modification.value
                };
                switch (modification.propertyPath)
                {
                    case "m_AutoUpdatePositions":
                        newMod.propertyPath = "m_AutoUpdatePosition";
                        break;
                    case "m_CostOverride":
                        newMod.propertyPath = "m_CostModifier";
                        if (float.TryParse(newMod.value, out var floatVal))
                        {
                            if (floatVal > 0)
                            {
                                var overrideMod = new PropertyModification()
                                {
                                    target = navMeshLink,
                                    propertyPath = "m_IsOverridingCost",
                                    value = "1"
                                };
                                updatedModifications.Add(overrideMod);
                            }
                        }
                        break;
                    case "m_BiDirectional":
                        newMod.propertyPath = "m_Bidirectional";
                        break;
                    case "m_Start":
                        newMod.propertyPath = "m_StartTransform";
                        break;
                    case "m_End":
                        newMod.propertyPath = "m_EndTransform";
                        break;
                    case "m_Activated":
                        newMod.propertyPath = modification.propertyPath;
                        break;
                }
                updatedModifications.Add(newMod);
            }
            propertyModifications = updatedModifications.ToArray();
        }

        /// <summary>
        /// Get the corresponding variant component of a source prefab component.
        /// </summary>
        /// <param name="component">The source prefab component</param>
        /// <param name="variantPrefab">The variant prefab</param>
        /// <typeparam name="TComponent">The type of component to search for</typeparam>
        /// <returns>Returns the corresponding component on the variant of the source prefab component, if found, or null.</returns>
        static TComponent GetCorrespondingVariantComponent<TComponent>(TComponent component, GameObject variantPrefab) where TComponent : Component
        {
            var variantComponents = variantPrefab.GetComponents(component.GetType());
            foreach (var variantComponent in variantComponents)
            {
                var correspondingComponent = PrefabUtility.GetCorrespondingObjectFromSource(variantComponent);
                if (correspondingComponent == component)
                    return variantComponent as TComponent;
            }
            return null;
        }
    }
#pragma warning restore 618
}

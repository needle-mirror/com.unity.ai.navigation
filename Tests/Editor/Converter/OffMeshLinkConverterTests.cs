using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AI.Navigation.Updater;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Unity.AI.Navigation.Editor.Tests
{
#pragma warning disable 618
    internal class OffMeshLinkConverterTests
    {
        string m_TestFolderName;
        string m_TestFolderPath;

        struct OffMeshLinkData
        {
            public bool activated;
            public bool autoUpdatePositions;
            public bool biDirectional;
            public float costOverride;
            public Transform startTransform;
            public Transform endTransform;
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(m_TestFolderPath) && AssetDatabase.IsValidFolder(m_TestFolderPath))
            {
                AssetDatabase.DeleteAsset(m_TestFolderPath);
                AssetDatabase.Refresh();
            }
        }

        [SetUp]
        public void SetUp()
        {
            m_TestFolderName = System.IO.Path.GetRandomFileName();
            AssetDatabase.CreateFolder("Assets", m_TestFolderName);
            m_TestFolderPath = "Assets/" + m_TestFolderName;
        }

        [Test]
        public void Convert_InvalidGuid_NoConversion()
        {
            const string invalidGuid = "InvalidGuid";
            var convertList = new List<string> { invalidGuid };
            OffMeshLinkUpdaterUtility.Convert(convertList, out var failedConversions);

            Assert.That(failedConversions.Count, Is.EqualTo(1), "Should have failed to convert the invalid GUID.");
        }

        [Test]
        public void FindObjectsToConvert_PrefabWithOML_ConverterFindsPrefab()
        {
            var omlPrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            Assume.That(omlPrefab.GetComponent<OffMeshLink>(), Is.Not.Null, "Prefab should have OffMeshLink component");

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            Assert.That(foundPrefabs.Count, Is.EqualTo(1), "Should have found 1 prefab with OffMeshLink component.");
            Assert.That(GuidToPrefab(foundPrefabs[0]) == omlPrefab, "Should have found the prefab with OffMeshLink component.");
        }

        [Test]
        public void FindObjectsToConvert_PrefabWithoutOML_ConverterDoesNotFindPrefab()
        {
            CreatePrefabWithComponent<SpriteRenderer>(m_TestFolderPath + "/SpriteRendererPrefab.prefab");

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            Assert.That(foundPrefabs.Count, Is.EqualTo(0), "Should not have found any prefabs with OffMeshLink component.");
        }

        [Test]
        public void FindObjectsToConvert_ConverterLookingInEmptyFolder_NoItemsFound()
        {
            CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            var randomFolderName = System.IO.Path.GetRandomFileName();
            var randomFolderPath = "Assets/" + randomFolderName;
            AssetDatabase.CreateFolder("Assets", randomFolderName);
            try
            {
                var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { randomFolderPath });
                Assert.That(foundPrefabs.Count, Is.EqualTo(0), "Should not have found any prefabs with OffMeshLink component.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(randomFolderPath);
            }

        }

        [Test]
        public void Convert_PrefabIsReadOnly_NoConversion()
        {
            var prefabPath = m_TestFolderPath + "/OffMeshLinkPrefab.prefab";
            CreatePrefabWithComponent<OffMeshLink>(prefabPath);
            var attributes = System.IO.File.GetAttributes(prefabPath);
            System.IO.File.SetAttributes(prefabPath, attributes | System.IO.FileAttributes.ReadOnly);

            var foundPrefabs = new List<string> { AssetDatabase.AssetPathToGUID(prefabPath) };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out var failedConversions);

            Assert.That(foundPrefabs.Count, Is.Zero, "Prefab should have been removed from conversion list.");
            Assert.That(failedConversions.Count, Is.EqualTo(1), "Should have failed to convert the prefab.");
            Assert.That(failedConversions[0].failureMessage, Is.Not.Empty, "Should have a failure message.");
        }

        [Test]
        public void FindObjectsToConvert_PrefabWithOMLOnChild_ConverterFindsPrefab()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.parent = parent.transform;
            child.AddComponent<OffMeshLink>();
            CreatePrefabFromGameObject(parent, m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            Assert.That(foundPrefabs.Count, Is.EqualTo(1), "Should have found 1 prefab with OffMeshLink component.");
        }

        [Test]
        public void Convert_PrefabWithOML_PrefabConverted()
        {
            CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var convertedPrefab = GuidToPrefab(foundPrefabs[0]);
            Assert.That(convertedPrefab.GetComponent<OffMeshLink>(), Is.Null, "Prefab should not have OffMeshLink component after conversion.");
            Assert.That(convertedPrefab.GetComponent<NavMeshLink>(), Is.Not.Null, "Prefab should have NavMeshLink component after conversion.");
        }

        [Test]
        public void Convert_PrefabWithOMLOnChild_ChildConverted()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.parent = parent.transform;
            child.AddComponent<OffMeshLink>();
            CreatePrefabFromGameObject(parent, m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            var objectsToConvert = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab") };
            OffMeshLinkUpdaterUtility.Convert(objectsToConvert, out _);

            var convertedPrefab = GuidToPrefab(objectsToConvert[0]);
            Assert.That(convertedPrefab.transform.GetChild(0).GetComponent<OffMeshLink>(), Is.Null, "Prefab should not have OffMeshLink component after conversion.");
            Assert.That(convertedPrefab.transform.GetChild(0).GetComponent<NavMeshLink>(), Is.Not.Null, "Prefab should have NavMeshLink component after conversion.");
        }

        [Test]
        public void FindObjectsToConvert_SceneWithOML_ConverterFindsScene()
        {
            var scene = CreateSceneWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkScene.unity");

            var gameObjects = scene.GetRootGameObjects();
            var omlGameObject = Array.Find(gameObjects, x => x.TryGetComponent<OffMeshLink>(out _));
            Assume.That(omlGameObject, Is.Not.Null, "A GameObject in the scene should have an OffMeshLink component.");

            var foundObjects = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            Assert.That(foundObjects.Count, Is.EqualTo(1), "Should have found 1 scene with OffMeshLink component.");
        }

        [Test]
        public void FindObjectsToConvert_SceneWithPrefabInstance_ConverterFindsCorrectNumberToConvert()
        {
            CreateSceneWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkScene.unity");
            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            // Open the scene and instantiate the prefab
            var scene = EditorSceneManager.OpenScene(m_TestFolderPath + "/OffMeshLinkScene.unity", OpenSceneMode.Additive);
            PrefabUtility.InstantiatePrefab(prefab);
            EditorSceneManager.SaveScene(scene);

            var foundObjects = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            Assert.That(foundObjects.Count, Is.EqualTo(2), "Should have found 2 objects to convert.");
        }

        [Test]
        public void Convert_SceneWithOML_InstanceInSceneConverted()
        {
            CreateSceneWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkScene.unity");
            var foundObjects = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            OffMeshLinkUpdaterUtility.Convert(foundObjects, out _);

            var pathToObject = AssetDatabase.GUIDToAssetPath(foundObjects[0]);
            var scene = EditorSceneManager.OpenScene(pathToObject, OpenSceneMode.Additive);
            var convertedGameObject = scene.GetRootGameObjects()[0];

            Assert.That(convertedGameObject.transform.GetComponent<OffMeshLink>(), Is.Null, "Prefab should not have OffMeshLink component after conversion.");
            Assert.That(convertedGameObject.transform.GetComponent<NavMeshLink>(), Is.Not.Null, "Prefab should have NavMeshLink component after conversion.");
        }

        [Test]
        public void Convert_SceneIsReadOnly_NoConversion()
        {
            var scenePath = m_TestFolderPath + "/OffMeshLinkScene.unity";
            CreateSceneWithComponent<OffMeshLink>(scenePath);
            var attributes = System.IO.File.GetAttributes(scenePath);
            System.IO.File.SetAttributes(scenePath, attributes | System.IO.FileAttributes.ReadOnly);

            var foundItems = new List<string> { AssetDatabase.AssetPathToGUID(scenePath) };
            OffMeshLinkUpdaterUtility.Convert(foundItems, out var failedConversions);

            Assert.That(foundItems.Count, Is.Zero, "Scene should have been removed from conversion list.");
            Assert.That(failedConversions.Count, Is.EqualTo(1), "Should have failed to convert the scene.");
            Assert.That(failedConversions[0].failureMessage, Is.Not.Empty, "Should have a failure message.");
        }

        [Test]
        public void Convert_SceneWithOMLOnChild_ChildConverted()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneManager.SetActiveScene(scene);

            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.parent = parent.transform;
            child.AddComponent<OffMeshLink>();

            EditorSceneManager.SaveScene(scene, m_TestFolderPath + "/OffMeshLinkScene.unity");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var foundObjects = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkScene.unity") };
            OffMeshLinkUpdaterUtility.Convert(foundObjects, out _);

            var pathToObject = AssetDatabase.GUIDToAssetPath(foundObjects[0]);
            scene = EditorSceneManager.OpenScene(pathToObject, OpenSceneMode.Additive);
            var convertedGameObject = scene.GetRootGameObjects()[0];

            Assert.That(convertedGameObject.transform.GetChild(0).GetComponent<OffMeshLink>(), Is.Null, "GameObject should not have OffMeshLink component after conversion.");
            Assert.That(convertedGameObject.transform.GetChild(0).GetComponent<NavMeshLink>(), Is.Not.Null, "GameObject should have NavMeshLink component after conversion.");
        }

        [Test]
        public void Convert_SceneWithPrefabInstance_PrefabAndInstanceConverted()
        {
            CreateSceneWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkScene.unity");
            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            // Open the scene and instantiate the prefab
            var scene = EditorSceneManager.OpenScene(m_TestFolderPath + "/OffMeshLinkScene.unity", OpenSceneMode.Additive);
            PrefabUtility.InstantiatePrefab(prefab);
            EditorSceneManager.SaveScene(scene);

            var foundObjects = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkScene.unity")
            };
            OffMeshLinkUpdaterUtility.Convert(foundObjects, out _);

            // Validate the converted prefab
            var convertedPrefab = GuidToPrefab(foundObjects[0]);
            Assume.That(convertedPrefab.GetComponent<OffMeshLink>(), Is.Null, "Prefab should not have OffMeshLink component after conversion.");
            Assume.That(convertedPrefab.GetComponent<NavMeshLink>(), Is.Not.Null, "Prefab should have NavMeshLink component after conversion.");
            Assert.That(convertedPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Prefab should have only one NavMeshLink component after conversion.");

            // Validate the converted scene
            scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(foundObjects[1]), OpenSceneMode.Additive);
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var sceneGo in rootGameObjects)
            {
                Assume.That(sceneGo.GetComponent<OffMeshLink>(), Is.Null, $"{sceneGo.name} should not have OffMeshLink component after conversion.");
                Assume.That(sceneGo.GetComponent<NavMeshLink>(), Is.Not.Null, $"{sceneGo.name} Prefab should have NavMeshLink component after conversion.");
                Assert.That(sceneGo.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), $"{sceneGo.name} Prefab should have only one NavMeshLink component after conversion.");
            }
        }

        [Test]
        public void Convert_PrefabWithCustomValues_ValuesAreCopiedOver()
        {
            var testData = new OffMeshLinkData()
            {
                activated = false,
                autoUpdatePositions = false,
                biDirectional = false,
                costOverride = 123f
            };

            var gameObject = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var oml = gameObject.GetComponent<OffMeshLink>();
            oml.activated = testData.activated;
            oml.autoUpdatePositions = testData.autoUpdatePositions;
            oml.biDirectional = testData.biDirectional;
            oml.costOverride = testData.costOverride;
            oml.startTransform = testData.startTransform;
            oml.endTransform = testData.endTransform;

            var foundPrefabs = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab") };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var convertedPrefab = GuidToPrefab(foundPrefabs[0]);
            AssertValuesAreEqual(testData, convertedPrefab.GetComponent<NavMeshLink>());
        }

        [Test]
        public void Convert_NestedPrefabWithCustomValuesOnChild_ValuesAreCopiedOver([Values(true, false)] bool shouldFlipPrefabOrder)
        {
            var testData = new OffMeshLinkData()
            {
                activated = false,
                autoUpdatePositions = false,
                biDirectional = false,
                costOverride = 123f
            };

            var parentPrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/ParentPrefab.prefab");
            var childPrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/ChildPrefab.prefab");

            NestPrefabs(parentPrefab, childPrefab);

            var oml = childPrefab.GetComponent<OffMeshLink>();
            oml.activated = testData.activated;
            oml.autoUpdatePositions = testData.autoUpdatePositions;
            oml.biDirectional = testData.biDirectional;
            oml.costOverride = testData.costOverride;
            oml.startTransform = testData.startTransform;
            oml.endTransform = testData.endTransform;

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            if (shouldFlipPrefabOrder)
                foundPrefabs.Reverse();

            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);
            var convertedPrefabs = GuidsToPrefabs(foundPrefabs);

            var convertedPrefab = convertedPrefabs.Find(x => x.name == childPrefab.name);
            Assume.That(convertedPrefab, Is.Not.Null, "Could not find child prefab.");
            AssertValuesAreEqual(testData, convertedPrefab.GetComponent<NavMeshLink>());
        }

        [Test]
        public void Convert_NestedPrefabWithOverriddenValues_ValuesAreOverriddenAfterConversion()
        {
            const float overrideValue = 321f;

            var parentPrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/ParentPrefab.prefab");
            var childPrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/ChildPrefab.prefab");

            NestPrefabs(parentPrefab, childPrefab);

            var parentInstance = PrefabUtility.InstantiatePrefab(parentPrefab) as GameObject;
            var childInstance = parentInstance.transform.GetChild(0);

            var oml = childInstance.GetComponent<OffMeshLink>();
            oml.costOverride = overrideValue;
            oml.activated = false;
            oml.autoUpdatePositions = false;
            oml.biDirectional = false;
            oml.startTransform = oml.transform;
            oml.endTransform = oml.transform;

            PrefabUtility.ApplyPrefabInstance(parentInstance, InteractionMode.AutomatedAction);

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/ParentPrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/ChildPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);
            var convertedPrefabs = GuidsToPrefabs(foundPrefabs);

            var convertedParentPrefab = convertedPrefabs.Find(x => x.name == parentPrefab.name);
            var convertedChildPrefab = convertedParentPrefab.transform.GetChild(0).gameObject;

            var hasAnyOverrides = PrefabUtility.HasPrefabInstanceAnyOverrides(convertedChildPrefab, false);
            Assume.That(hasAnyOverrides, Is.True, "Child prefab should have overrides.");

            var propertyModifications = PrefabUtility.GetPropertyModifications(convertedChildPrefab);
            Assume.That(propertyModifications.Length, Is.GreaterThan(0), "Child prefab should have property modifications.");

            var propertyModification = Array.Find(propertyModifications, x => x.propertyPath == "m_CostModifier" && x.target?.GetType() == typeof(NavMeshLink));
            Assume.That(propertyModification, Is.Not.Null, "Property modification for cost override should exist.");
            Assert.That(propertyModification.value, Is.EqualTo(overrideValue.ToString()), "Property modification value should be equal to the override value.");
        }

        [Test]
        public void Convert_SceneWithOverriddenPrefabInstance_ValuesAreOverriddenAfterConversion()
        {
            const float overrideValue = 42f;

            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            CreateSceneWithPrefabInstance(prefab, m_TestFolderPath + "/OffMeshLinkScene.unity");

            // Open the scene and override prefab instance values
            var scene = EditorSceneManager.OpenScene(m_TestFolderPath + "/OffMeshLinkScene.unity", OpenSceneMode.Additive);
            var rootGameObjects = scene.GetRootGameObjects();
            var prefabInstance = Array.Find(rootGameObjects, x => x.GetComponent<OffMeshLink>());
            var oml = prefabInstance.GetComponent<OffMeshLink>();
            oml.costOverride = overrideValue;
            oml.activated = false;
            oml.autoUpdatePositions = false;
            oml.biDirectional = false;
            oml.startTransform = oml.transform;
            oml.endTransform = oml.transform;
            EditorSceneManager.SaveScene(scene);

            var foundObjects = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkScene.unity")
            };
            OffMeshLinkUpdaterUtility.Convert(foundObjects, out _);

            Assume.That(foundObjects.Count, Is.EqualTo(2));

            scene = EditorSceneManager.OpenScene(m_TestFolderPath + "/OffMeshLinkScene.unity", OpenSceneMode.Additive);
            rootGameObjects = scene.GetRootGameObjects();
            prefabInstance = Array.Find(rootGameObjects, x => x.GetComponent<NavMeshLink>());

            var hasAnyOverrides = PrefabUtility.HasPrefabInstanceAnyOverrides(prefabInstance, false);
            Assume.That(hasAnyOverrides, Is.True, "Prefab instance should have overrides.");

            var propertyModifications = PrefabUtility.GetPropertyModifications(prefabInstance);
            Assume.That(propertyModifications.Length, Is.GreaterThan(0), "Prefab instance should have property modifications.");

            var propertyModification = Array.Find(propertyModifications, x => x.propertyPath == "m_CostModifier" && x.target?.GetType() == typeof(NavMeshLink));
            Assume.That(propertyModification, Is.Not.Null, "Property modification for cost override should exist.");
            Assert.That(propertyModification.value, Is.EqualTo(overrideValue.ToString()), "Property modification value should be equal to the override value.");
        }

        [Test]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(16)]
        public void Convert_PrefabWithMultipleOML_ConvertedToMultipleNML(int noOfComponets)
        {
            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            // Start at 1 since we already have one component
            for (var i = 1; i < noOfComponets; ++i)
                prefab.AddComponent<OffMeshLink>();
            PrefabUtility.SavePrefabAsset(prefab);

            var foundPrefabs = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab") };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var convertedPrefab = GuidToPrefab(foundPrefabs[0]);
            Assert.That(convertedPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(noOfComponets), "Should have converted all OffMeshLink components to NavMeshLink components.");
            Assert.That(convertedPrefab.GetComponents<OffMeshLink>().Length, Is.Zero, "Should not have any OffMeshLink components left.");
        }

        [Test]
        public void Convert_PrefabWithOMLAndNML_PrefabConvertedAndHasOriginalNML()
        {
            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            prefab.AddComponent<NavMeshLink>();
            PrefabUtility.SavePrefabAsset(prefab);

            var foundPrefabs = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab") };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var convertedPrefab = GuidToPrefab(foundPrefabs[0]);
            Assert.That(convertedPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(2), "Should have converted all OffMeshLink components to NavMeshLink components.");
            Assert.That(convertedPrefab.GetComponents<OffMeshLink>().Length, Is.Zero, "Should not have any OffMeshLink components left.");
        }

        [Test]
        public void Convert_PrefabWithOMLAndNML_ValuesAreCopiedOverToCorrectNML()
        {
            var testData = new OffMeshLinkData()
            {
                activated = false,
                autoUpdatePositions = false,
                biDirectional = false,
                costOverride = 123f
            };

            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var oml = prefab.GetComponent<OffMeshLink>();
            oml.activated = testData.activated;
            oml.autoUpdatePositions = testData.autoUpdatePositions;
            oml.biDirectional = testData.biDirectional;
            oml.costOverride = testData.costOverride;
            oml.startTransform = testData.startTransform;
            oml.endTransform = testData.endTransform;

            var existingNml = prefab.AddComponent<NavMeshLink>();

            PrefabUtility.SavePrefabAsset(prefab);

            var foundPrefabs = new List<string> { AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab") };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var convertedPrefab = GuidToPrefab(foundPrefabs[0]);
            var links = convertedPrefab.GetComponents<NavMeshLink>();
            var convertedNml = Array.Find(links, x => x != existingNml);
            AssertValuesAreEqual(testData, convertedNml);
        }

        [Test]
        public void Convert_VariantPrefabWithOml_VariantConverted()
        {
            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            var convertedSourcePrefab = prefabs.Find(x => !PrefabUtility.IsPartOfVariantPrefab(x));
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been converted.");

            Assert.That(convertedVariantPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Variant prefab should have one NavMeshLink component.");
            Assert.That(convertedSourcePrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Source prefab should have one NavMeshLink component.");
        }

        [Test]
        public void Convert_VariantPrefabWithOverrides_ValuesAreOverriddenAfterConversion()
        {
            const float costOverride = 42f;

            var prefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            variantPrefab.GetComponent<OffMeshLink>().costOverride = costOverride;
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            Assume.That(prefab.GetComponent<OffMeshLink>().costOverride, Is.EqualTo(-1), "Source prefab should have a cost override equal to -1.");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            var convertedSourcePrefab = prefabs.Find(x => !PrefabUtility.IsPartOfVariantPrefab(x));
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been converted.");
            Assume.That(convertedVariantPrefab.GetComponent<NavMeshLink>(), Is.Not.Null, "Variant prefab should have NavMeshLink component.");

            Assert.That(convertedVariantPrefab.GetComponent<NavMeshLink>().costModifier, Is.EqualTo(costOverride), $"Variant prefab should have a cost modifier equal to {costOverride}.");
            Assert.That(convertedSourcePrefab.GetComponent<NavMeshLink>().costModifier, Is.EqualTo(-1), "Source prefab should have a cost modifier equal to -1.");
        }

        [Test]
        public void Convert_VariantPrefabWithAddedOML_VariantConvertedWithAddedNMLAndOriginalNML()
        {
            var sourcePrefab = CreatePrefabFromGameObject(new GameObject(), m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            variantPrefab.AddComponent<OffMeshLink>();
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been converted.");
            Assert.That(convertedVariantPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Variant prefab should have one NavMeshLink components.");
        }

        [Test]
        public void FindObjectsToConvert_VariantPrefabRemovedOML_ConverterFindsVariant()
        {
            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            Object.DestroyImmediate(variantPrefab.GetComponent<OffMeshLink>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = OffMeshLinkUpdaterUtility.FindObjectsToConvert(new[] { m_TestFolderPath });
            var prefabs = GuidsToPrefabs(foundPrefabs);
            var foundVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            Assume.That(foundPrefabs.Count, Is.EqualTo(2), "Should have found 2 prefabs.");
            Assert.That(foundVariantPrefab, Is.Not.Null, "Variant prefab should have been found.");
        }

        [Test]
        public void Convert_VariantPrefabRemovedOML_SourceConvertedToNML()
        {
            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            Object.DestroyImmediate(variantPrefab.GetComponent<OffMeshLink>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedSourcePrefab = prefabs.Find(x => !PrefabUtility.IsPartOfVariantPrefab(x));
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been found.");
            Assume.That(convertedSourcePrefab, Is.Not.Null, "Source prefab should have been found.");
            Assert.That(convertedVariantPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(0), "Variant prefab should not have any NavMeshLink components.");
            Assert.That(convertedSourcePrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Source prefab should have one NavMeshLink components.");
        }

        [Test]
        public void Convert_VariantPrefabRemovedOneKeptOneOML_ConvertedWithOneNML()
        {
            const float costOverrideValue = 1234f;

            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var secondaryOml = sourcePrefab.AddComponent<OffMeshLink>();
            secondaryOml.costOverride = costOverrideValue;

            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            var omlToDestroy = Array.Find(variantPrefab.GetComponents<OffMeshLink>(), x => !Mathf.Approximately(x.costOverride, costOverrideValue));
            Object.DestroyImmediate(omlToDestroy);
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been found.");

            var convertedNavMeshLink = convertedVariantPrefab.GetComponent<NavMeshLink>();
            Assume.That(convertedNavMeshLink, Is.Not.Null, "Variant prefab should have one NavMeshLink components.");

            Assert.That(convertedNavMeshLink.costModifier, Is.EqualTo(costOverrideValue).Within(Mathf.Epsilon), "Cost modifier should be equal to the cost override value.");
        }

        [Test]
        public void Convert_VariantPrefabRemovedOMLAndAddNewOML_ConvertedWithOneNML()
        {
            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");
            var variantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            Object.DestroyImmediate(variantPrefab.GetComponent<OffMeshLink>());
            variantPrefab.AddComponent<OffMeshLink>();
            PrefabUtility.RecordPrefabInstancePropertyModifications(variantPrefab);
            PrefabUtility.SaveAsPrefabAsset(variantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariant.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };
            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedSourcePrefab = prefabs.Find(x => !PrefabUtility.IsPartOfVariantPrefab(x));
            var convertedVariantPrefab = prefabs.Find(PrefabUtility.IsPartOfVariantPrefab);
            Assume.That(convertedVariantPrefab, Is.Not.Null, "Variant prefab should have been found.");
            Assume.That(convertedSourcePrefab, Is.Not.Null, "Source prefab should have been found.");
            Assert.That(convertedVariantPrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Variant prefab should have one NavMeshLink components.");
            Assert.That(convertedSourcePrefab.GetComponents<NavMeshLink>().Length, Is.EqualTo(1), "Source prefab should have one NavMeshLink components.");
        }

        [Test]
        public void Convert_TwoVariantsWithOverrides_VariantsKeepTheirOverridesAfterConversion()
        {
            const float firstCostOverride = 42f;
            const float secondCostOverride = 1234f;

            var sourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/OffMeshLinkPrefab.prefab");

            var firstVariantPrefab = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            firstVariantPrefab.GetComponent<OffMeshLink>().costOverride = firstCostOverride;
            PrefabUtility.RecordPrefabInstancePropertyModifications(firstVariantPrefab);
            var firstVariantPrefabAsset = PrefabUtility.SaveAsPrefabAsset(firstVariantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariantOne.prefab");

            var secondVariantPrefab = PrefabUtility.InstantiatePrefab(firstVariantPrefabAsset) as GameObject;
            secondVariantPrefab.GetComponent<OffMeshLink>().costOverride = secondCostOverride;
            PrefabUtility.RecordPrefabInstancePropertyModifications(secondVariantPrefab);
            PrefabUtility.SaveAsPrefabAsset(secondVariantPrefab, m_TestFolderPath + "/OffMeshLinkPrefabVariantTwo.prefab");

            Assume.That(sourcePrefab.GetComponent<OffMeshLink>().costOverride, Is.EqualTo(-1), "Source prefab should have a cost override equal to -1.");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariantOne.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefabVariantTwo.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/OffMeshLinkPrefab.prefab")
            };

            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var convertedVariantPrefabOne = prefabs.Find(x => x.name == "OffMeshLinkPrefabVariantOne");
            var convertedVariantPrefabTwo = prefabs.Find(x => x.name == "OffMeshLinkPrefabVariantTwo");
            var convertedSourcePrefab = prefabs.Find(x => x.name == "OffMeshLinkPrefab");

            Assume.That(convertedVariantPrefabOne, Is.Not.Null, "Variant prefab one should have been found.");
            Assume.That(convertedVariantPrefabTwo, Is.Not.Null, "Variant prefab two should have been found.");
            Assume.That(convertedSourcePrefab, Is.Not.Null, "Source prefab should have been found.");
            Assume.That(convertedVariantPrefabOne.GetComponent<NavMeshLink>(), Is.Not.Null, "Variant prefab one should have a NavMeshLink component.");
            Assume.That(convertedVariantPrefabTwo.GetComponent<NavMeshLink>(), Is.Not.Null, "Variant prefab two should have a NavMeshLink component.");
            Assume.That(convertedSourcePrefab.GetComponent<NavMeshLink>(), Is.Not.Null, "Source prefab should have a NavMeshLink component.");

            Assert.That(convertedVariantPrefabOne.GetComponent<NavMeshLink>().costModifier, Is.EqualTo(firstCostOverride), $"Variant prefab one should have a cost modifier equal to {firstCostOverride}.");
            Assert.That(convertedVariantPrefabTwo.GetComponent<NavMeshLink>().costModifier, Is.EqualTo(secondCostOverride), $"Variant prefab two should have a cost modifier equal to {secondCostOverride}.");
            Assert.That(convertedSourcePrefab.GetComponent<NavMeshLink>().costModifier, Is.EqualTo(-1), "Source prefab should have a cost modifier equal to -1.");
        }

        [Test]
        public void Convert_TwoVariantsSameSource_VariantsAreSuccessfullyConverted()
        {
            var topSourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/TopSourcePrefab.prefab");

            var midSourcePrefab = PrefabUtility.InstantiatePrefab(topSourcePrefab) as GameObject;
            var midSourcePrefabAsset = PrefabUtility.SaveAsPrefabAsset(midSourcePrefab, m_TestFolderPath + "/MidSourcePrefab.prefab");

            var firstLeafPrefab = PrefabUtility.InstantiatePrefab(midSourcePrefabAsset) as GameObject;
            PrefabUtility.SaveAsPrefabAsset(firstLeafPrefab, m_TestFolderPath + "/FirstLeafPrefab.prefab");

            var secondLeafPrefab = PrefabUtility.InstantiatePrefab(midSourcePrefabAsset) as GameObject;
            PrefabUtility.SaveAsPrefabAsset(secondLeafPrefab, m_TestFolderPath + "/SecondLeafPrefab.prefab");

            var foundPrefabs = new List<string>
            {
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/FirstLeafPrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/MidSourcePrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/SecondLeafPrefab.prefab"),
                AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/TopSourcePrefab.prefab")
            };

            OffMeshLinkUpdaterUtility.Convert(foundPrefabs, out _);

            var prefabs = GuidsToPrefabs(foundPrefabs);
            var firstLeaf = prefabs.Find(x => x.name == "FirstLeafPrefab");
            var secondLeaf = prefabs.Find(x => x.name == "SecondLeafPrefab");
            var midSource = prefabs.Find(x => x.name == "MidSourcePrefab");
            var topSource = prefabs.Find(x => x.name == "TopSourcePrefab");

            Assume.That(firstLeaf, Is.Not.Null, "First leaf prefab should have been found.");
            Assume.That(secondLeaf, Is.Not.Null, "Second leaf prefab should have been found.");
            Assume.That(midSource, Is.Not.Null, "Mid source prefab should have been found.");
            Assume.That(topSource, Is.Not.Null, "Top source prefab should have been found.");
            Assert.That(firstLeaf.GetComponent<NavMeshLink>(), Is.Not.Null, "First leaf prefab should have a NavMeshLink component.");
            Assert.That(secondLeaf.GetComponent<NavMeshLink>(), Is.Not.Null, "Second leaf prefab should have a NavMeshLink component.");
            Assert.That(midSource.GetComponent<NavMeshLink>(), Is.Not.Null, "Mid source prefab should have a NavMeshLink component.");
            Assert.That(topSource.GetComponent<NavMeshLink>(), Is.Not.Null, "Top source prefab should have a NavMeshLink component.");
        }

        [Test]
        public void Sort_VariantsAndScene_ConversionOrderIsDeterministic()
        {
            var topSourcePrefab = CreatePrefabWithComponent<OffMeshLink>(m_TestFolderPath + "/TopSourcePrefab.prefab");

            var midSourcePrefab = PrefabUtility.InstantiatePrefab(topSourcePrefab) as GameObject;
            var midSourcePrefabAsset = PrefabUtility.SaveAsPrefabAsset(midSourcePrefab, m_TestFolderPath + "/MidSourcePrefab.prefab");

            var firstLeafPrefab = PrefabUtility.InstantiatePrefab(midSourcePrefabAsset) as GameObject;
            PrefabUtility.SaveAsPrefabAsset(firstLeafPrefab, m_TestFolderPath + "/FirstLeafPrefab.prefab");

            var secondLeafPrefab = PrefabUtility.InstantiatePrefab(midSourcePrefabAsset) as GameObject;
            PrefabUtility.SaveAsPrefabAsset(secondLeafPrefab, m_TestFolderPath + "/SecondLeafPrefab.prefab");

            CreateSceneWithComponent<OffMeshLink>("Assets/OffMeshLinkScene.unity");

            var firstLeafGuid = AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/FirstLeafPrefab.prefab");
            var secondLeafGuid = AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/SecondLeafPrefab.prefab");
            var midSourceGuid = AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/MidSourcePrefab.prefab");
            var topSourceGuid = AssetDatabase.AssetPathToGUID(m_TestFolderPath + "/TopSourcePrefab.prefab");
            var sceneGuid = AssetDatabase.AssetPathToGUID("Assets/OffMeshLinkScene.unity");

            var assetsToConvert = new List<string>
            {
                firstLeafGuid,
                midSourceGuid,
                secondLeafGuid,
                topSourceGuid,
                sceneGuid
            };
            OffMeshLinkUpdaterUtility.Convert(assetsToConvert, out _);

            Assume.That(assetsToConvert.Count, Is.EqualTo(5), "Should have 5 assets to convert.");
            Assert.That(assetsToConvert[0], Is.EqualTo(topSourceGuid));
            Assert.That(assetsToConvert[1], Is.EqualTo(midSourceGuid));
            Assert.That(assetsToConvert[2], Is.EqualTo(firstLeafGuid));
            Assert.That(assetsToConvert[3], Is.EqualTo(secondLeafGuid));
            Assert.That(assetsToConvert[4], Is.EqualTo(sceneGuid));
        }

        static GameObject CreatePrefabWithComponent<T>(string savePath) where T : Component
        {
            var go = new GameObject();
            go.AddComponent<T>();
            return CreatePrefabFromGameObject(go, savePath);
        }

        static GameObject CreatePrefabFromGameObject(GameObject go, string savePath)
        {
            var prefabGo = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Object.DestroyImmediate(go);
            return prefabGo;
        }

        static Scene CreateSceneWithComponent<T>(string savePath) where T : Component
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneManager.SetActiveScene(scene);

            var go = new GameObject();
            go.AddComponent<T>();

            EditorSceneManager.SaveScene(scene, savePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return scene;
        }

        static Scene CreateSceneWithPrefabInstance(GameObject prefab, string savePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneManager.SetActiveScene(scene);

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            EditorSceneManager.SaveScene(scene, savePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            return scene;
        }

        static void NestPrefabs(GameObject parentPrefab, GameObject childPrefab)
        {
            var parentInstance = PrefabUtility.InstantiatePrefab(parentPrefab) as GameObject;
            PrefabUtility.InstantiatePrefab(childPrefab, parentInstance.transform);
            PrefabUtility.ApplyPrefabInstance(parentInstance, InteractionMode.AutomatedAction);
        }

        static GameObject GuidToPrefab(string guid)
        {
            var pathToObject = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<GameObject>(pathToObject);
        }

        static List<GameObject> GuidsToPrefabs(List<string> guids)
        {
            var gameObjects = new List<GameObject>();
            foreach (var guid in guids)
                gameObjects.Add(GuidToPrefab(guid));
            return gameObjects;
        }

        static void AssertValuesAreEqual(OffMeshLinkData expectedData, NavMeshLink navMeshLink)
        {
            Assert.That(navMeshLink.activated, Is.EqualTo(expectedData.activated), "Expected activated to be equal.");
            Assert.That(navMeshLink.autoUpdate, Is.EqualTo(expectedData.autoUpdatePositions), "Expected autoUpdate to be equal.");
            Assert.That(navMeshLink.bidirectional, Is.EqualTo(expectedData.biDirectional), "Expected bidirectional to be equal.");
            Assert.That(navMeshLink.costModifier, Is.EqualTo(expectedData.costOverride), "Expected costModifier to be equal.");
            Assert.That(navMeshLink.startTransform, Is.EqualTo(expectedData.startTransform), "Expected startTransform to be equal.");
            Assert.That(navMeshLink.endTransform, Is.EqualTo(expectedData.endTransform), "Expected endTransform to be equal.");
        }
    }
#pragma warning restore 618
}

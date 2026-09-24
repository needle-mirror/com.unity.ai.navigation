using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.AI.Navigation.Editor
{
    [InitializeOnLoad]
    static class NavMeshSurfaceSelectionTracker
    {
        static readonly HashSet<GameObject> s_SelectedGameObjects = new HashSet<GameObject>();
        static readonly HashSet<NavMeshSurface> s_SurfacesInSelection = new HashSet<NavMeshSurface>();

        // The NavMesh visualization is drawn by the engine, which needs to know which surfaces belong to the
        // selection in order to draw them with the opacity chosen for selected surfaces. Reporting that from the
        // NavMeshSurface gizmo callbacks would make the visualization follow the NavMeshSurface entry of the
        // Gizmos menu, which must only control the bounding box. (UUM-133537)
        // The engine clears the reported set after every draw, so the surfaces are reported again just before each
        // gizmo-drawing camera renders: the Scene view through beforeSceneGui, and the Game view through onPreCull
        // (built-in render pipeline) or beginCameraRendering (scriptable ones). The selected GameObjects only change
        // with the selection, so that set is cached and rebuilt from Selection.selectionChanged instead of on every
        // render.
        // These events live in long-lived engine/editor assemblies, so the subscriptions must be removed before this
        // assembly unloads: under CoreCLR there is no domain reload to purge them, and a dangling delegate would keep
        // this collectible assembly loaded and its handlers firing. beforeAssemblyReload runs the unsubscribe on both
        // backends (matching the pattern in Editor/Mono/Handles/HandleUtility.cs).
        static NavMeshSurfaceSelectionTracker()
        {
            RebuildSelectionCache();

            Selection.selectionChanged += RebuildSelectionCache;
            SceneView.beforeSceneGui += OnBeforeSceneGui;
            Camera.onPreCull += OnCameraPreCull;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        static void OnBeforeAssemblyReload()
        {
            Selection.selectionChanged -= RebuildSelectionCache;
            SceneView.beforeSceneGui -= OnBeforeSceneGui;
            Camera.onPreCull -= OnCameraPreCull;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        }

        static void OnBeforeSceneGui(SceneView sceneView)
        {
            if (Event.current != null && Event.current.type == EventType.Repaint)
                ReportSurfacesInSelectionHierarchy();
        }

        static void OnCameraPreCull(Camera camera)
        {
            if (ReportsThroughRenderCallback(camera))
                ReportSurfacesInSelectionHierarchy();
        }

        static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (ReportsThroughRenderCallback(camera))
                ReportSurfacesInSelectionHierarchy();
        }

        // The Scene view reports through beforeSceneGui, so the render callbacks only need to cover the Game view,
        // which draws the visualization when gizmos are enabled. Reporting for the Scene-view camera here as well
        // would walk the active surfaces twice per Scene-view repaint, and reporting for preview thumbnails,
        // reflection probes or other game cameras would walk them for a render that never draws the visualization.
        // ShouldRenderGizmos() further skips game views with gizmos off and off-screen cameras (render textures),
        // which never draw the visualization either.
        static bool ReportsThroughRenderCallback(Camera camera)
        {
            return camera.cameraType == CameraType.Game && Handles.ShouldRenderGizmos();
        }

        static void ReportSurfacesInSelectionHierarchy()
        {
            CollectSurfacesInSelectionHierarchy(s_SelectedGameObjects, s_SurfacesInSelection);

            foreach (var surface in s_SurfacesInSelection)
                surface.navMeshDataInstance.FlagAsInSelectionHierarchy();
        }

        static void RebuildSelectionCache()
        {
            s_SelectedGameObjects.Clear();
            foreach (var selectedGameObject in Selection.gameObjects)
                s_SelectedGameObjects.Add(selectedGameObject);
        }

        // A surface is in the selection hierarchy when it, or one of its ancestors, is selected. Iterating the
        // (usually few) active surfaces and walking each one up to the root keeps the cost proportional to the
        // surface count and the hierarchy depth, independent of how many objects are selected - so a large
        // selection, such as Select All in a big scene, stays cheap. activeSurfaces only holds surfaces that are
        // active in the hierarchy, matching the enabled-object behaviour of the gizmo callback this replaced.
        internal static void CollectSurfacesInSelectionHierarchy(
            HashSet<GameObject> selectedGameObjects, HashSet<NavMeshSurface> surfacesInSelection)
        {
            surfacesInSelection.Clear();

            if (selectedGameObjects.Count == 0)
                return;

            var activeSurfaces = NavMeshSurface.activeSurfaces;
            for (var i = 0; i < activeSurfaces.Count; ++i)
            {
                var surface = activeSurfaces[i];

                // activeSurfaces exposes the backing list, so it can hold a null or destroyed entry; skip it rather
                // than dereference its transform.
                if (!surface)
                    continue;

                for (var ancestor = surface.transform; ancestor != null; ancestor = ancestor.parent)
                {
                    if (selectedGameObjects.Contains(ancestor.gameObject))
                    {
                        surfacesInSelection.Add(surface);
                        break;
                    }
                }
            }
        }
    }
}

using System.Collections.Generic;
#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Unity.AI.Navigation.Editor")]
#endif

namespace Unity.AI.Navigation
{
    /// <summary> Sets the method for filtering the objects retrieved when baking the NavMesh. </summary>
    public enum CollectObjects
    {
        /// <summary> Use all the active objects. </summary>
        [InspectorName("All Game Objects")]
        All = 0,
        /// <summary> Use all the active objects that overlap the bounding volume. </summary>
        [InspectorName("Volume")]
        Volume = 1,
        /// <summary> Use all the active objects that are children of this GameObject. </summary>
        /// <remarks> This includes the current GameObject and all the children of the children that are active.</remarks>
        [InspectorName("Current Object Hierarchy")]
        Children = 2,
        /// <summary> Use all the active objects that are marked with a NavMeshModifier. </summary>
        [InspectorName("NavMeshModifier Component Only")]
        MarkedWithModifier = 3,
    }

    /// <summary> Component used for building and enabling a NavMesh surface for one agent type. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMesh Surface", 30)]
    [HelpURL(HelpUrls.Manual + "NavMeshSurface.html")]
    public class NavMeshSurface : MonoBehaviour
    {
#pragma warning disable 0414

        // Serialized version is used to upgrade older serialized data to the current format.
        // Version 0: Initial version.
        [SerializeField, HideInInspector]
        byte m_SerializedVersion = 0;
#pragma warning restore 0414

        [SerializeField]
        int m_AgentTypeID;

        [SerializeField]
        CollectObjects m_CollectObjects = CollectObjects.All;

        [SerializeField]
        Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField]
        Vector3 m_Center = new Vector3(0, 2.0f, 0);

        [SerializeField]
        LayerMask m_LayerMask = ~0;

        [SerializeField]
        NavMeshCollectGeometry m_UseGeometry = NavMeshCollectGeometry.RenderMeshes;

        [SerializeField]
        int m_DefaultArea;

        [SerializeField]
        bool m_GenerateLinks;

        [SerializeField]
        bool m_IgnoreNavMeshAgent = true;

        [SerializeField]
        bool m_IgnoreNavMeshObstacle = true;

        [SerializeField]
        bool m_OverrideTileSize;

        [SerializeField]
        int m_TileSize = 256;

        [SerializeField]
        bool m_OverrideVoxelSize;

        [SerializeField]
        float m_VoxelSize;

        [SerializeField]
        float m_MinRegionArea = 2;

        [FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        NavMeshData m_NavMeshData;

        [SerializeField]
        bool m_BuildHeightMesh;

        /// <summary> Gets or sets the identifier of the agent type that will use this NavMesh Surface. </summary>
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; } }

        /// <summary> Gets or sets the method for retrieving the objects that will be used for baking. </summary>
        public CollectObjects collectObjects { get { return m_CollectObjects; } set { m_CollectObjects = value; } }

        /// <summary> Gets or sets the size of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The size applies in the local space of the GameObject. </remarks>
        public Vector3 size { get { return m_Size; } set { m_Size = value; } }

        /// <summary> Gets or sets the center position of the volume that delimits the NavMesh created by this component. </summary>
        /// <remarks> It is used only when <c>collectObjects</c> is set to <c>Volume</c>. The position applies in the local space of the GameObject. </remarks>
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }

        /// <summary> Gets or sets a bitmask representing which layers to consider when selecting the objects that will be used for baking the NavMesh. </summary>
        public LayerMask layerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

        /// <summary> Gets or sets which type of component in the GameObjects provides the geometry used for baking the NavMesh. </summary>
        public NavMeshCollectGeometry useGeometry { get { return m_UseGeometry; } set { m_UseGeometry = value; } }

        /// <summary> Gets or sets the area type assigned to any object that does not have one specified. </summary>
        /// <remarks> To customize the area type of an object add a <see cref="NavMeshModifier"/> component and set <see cref="NavMeshModifier.overrideArea"/> to <c>true</c>. The area type information is used when baking the NavMesh. </remarks>
        /// <seealso href="https://docs.unity3d.com/Manual/nav-AreasAndCosts.html"/>
        public int defaultArea { get { return m_DefaultArea; } set { m_DefaultArea = value; } }

        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshAgent"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshAgent { get { return m_IgnoreNavMeshAgent; } set { m_IgnoreNavMeshAgent = value; } }

        /// <summary> Gets or sets whether the process of building the NavMesh ignores the GameObjects containing a <see cref="NavMeshObstacle"/> component. </summary>
        /// <remarks> There is generally no need for the NavMesh to take into consideration the objects that can move.</remarks>
        public bool ignoreNavMeshObstacle
        {
            get { return m_IgnoreNavMeshObstacle; }
            set { m_IgnoreNavMeshObstacle = value; }
        }

        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="tileSize"/> value. </summary>
        public bool overrideTileSize { get { return m_OverrideTileSize; } set { m_OverrideTileSize = value; } }

        /// <summary> Gets or sets the width of the square grid of voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value represents a number of voxels. Together with <see cref="voxelSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public int tileSize { get { return m_TileSize; } set { m_TileSize = value; } }

        /// <summary> Gets or sets whether the NavMesh building process uses the <see cref="voxelSize"/> value. </summary>
        public bool overrideVoxelSize { get { return m_OverrideVoxelSize; } set { m_OverrideVoxelSize = value; } }

        /// <summary> Gets or sets the width of the square voxels that the NavMesh building process uses for sampling the scene geometry. </summary>
        /// <remarks> This value is in world units. Together with <see cref="tileSize"/> it determines the real size of the individual sections that comprise the NavMesh. </remarks>
        public float voxelSize { get { return m_VoxelSize; } set { m_VoxelSize = value; } }

        /// <summary> Gets or sets the minimum acceptable surface area of any continuous portion of the NavMesh. </summary>
        /// <remarks> This parameter is used only at the time when the NavMesh is getting built. It allows you to cull away any isolated NavMesh regions that are smaller than this value and that do not straddle or touch a tile boundary. </remarks>
        public float minRegionArea { get { return m_MinRegionArea; } set { m_MinRegionArea = value; } }

        /// <summary> Gets or sets whether the NavMesh building process produces more detailed elevation information. </summary>
        /// <seealso href="https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.0/manual/NavMeshSurface.html#advanced-settings"/>
        public bool buildHeightMesh { get { return m_BuildHeightMesh; } set { m_BuildHeightMesh = value; } }

        /// <summary> Gets or sets the reference to the NavMesh data instantiated by this surface. </summary>
        public NavMeshData navMeshData { get { return m_NavMeshData; } set { m_NavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance m_NavMeshDataInstance;
        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        internal NavMeshDataInstance navMeshDataInstance => m_NavMeshDataInstance;

        static readonly List<NavMeshSurface> s_NavMeshSurfaces = new List<NavMeshSurface>();

        /// <summary> Gets the list of all the <see cref="NavMeshSurface"/> components that are currently active in the scene. </summary>
        public static List<NavMeshSurface> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        Bounds GetInflatedBounds()
        {
            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            var agentRadius = settings.agentTypeID != -1 ? settings.agentRadius : 0f;

            var bounds = new Bounds(center, size);
            bounds.Expand(new Vector3(agentRadius, 0, agentRadius));
            return bounds;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ClearNavMeshSurfaces()
        {
            NavMesh.onPreUpdate -= UpdateActive;
            s_NavMeshSurfaces.Clear();
        }

        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        /// <summary> Creates an instance of the NavMesh data and activates it in the navigation system. </summary>
        /// <remarks> The instance is created at the position and with the orientation of the GameObject. </remarks>
        public void AddData()
        {
#if UNITY_EDITOR
            if (IsEditedInPrefab(this))
                return;
#endif
            if (m_NavMeshDataInstance.valid)
                return;

            if (m_NavMeshData != null)
            {
                m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, transform.position, transform.rotation);
                m_NavMeshDataInstance.owner = this;
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        /// <summary> Removes the instance of this NavMesh data from the navigation system. </summary>
        /// <remarks> This operation does not destroy the <see cref="navMeshData"/>. </remarks>
        public void RemoveData()
        {
            m_NavMeshDataInstance.Remove();
            m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        /// <summary> Retrieves a copy of the current settings chosen for building this NavMesh surface. </summary>
        /// <returns> The settings configured in this NavMeshSurface. </returns>
        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = m_AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }

            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }

            buildSettings.minRegionArea = minRegionArea;
            buildSettings.buildHeightMesh = buildHeightMesh;

            return buildSettings;
        }

        /// <summary> Builds and instantiates this NavMesh surface. </summary>
        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since NavMesh data has no scaling support - it is the right choice here.
            var surfaceBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects.Volume)
            {
                surfaceBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                sources, surfaceBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                m_NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        /// <summary> Rebuilds parts of an existing NavMesh in the regions of the scene where the objects have changed. </summary>
        /// <remarks> This operation is executed asynchronously. </remarks>
        /// <param name="data"> The NavMesh to update according to the changes in the scene. </param>
        /// <returns> A reference to the asynchronous coroutine that builds the NavMesh. </returns>
        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since NavMesh data has no scaling support - it is the right choice here.
            var surfaceBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects != CollectObjects.Volume)
                surfaceBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, surfaceBounds);
        }

        static void Register(NavMeshSurface surface)
        {
#if UNITY_EDITOR
            if (IsEditedInPrefab(surface))
                return;
#endif
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurface surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(gameObject);
            if (!myStage.IsValid())
                return;
#endif

            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                    continue;
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y),
                    m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.ModifierBox;
                src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
                src.size = msize;
                src.area = m.area;
                sources.Add(src);
            }
        }

        // A project can opt out of baking a terrain's trees altogether by adding AI_NAVIGATION_IGNORE_TERRAIN_TREES
        // to its Scripting Define Symbols, which compiles out everything below and restores the behaviour of
        // the versions that never collected them.
#if NMC_CAN_ACCESS_TERRAIN && !AI_NAVIGATION_IGNORE_TERRAIN_TREES
        // Per-prototype shape descriptor: one entry per renderable mesh, per supported collider or per
        // NavMeshModifierVolume found on a tree prefab.
        // For Mesh sources the dimensions come from the mesh itself; the primitive shapes take theirs from `size`.
        // localToRoot places the shape relative to the prefab's root, so geometry and volumes authored on
        // child GameObjects (LOD children, multi-part trees, offset volumes) are not collapsed onto the root.
        struct TreeShape
        {
            public NavMeshBuildSourceShape shape;
            public UnityEngine.Object sourceObject; // Mesh for Mesh shape; null for the primitive shapes
            public Vector3 size;                    // full extent per axis, in the shape's own local space
            public Matrix4x4 localToRoot;           // shape placement, relative to the prefab's root
            public int area;                        // area this shape bakes with
            public bool generateLinks;              // resolved generateLinks for this shape
            public bool marked;                     // a NavMeshModifier governs this shape
            public Bounds localBounds;              // conservative extent of the shape, relative to the prefab's root
        }

        // Settings a NavMeshModifier contributes to one shape of a tree prefab.
        struct TreeShapeSettings
        {
            public bool ignore;
            public bool marked;
            public int area;
            public bool generateLinks;
        }

        // Resolved bake metadata for one TreePrototype on a terrain.
        struct TreePrototypeBakeInfo
        {
            public bool valid;              // false means all instances of this prototype are skipped
            public bool emitsInMarkedMode;  // holds at least one shape that survives CollectObjects.MarkedWithModifier
            public bool hasRoundShapes;     // holds a Sphere or Capsule, which localBounds can understate
            public Vector3 prefabScale;     // protoPrefab.transform.localScale
            public Bounds localBounds;      // union of all shape bounds, relative to the prefab's root
            public List<TreeShape> shapes;
        }

        // Unity's NavMeshBuilder.CollectSources emits terrains as Terrain-shape sources but does not enumerate
        // the trees stored in TerrainData. This method appends those tree instances as additional sources so
        // they participate in the NavMesh bake. A terrain's trees are collected only when its own layer passes
        // the surface's mask; each instance is then filtered by the CollectObjects mode, and governed by any
        // NavMeshModifier or NavMeshModifierVolume component on the tree prefab.
        void AppendTerrainTrees(List<NavMeshBuildSource> sources)
        {
            // Snapshot count: we are about to append to the list, so iterate only the original entries.
            var originalCount = sources.Count;
            if (originalCount == 0)
                return;

            var collectWithModifiersOnly = m_CollectObjects == CollectObjects.MarkedWithModifier;
            var collectInVolume = m_CollectObjects == CollectObjects.Volume;

            // Volume bounds in world space (computed lazily and only when needed).
            var haveVolumeBounds = false;
            var volumeBounds = new Bounds();

            for (var i = 0; i < originalCount; i++)
            {
                var s = sources[i];
                if (s.shape != NavMeshBuildSourceShape.Terrain)
                    continue;

                var terrainData = s.sourceObject as TerrainData;
                if (terrainData == null)
                    continue;

                var terrain = s.component as Terrain;
                var terrainTransform = terrain != null ? terrain.transform : null;
                if (terrainTransform == null)
                    continue;

                // Terrain-level layer mask: Unity's collector already filters by layer, but trees are not
                // present in the collector's output so we re-check defensively here.
                if ((m_LayerMask.value & (1 << terrainTransform.gameObject.layer)) == 0)
                    continue;

                var prototypes = terrainData.treePrototypes;
                var treeInstanceCount = terrainData.treeInstanceCount;
                if (prototypes == null || prototypes.Length == 0 || treeInstanceCount == 0)
                    continue;

                // Build per-prototype cache once per terrain to avoid GetComponent calls inside the instance loop.
                // The terrain source carries the area and link settings that markup collection already resolved
                // for it, so the trees start from those rather than from the surface defaults - a NavMeshModifier
                // on the terrain has to reach its trees too, or the ground and the trees standing on it end up
                // in different areas.
                var prototypeInfos = new TreePrototypeBakeInfo[prototypes.Length];
                for (var p = 0; p < prototypes.Length; p++)
                    prototypeInfos[p] = BuildPrototypeBakeInfo(prototypes[p], s.area, s.generateLinks);

                // Lazily compute volume bounds the first time a terrain in Volume mode needs them.
                if (collectInVolume && !haveVolumeBounds)
                {
                    var localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    volumeBounds = GetWorldBounds(localToWorld, GetInflatedBounds());
                    haveVolumeBounds = true;
                }

                var terrainSize = terrainData.size;
                // A Terrain draws its heightmap, its textures and its trees axis-aligned from
                // transform.position, taking its extent from TerrainData.size: the GameObject's rotation and
                // scale are ignored outright, and the terrain source the engine hands us carries a bare
                // translation for the same reason. Placing the trees through the full matrix instead would
                // put obstacles where the terrain draws nothing, and leave the drawn trees unbaked.
                // The matrix is also cached outside the per-instance loop because transform property getters
                // cross the C#/C++ boundary on each access, which counts when there are thousands of
                // instances. The terrain transform does not change during the bake.
                var terrainToWorld = Matrix4x4.Translate(terrainTransform.position);
                for (var t = 0; t < treeInstanceCount; t++)
                {
                    // GetTreeInstance(i) returns a struct by value; the `treeInstances` property
                    // would allocate a fresh array copy (megabytes for dense terrains).
                    var inst = terrainData.GetTreeInstance(t);
                    if (inst.prototypeIndex < 0 || inst.prototypeIndex >= prototypeInfos.Length)
                        continue;

                    var info = prototypeInfos[inst.prototypeIndex];
                    if (!info.valid)
                        continue;
                    if (collectWithModifiersOnly && !info.emitsInMarkedMode)
                        continue;

                    // TreeInstance.position is normalized (0..1) within (terrainSize.x, terrainSize.y, terrainSize.z).
                    var localOnTerrain = Vector3.Scale(inst.position, terrainSize);
                    var instanceScale = new Vector3(
                        info.prefabScale.x * inst.widthScale,
                        info.prefabScale.y * inst.heightScale,
                        info.prefabScale.z * inst.widthScale);

                    // A tree is placed in the terrain's local space and carried into the world by the terrain's
                    // origin, so a terrain that is moved takes its trees with it. The rotation and the scale
                    // applied here are the instance's own, painted onto the terrain - not the terrain
                    // GameObject's, which the terrain itself disregards.
                    var instanceToWorld = terrainToWorld * Matrix4x4.TRS(
                        localOnTerrain,
                        Quaternion.Euler(0f, inst.rotation * Mathf.Rad2Deg, 0f),
                        instanceScale);

                    if (collectInVolume)
                    {
                        // Cheap conservative test against the surface volume, using the prototype's real extent
                        // (the union of its meshes, colliders and modifier volumes). Testing the instance origin
                        // alone would wrongly skip a tree whose canopy or volume reaches into the collect volume
                        // while its trunk stays outside.
                        // A Sphere or a Capsule can reach further than that extent once the instance's scale is
                        // applied, so a tree that fails the test is still kept when one of those does reach in.
                        if (!volumeBounds.Intersects(GetWorldBounds(instanceToWorld, info.localBounds))
                            && !(info.hasRoundShapes && AnyRoundShapeIntersects(info.shapes, instanceToWorld, volumeBounds)))
                            continue;
                    }

                    for (var sh = 0; sh < info.shapes.Count; sh++)
                    {
                        var shape = info.shapes[sh];

                        // Modifier volumes are appended whatever the collect mode is, exactly as the volumes of
                        // a scene are. Geometry, on the other hand, only takes part in marked mode when a
                        // NavMeshModifier governs it, so a tree behaves like any other object in the scene.
                        if (collectWithModifiersOnly && !shape.marked && shape.shape != NavMeshBuildSourceShape.ModifierBox)
                            continue;

                        // Compose the shape's prefab-local placement with the instance's placement on the terrain.
                        var shapeToWorld = instanceToWorld * shape.localToRoot;

                        NavMeshBuildSource src;
                        if (shape.shape == NavMeshBuildSourceShape.Mesh)
                        {
                            // Mesh sources carry their dimensions in the mesh, so the whole matrix goes through.
                            src = new NavMeshBuildSource
                            {
                                shape = NavMeshBuildSourceShape.Mesh,
                                sourceObject = shape.sourceObject,
                                transform = shapeToWorld,
                                size = Vector3.zero,
                                area = shape.area,
                                component = s.component,
                                generateLinks = shape.generateLinks,
                            };
                        }
                        else
                        {
                            // Box, Sphere, Capsule and ModifierBox sources all carry their dimensions in `size`,
                            // as the full extent per axis in world units, so the transform keeps an identity
                            // scale and the scale is folded into the size instead.
                            src = new NavMeshBuildSource
                            {
                                shape = shape.shape,
                                sourceObject = null,
                                transform = Matrix4x4.TRS(shapeToWorld.GetPosition(), shapeToWorld.rotation, Vector3.one),
                                size = GetScaledSize(shape, shapeToWorld),
                                area = shape.area,
                                component = s.component,
                                // A modifier volume only re-labels the area of other geometry, so it never
                                // participates in link generation - matching how scene volumes are appended.
                                generateLinks = shape.generateLinks && shape.shape != NavMeshBuildSourceShape.ModifierBox
                            };
                        }
                        sources.Add(src);
                    }
                }
            }
        }

        // `defaultArea` and `defaultGenerateLinks` come from the terrain source the trees stand on, so that any
        // markup already applied to the terrain carries over to its trees unless the prefab overrides it.
        TreePrototypeBakeInfo BuildPrototypeBakeInfo(TreePrototype proto, int defaultArea, bool defaultGenerateLinks)
        {
            var info = new TreePrototypeBakeInfo
            {
                valid = false,
                prefabScale = Vector3.one,
                shapes = new List<TreeShape>(),
            };

            var prefab = proto.prefab;
            if (prefab == null)
                return info;

            info.prefabScale = prefab.transform.localScale;

            // Without the physics module there are no colliders to read, so PhysicsColliders contributes no
            // geometry at all - the same answer the engine gives for the rest of the scene in that configuration.
            var ignoredAnyShape = false;
            if (m_UseGeometry == NavMeshCollectGeometry.RenderMeshes)
                ignoredAnyShape = CollectRenderMeshShapes(prefab, proto.navMeshLod, defaultArea, defaultGenerateLinks, info.shapes);
#if NMC_CAN_ACCESS_PHYSICS
            else
                ignoredAnyShape = CollectColliderShapes(prefab, defaultArea, defaultGenerateLinks, info.shapes);
#endif

            // Modifier volumes are not geometry, so they are collected in both useGeometry modes, and they are
            // not governed by NavMeshModifier markup any more than the volumes of a scene are. A tree prefab
            // that carries nothing but a NavMeshModifierVolume is a valid and useful prototype: painting it onto
            // a terrain re-labels the area of the NavMesh underneath without adding any tree-shaped geometry,
            // which is the only practical way to vary areas across a terrain.
            CollectModifierVolumeShapes(prefab, m_AgentTypeID, info.shapes);

            info.valid = info.shapes.Count > 0;
            if (info.valid)
            {
                info.localBounds = CalculateShapesBounds(info.shapes);
                foreach (var shape in info.shapes)
                {
                    if (shape.marked || shape.shape == NavMeshBuildSourceShape.ModifierBox)
                        info.emitsInMarkedMode = true;
                    if (shape.shape == NavMeshBuildSourceShape.Sphere || shape.shape == NavMeshBuildSourceShape.Capsule)
                        info.hasRoundShapes = true;
                }
            }

#if UNITY_EDITOR
            // A prototype left with nothing to bake because a modifier ignores it was configured that way on
            // purpose, so only the prototypes that have nothing usable at all are worth warning about.
            if (!info.valid && !ignoredAnyShape)
            {
                const string colliderAdvice =
#if NMC_CAN_ACCESS_PHYSICS
                    "a supported Collider (MeshCollider/BoxCollider/SphereCollider/CapsuleCollider, with " +
                    "useGeometry=PhysicsColliders), ";
#else
                    // Suggesting a collider would be a dead end when the project has no physics module.
                    "";
#endif
                Debug.LogWarning($"The tree prefab '{prefab.name}' has nothing that the NavMesh bake can use. " +
                    "Verify that the prefab has a MeshFilter drawn by an enabled MeshRenderer (with " +
                    $"useGeometry=RenderMeshes), {colliderAdvice}or a NavMeshModifierVolume.", prefab);
            }
#endif

            return info;
        }

        // Finds the NavMeshModifier that governs one GameObject of a tree prefab, following the same rules that
        // markup collection applies in a scene: the nearest modifier wins, a modifier on an ancestor only reaches
        // this object when its applyToChildren is on, and a disabled modifier does not count. The search stops at
        // the prefab root because anything above it is not part of the tree.
        NavMeshModifier FindModifierFor(Transform root, Transform owner)
        {
            var modifiers = new List<NavMeshModifier>();
            for (var t = owner; t != null; t = t.parent)
            {
                t.GetComponents(modifiers);
                foreach (var modifier in modifiers)
                {
                    if (modifier.enabled && modifier.AffectsAgentType(m_AgentTypeID)
                        && (t == owner || modifier.applyToChildren))
                        return modifier;
                }

                if (t == root)
                    break;
            }

            return null;
        }

        TreeShapeSettings ResolveShapeSettings(Transform root, Transform owner, int defaultArea, bool defaultGenerateLinks)
        {
            var settings = new TreeShapeSettings { area = defaultArea, generateLinks = defaultGenerateLinks };

            var modifier = FindModifierFor(root, owner);
            if (modifier == null)
                return settings;

            settings.marked = true;
            settings.ignore = modifier.ignoreFromBuild;
            if (modifier.overrideArea)
                settings.area = modifier.area;
            if (modifier.overrideGenerateLinks)
                settings.generateLinks = modifier.generateLinks;

            return settings;
        }

        // Union of all the shapes' extents, in the prefab root's local space.
        static Bounds CalculateShapesBounds(List<TreeShape> shapes)
        {
            var result = shapes[0].localBounds;
            for (var i = 1; i < shapes.Count; i++)
                result.Encapsulate(shapes[i].localBounds);
            return result;
        }

        // The size a primitive shape is emitted with: its full extent per axis, with the scale of its placement
        // in the world folded in.
        static Vector3 GetScaledSize(TreeShape shape, Matrix4x4 shapeToWorld)
        {
            return Vector3.Scale(shape.size, Abs(shapeToWorld.lossyScale));
        }

        // The full extent per axis that the builder gives a primitive source of this size. A Sphere takes the
        // largest of the three, and a Capsule the larger of X and Z across, and at least that much along Y, since
        // a capsule shorter than it is wide is built as a sphere. See CalculateTriangleData in
        // RuntimeNavMeshBuilder.cpp.
        static Vector3 GetBuiltExtent(NavMeshBuildSourceShape shape, Vector3 size)
        {
            switch (shape)
            {
                case NavMeshBuildSourceShape.Sphere:
                {
                    var diameter = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
                    return new Vector3(diameter, diameter, diameter);
                }
                case NavMeshBuildSourceShape.Capsule:
                {
                    var diameter = Mathf.Max(size.x, size.z);
                    return new Vector3(diameter, Mathf.Max(size.y, diameter), diameter);
                }
                default:
                    return size;
            }
        }

        // Tests the Sphere and Capsule shapes of one tree instance against the collect volume by the extent the
        // builder gives them, which the prototype's localBounds understate whenever the scale reaching a shape
        // is not uniform.
        static bool AnyRoundShapeIntersects(List<TreeShape> shapes, Matrix4x4 instanceToWorld, Bounds volumeBounds)
        {
            foreach (var shape in shapes)
            {
                if (shape.shape != NavMeshBuildSourceShape.Sphere && shape.shape != NavMeshBuildSourceShape.Capsule)
                    continue;

                var shapeToWorld = instanceToWorld * shape.localToRoot;
                var placement = Matrix4x4.TRS(shapeToWorld.GetPosition(), shapeToWorld.rotation, Vector3.one);
                var extent = GetBuiltExtent(shape.shape, GetScaledSize(shape, shapeToWorld));
                if (volumeBounds.Intersects(GetWorldBounds(placement, new Bounds(Vector3.zero, extent))))
                    return true;
            }

            return false;
        }

        // Placement of a component's GameObject relative to the prefab's root, kept as a matrix. A prefab that
        // scales an intermediate child non-uniformly and rotates something below it produces a sheared
        // placement, which no position/rotation/scale triple can express, so the matrix is carried whole.
        // The root's own scale is excluded because it is already folded into the per-instance scale through
        // TreePrototypeBakeInfo.prefabScale.
        static Matrix4x4 GetLocalToRoot(Transform root, Transform owner)
        {
            return owner == root ? Matrix4x4.identity : root.worldToLocalMatrix * owner.localToWorldMatrix;
        }

        static TreeShape MakeMeshShape(Transform root, Transform owner, Mesh mesh, TreeShapeSettings settings)
        {
            var localToRoot = GetLocalToRoot(root, owner);
            return new TreeShape
            {
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = mesh,
                localToRoot = localToRoot,
                area = settings.area,
                generateLinks = settings.generateLinks,
                marked = settings.marked,
                localBounds = GetWorldBounds(localToRoot, mesh.bounds),
            };
        }

        // `center` is expressed in the owner's local space, so it is carried through the owner's rotation
        // and scale to land at the right place relative to the prefab's root. `localRotation` then orients the
        // primitive within that space, which a Capsule needs because its own axis is fixed to Y.
        static TreeShape MakePrimitiveShape(Transform root, Transform owner, NavMeshBuildSourceShape primitive,
            Vector3 center, Vector3 size, Quaternion localRotation, TreeShapeSettings settings)
        {
            // Translating along the owner's own axes keeps `center` in the owner's local space, which is what
            // a Collider's or a volume's center means, and carries any shear in the owner's placement with it.
            var localToRoot = GetLocalToRoot(root, owner) * Matrix4x4.Translate(center) * Matrix4x4.Rotate(localRotation);
            return new TreeShape
            {
                shape = primitive,
                size = size,
                localToRoot = localToRoot,
                area = settings.area,
                generateLinks = settings.generateLinks,
                marked = settings.marked,
                localBounds = GetWorldBounds(localToRoot, new Bounds(Vector3.zero, size)),
            };
        }

        // Each collector returns whether a NavMeshModifier kept at least one shape out of the bake, so that the
        // caller can tell a deliberately ignored prototype apart from one that has nothing usable.
        bool CollectRenderMeshShapes(GameObject prefab, int navMeshLod, int defaultArea, bool defaultGenerateLinks, List<TreeShape> shapes)
        {
            var root = prefab.transform;
            var ignoredAny = false;

            if (prefab.TryGetComponent<LODGroup>(out var lodGroup))
            {
                var lods = lodGroup.GetLODs();
                if (lods.Length == 0)
                    return false;
                var lodIndex = Mathf.Clamp(navMeshLod, 0, lods.Length - 1);
                foreach (var r in lods[lodIndex].renderers)
                {
                    if (r == null || !r.enabled || !IsActiveBelowRoot(root, r.transform))
                        continue;
                    var lodMeshFilter = r.GetComponent<MeshFilter>();
                    if (lodMeshFilter == null || lodMeshFilter.sharedMesh == null)
                        continue;

                    var lodSettings = ResolveShapeSettings(root, lodMeshFilter.transform, defaultArea, defaultGenerateLinks);
                    if (lodSettings.ignore)
                        ignoredAny = true;
                    else
                        shapes.Add(MakeMeshShape(root, lodMeshFilter.transform, lodMeshFilter.sharedMesh, lodSettings));
                }

                return ignoredAny;
            }

            // includeInactive reaches the whole prefab even when its root is inactive; IsActiveBelowRoot then
            // discards the parts that the prefab itself deactivates.
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null || !IsDrawnByEnabledRenderer(mf) || !IsActiveBelowRoot(root, mf.transform))
                    continue;

                var settings = ResolveShapeSettings(root, mf.transform, defaultArea, defaultGenerateLinks);
                if (settings.ignore)
                    ignoredAny = true;
                else
                    shapes.Add(MakeMeshShape(root, mf.transform, mf.sharedMesh, settings));
            }

            return ignoredAny;
        }

        // A mesh is render geometry only while an enabled renderer draws it. This is the same contract that
        // source collection applies to the objects of a scene in RenderMeshes mode, so a tree follows it too.
        static bool IsDrawnByEnabledRenderer(MeshFilter meshFilter)
        {
            var renderer = meshFilter.GetComponent<MeshRenderer>();
            return renderer != null && renderer.enabled;
        }

        // The terrain does not draw a deactivated part of a tree prefab, so the bake must not use it either.
        // The prefab root's own active state is deliberately not part of this: a prototype is normally an asset,
        // or a scene object deactivated precisely to keep it out of the scene's own collection, and its trees are
        // drawn regardless. Only a deactivation authored inside the prefab says anything about the tree.
        static bool IsActiveBelowRoot(Transform root, Transform owner)
        {
            for (var t = owner; t != null && t != root; t = t.parent)
            {
                if (!t.gameObject.activeSelf)
                    return false;
            }

            return true;
        }

#if NMC_CAN_ACCESS_PHYSICS
        bool CollectColliderShapes(GameObject prefab, int defaultArea, bool defaultGenerateLinks, List<TreeShape> shapes)
        {
            var root = prefab.transform;
            var ignoredAny = false;

            var colliders = prefab.GetComponentsInChildren<Collider>(includeInactive: true);
            foreach (var c in colliders)
            {
                if (!c.enabled || c.isTrigger || !IsActiveBelowRoot(root, c.transform))
                    continue;

                var settings = ResolveShapeSettings(root, c.transform, defaultArea, defaultGenerateLinks);
                if (settings.ignore)
                {
                    ignoredAny = true;
                    continue;
                }

                if (c is MeshCollider mc && mc.sharedMesh != null)
                {
                    shapes.Add(MakeMeshShape(root, mc.transform, mc.sharedMesh, settings));
                }
                else if (c is BoxCollider box)
                {
                    shapes.Add(MakePrimitiveShape(root, box.transform, NavMeshBuildSourceShape.Box,
                        box.center, box.size, Quaternion.identity, settings));
                }
                else if (c is SphereCollider sphere)
                {
                    // A Sphere source takes the full extent on each axis, exactly as a Box does. The instance's
                    // scale is folded into that size per axis later on, which a sphere survives even though a
                    // SphereCollider is never an ellipsoid: the builder reduces a Sphere source to the largest of
                    // its three half-extents, so a per-axis size and the uniform one the engine's own collector
                    // emits describe the same sphere. See CalculateTriangleData in RuntimeNavMeshBuilder.cpp.
                    var d = 2f * sphere.radius;
                    shapes.Add(MakePrimitiveShape(root, sphere.transform, NavMeshBuildSourceShape.Sphere,
                        sphere.center, new Vector3(d, d, d), Quaternion.identity, settings));
                }
                else if (c is CapsuleCollider capsule)
                {
                    var d = 2f * capsule.radius;

                    // The height is passed on as authored, without being raised to the diameter first. A collider
                    // shorter than it is wide is legal - Unity draws it as a sphere - and the collider scales the
                    // height it was given, letting the cylinder between the caps vanish. Clamping here instead
                    // would hand the instance's scale an inflated height to multiply.
                    var h = capsule.height;

                    // A Capsule source always runs along its own local Y, taking its total height from size.y and
                    // its cross-section from size.x and size.z. A collider aligned to X or Z is therefore rotated
                    // to bring that axis onto Y, and the size is then given in the capsule's own frame.
                    var axis = capsule.direction switch
                    {
                        0 => Quaternion.Euler(0f, 0f, -90f), // collider runs along X
                        2 => Quaternion.Euler(90f, 0f, 0f),  // collider runs along Z
                        _ => Quaternion.identity             // collider runs along Y
                    };
                    shapes.Add(MakePrimitiveShape(root, capsule.transform, NavMeshBuildSourceShape.Capsule,
                        capsule.center, new Vector3(d, h, d), axis, settings));
                }
            }

            return ignoredAny;
        }
#endif // NMC_CAN_ACCESS_PHYSICS

        static void CollectModifierVolumeShapes(GameObject prefab, int agentTypeID, List<TreeShape> shapes)
        {
            var volumes = prefab.GetComponentsInChildren<NavMeshModifierVolume>(includeInactive: true);
            if (volumes.Length == 0)
                return;

            var root = prefab.transform;
            foreach (var v in volumes)
            {
                if (!v.enabled || !v.AffectsAgentType(agentTypeID) || !IsActiveBelowRoot(root, v.transform))
                    continue;

                // A volume carries its own area and never generates links, so it takes nothing from the
                // surrounding modifier markup.
                var settings = new TreeShapeSettings { area = v.area };
                shapes.Add(MakePrimitiveShape(root, v.transform, NavMeshBuildSourceShape.ModifierBox,
                    v.center, v.size, Quaternion.identity, settings));
            }
        }
#endif

        internal List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup();
                markup.root = m.transform;
                markup.overrideArea = m.overrideArea;
                markup.area = m.area;
                markup.ignoreFromBuild = m.ignoreFromBuild;
                markup.applyToChildren = m.applyToChildren;
                markup.overrideGenerateLinks = m.overrideGenerateLinks;
                markup.generateLinks = m.generateLinks;
                markups.Add(markup);
            }

            switch (m_CollectObjects)
            {
                default:
                case CollectObjects.All:
                    CollectSourcesInHierarchy(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups,
                        false, sources);
                    break;
                case CollectObjects.Children:
                    CollectSourcesInHierarchy(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks,
                        markups, false, sources);
                    break;
                case CollectObjects.Volume:
                {
                    var localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, GetInflatedBounds());
                    CollectSourcesInVolume(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks,
                        markups, false, sources);
                    break;
                }
                case CollectObjects.MarkedWithModifier:
                    CollectSourcesInHierarchy(null, m_LayerMask, m_UseGeometry, m_DefaultArea, m_GenerateLinks, markups,
                        true, sources);
                    break;
            }

#if NMC_CAN_ACCESS_TERRAIN && !AI_NAVIGATION_IGNORE_TERRAIN_TREES
            AppendTerrainTrees(sources);
#endif

            // A terrain contributes one source per shape per tree instance, all sharing the terrain's own
            // component, so the per-source GetComponent below is cached to avoid repeating the same native
            // lookup thousands of times.
            if (m_IgnoreNavMeshAgent)
                RemoveSourcesOfObjectsWith<NavMeshAgent>(sources);

            if (m_IgnoreNavMeshObstacle)
                RemoveSourcesOfObjectsWith<NavMeshObstacle>(sources);

            AppendModifierVolumes(ref sources);

            return sources;
        }

        static void RemoveSourcesOfObjectsWith<T>(List<NavMeshBuildSource> sources) where T : Component
        {
            var checkedObjects = new Dictionary<GameObject, bool>();
            sources.RemoveAll(x =>
            {
                if (x.component == null)
                    return false;

                var go = x.component.gameObject;
                if (!checkedObjects.TryGetValue(go, out var hasComponent))
                {
                    hasComponent = go.GetComponent<T>() != null;
                    checkedObjects.Add(go, hasComponent);
                }

                return hasComponent;
            });
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        internal Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            var worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            // Bounds is a mutable struct, so a Bounds? would only let us mutate a copy obtained through its Value property.
            // We track the initialization manually instead, to avoid starting from bounds that contain the local origin.
            Bounds result = default;
            var initialized = false;
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                    {
                        var mesh = src.sourceObject as Mesh;
                        EncapsulateSafe(GetWorldBounds(worldToLocal * src.transform, mesh.bounds));
                        break;
                    }
                    case NavMeshBuildSourceShape.Terrain:
                    {
#if NMC_CAN_ACCESS_TERRAIN
                        // Terrain pivot is lower/left corner - shift bounds accordingly
                        var terrain = src.sourceObject as TerrainData;
                        EncapsulateSafe(GetWorldBounds(worldToLocal * src.transform,
                            new Bounds(0.5f * terrain.size, terrain.size)));
#else
                        Debug.LogWarning("The NavMesh cannot be properly baked for the terrain because the necessary functionality is missing. Add the com.unity.modules.terrain package through the Package Manager.");
#endif
                        break;
                    }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        EncapsulateSafe(GetWorldBounds(worldToLocal * src.transform,
                            new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }

            // When no source contributed any bounds the result remains an empty box at the local origin.
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;

            void EncapsulateSafe(Bounds other)
            {
                if (initialized)
                {
                    result.Encapsulate(other);
                }
                else
                {
                    result = other;
                    initialized = true;
                }
            }
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position)
                return true;
            if (m_LastRotation != transform.rotation)
                return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

        void CollectSourcesInVolume(
            Bounds includedWorldBounds, int includedLayerMask, NavMeshCollectGeometry geometry, int areaByDefault,
            bool generateLinksByDefault,
            List<NavMeshBuildMarkup> markups, bool includeOnlyMarkedObjects, List<NavMeshBuildSource> results)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying || IsPartOfPrefab())
            {
#if EDITOR_ONLY_NAVMESH_BUILDER_DEPRECATED
                UnityEditor.AI.NavMeshEditorHelpers.CollectSourcesInStage(
                    includedWorldBounds, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, gameObject.scene, results);
#else
                UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                    includedWorldBounds, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, gameObject.scene, results);
#endif
            }
            else
#endif
            {
                NavMeshBuilder.CollectSources(
                    includedWorldBounds, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, results);
            }
        }

        void CollectSourcesInHierarchy(
            Transform root, int includedLayerMask, NavMeshCollectGeometry geometry, int areaByDefault,
            bool generateLinksByDefault,
            List<NavMeshBuildMarkup> markups, bool includeOnlyMarkedObjects, List<NavMeshBuildSource> results)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying || IsPartOfPrefab())
            {
#if EDITOR_ONLY_NAVMESH_BUILDER_DEPRECATED
                UnityEditor.AI.NavMeshEditorHelpers.CollectSourcesInStage(
                    root, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, gameObject.scene, results);
#else
                UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                    root, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, gameObject.scene, results);
#endif
            }
            else
#endif
            {
                NavMeshBuilder.CollectSources(
                    root, includedLayerMask, geometry, areaByDefault, generateLinksByDefault,
                    markups, includeOnlyMarkedObjects, results);
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (m_NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == m_NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced NavMesh data", this);
                m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!m_OverrideVoxelSize)
                    m_VoxelSize = settings.agentRadius / 3.0f;
                if (m_VoxelSize < kMinVoxelSize)
                    m_VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!m_OverrideTileSize)
                    m_TileSize = kDefaultTileSize;

                // Make sure tilesize is in sane range.
                if (m_TileSize < kMinTileSize)
                    m_TileSize = kMinTileSize;
                if (m_TileSize > kMaxTileSize)
                    m_TileSize = kMaxTileSize;

                if (m_MinRegionArea < 0)
                    m_MinRegionArea = 0;
            }
        }

        static bool IsEditedInPrefab(NavMeshSurface navMeshSurface)
        {
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(navMeshSurface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(navMeshSurface);
            // if (isPrefab)
            //     Debug.Log($"NavMeshData from {navMeshSurface.gameObject.name}.{navMeshSurface.name} will not be added to the NavMesh world because the gameObject is a prefab.");
            return isPrefab;
        }

        internal bool IsPartOfPrefab()
        {
            var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            var isPartOfPrefab = prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject);
            return isPartOfPrefab;
        }
#endif
    }
}

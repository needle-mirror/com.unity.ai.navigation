# NavMesh Surface component reference

Use the NavMesh Surface component to define and build a [NavMesh](./Glossary.md#navmesh) for a specific type of [NavMesh Agent](./NavMeshAgent.md) in your scene. Use the available properties to specify the type of NavMesh Agent that can use the NavMesh, the area type to assign to the generated NavMesh, and the geometry to use when you bake the NavMesh.

To use the **NavMesh Surface** component, apply it to the [GameObject](./Glossary.md#gameobject) on which you want to build the NavMesh.

To apply the NavMesh Surface component to a GameObject, do the following:

1. Select the GameObject.
2. In the Inspector, select **Add Component** &gt; **Navigation** &gt; **NavMesh Surface**. <br/> The Inspector window displays the NavMesh Surface component.

In the NavMesh Surface component, you can click **Bake** to generate the NavMesh for the current settings and scene geometry. The resulting [NavMesh data](./NavMeshSurface.md#navmesh-surface-asset-file) replaces any NavMesh that **NavMesh Surface** already contains, if that is the case.
 A [**Scene**](./Glossary.md#scene) can contain multiple NavMesh surfaces. You can add the NavMesh Surface component to any GameObject in your scene. This is useful for when you want to use the GameObject parenting [hierarchy][1] to define which GameObjects contribute to the NavMesh. Only the NavMesh Surface components that are enabled and part of active GameObjects load their NavMesh data into the navigation system. You can unload NavMesh data from the scene by disabling either the **NavMesh Surface** that contains it or the GameObject that the NavMesh Surface is attached to.

<a id="navmesh-surface-main-settings"></a>
The following table describes the properties available in the NavMesh Surface component. Use the main settings for the NavMesh Surface component to filter the input geometry on a broad scale. Use the [NavMesh Modifier](./NavMeshModifier.md) component to adjust how Unity treats input geometry on a per-GameObject basis.

| Property | Description |
|---|---|
| **Agent Type** | Select the type of [NavMesh Agent](NavMeshAgent) that can use the NavMesh. |
| **Default Area** | Specify the area type to assign to the generated NavMesh. The area types define how difficult it is for agents to move across the NavMesh. The available options include all of the area types defined on the Areas tab of the Navigation window. There are 29 custom area types and 3 built-in area types: <br/>Available built-in options: <ul><li>**Walkable**: Make the NavMesh walkable for the assigned Agent type. (This is the default option.)</li><li>**Not Walkable**: Prevent the specified Agent type from crossing this surface unless there is a GameObject that [overrides the area type](NavMeshModifier).</li><li>**Jump**: This option is used for automatically generated links.</li></ul> For more details about area types, refer to [Navigation Areas and Costs](AreasAndCosts). |
| **Generate Links** | Automatically generate links between objects that the NavMesh Surface collects when you bake the NavMesh. If you select **Generate Links**, NavMesh Surface attempts to generate links between any collected GameObjects when you bake the NavMesh. If you do not select **Generate Links**, NavMesh Surface doesn't attempt to generate any links between the collected GameObjects when you bake the NavMesh.<br/> Refer to the [Links Generation](BuildingOffMeshLinksAutomatically#links-generation) section for more information. |
| **Use Geometry** | Select which geometry to use when you bake the NavMesh.<br/>Available options:<ul><li>**Render Meshes**: Use geometry from Render Meshes and [Terrains](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-UsingTerrains.html).</li><li>**Physics Colliders**: Use geometry from Colliders and Terrains. Agents can move closer to the edge of the physical bounds of the environment with this option than they can with the **Render Meshes** option. For more information on Colliders, refer to [Introduction to collision](https://docs.unity3d.com/6000.0/Documentation/Manual/CollidersOverview.html).</li></ul> The trees painted on a collected terrain contribute the render meshes or the colliders of their tree prefabs, based on this property. For more information, refer to [Terrain trees](./NavMeshSurface.md#terrain-trees). |
| **NavMesh Data** | (Read-only) Locate the asset file where the NavMesh is stored.</br> The text box displays **None** when the NavMesh Surface does not contain NavMesh data.</br> The text box displays **Missing** if you delete the asset file from the Project window and don't use **Clear** first.</br> |
| **Clear** | Remove the asset file where the NavMesh is stored. </br> Use this button also when you plan to remove the component. |
| **Bake** | Bake a NavMesh with the current settings. When you bake the NavMesh, it automatically excludes GameObjects that have a **NavMesh Agent** or **NavMesh Obstacle**. They are dynamic users of the NavMesh and don't contribute to the process. </br> Unity stores the NavMesh data in an asset file. The **NavMesh Data** property displays a reference to the asset file. |

## Object collection

Use the Object Collection settings to define which GameObjects to use when you bake the NavMesh.

| Property | Description |
|---|---|
| **Collect Objects** | Define which GameObjects to use when you bake the NavMesh.<br/>Available options:<ul><li>**All Game Objects**: Use all active GameObjects in the scene. (This is the default option.)</li><li>**Volume**: Use all active GameObjects that overlap the bounding volume. When you bake the NavMesh, Unity also includes geometry that lies outside the bounding volume but within the agent radius.</li><li>**Current Object Hierarchy**: Use the GameObject that holds the **NavMesh Surface** component and all of its active child GameObjects.</li><li>**NavMeshModifier Component Only**: Use every GameObject in the scene that has a **NavMesh Modifier** component. If you enable a modifier's **Apply To Children** option, also use the child GameObjects of that modifier's GameObject.</li></ul> |
| **Include Layers** | Select the layers for which GameObjects are included in the bake process. In addition to **Collect Objects**, this allows for further exclusion of specific GameObjects from the bake process (for example, effects or animated characters). This is set to **Everything** by default, but you can toggle options on (denoted by a check mark) or off, individually. |

Furthermore, you can use the [NavMesh Modifier](./NavMeshModifier.md) component to designate more precisely the objects, and their hierarchies, that the NavMesh Surface can or cannot collect.

## Advanced Settings

Use the Advanced settings section to customize the following additional properties:

| **Property**            | **Description**      |
|:------------------------|:---------------------|
| **Override Voxel Size** | Control how accurately Unity processes the input geometry when you bake the NavMesh. This is a trade-off between speed and accuracy. <br/> The default size is one third of the Agent [radius](./NavigationWindow.md#agents-tab), which translates into 3 [voxels](./NavInnerWorkings.md#about-voxels) per Agent radius. This voxel size allows the capture of narrow passages, such as doors, and maintains a quick baking time. For big open areas, you can use 1 or 2 voxels per radius to speed up baking. Tight indoor spots are better suited to smaller voxels, for example 4 to 6 voxels per radius. More than 8 voxels per radius doesn't usually provide much additional benefit. <br/> To change the default size, select this checkbox. In the **Voxel Size** field, specify the size of the voxels to use when you bake the NavMesh. |
| **Voxel Size**         | Specify the size, in world units, of the voxels to use when you bake the NavMesh. This property is only available if you select the **Override Voxel Size** option.|
| **Override Tile Size** | Change the default Tile Size of the NavMesh. To make the bake process parallel and memory efficient, the Scene is divided into tiles for baking. The white lines visible on the NavMesh are tile boundaries. <br/> The default tile size is 256 voxels, which provides a good trade-off between memory use and NavMesh fragmentation. <br/> To change this default tile size, select this checkbox. In the **Tile Size** field, specify the number of voxels you want the tile size to be. <br/> The smaller the tiles, the more fragmented the NavMesh is. This can sometimes cause non-optimal paths. NavMesh carving also operates on tiles. If you have a lot of obstacles in your scene, you can often speed up carving by making the tile size smaller (for example around 64 to 128 voxels). For more information, refer to [Carving](./AboutObstacles.md#carving). <br/> If you plan to bake the NavMesh at runtime, use a smaller tile size to keep the maximum memory use low.|
| **Tile Size**          | Specify the desired Tile Size in voxels. This property is only available if you select the **Override Tile Size** option. |
| **Minimum Region Area**| Remove small regions that are disconnected from the larger NavMesh. The process that builds the NavMesh doesn't retain the stretches of the mesh that have a surface size smaller than the specified value. <br/> **Note**: Some areas might not get removed despite the **Minimum Region Area** parameter. The NavMesh is built in parallel as a grid of tiles. If an area straddles a tile boundary, the area isn't removed. The reason for this is that the area pruning step takes place at a stage in the build process when the surrounding tiles aren't accessible. |
| **Build Height Mesh** | Generate additional data that specifies the height of the surface at each point on the NavMesh. Select this option to generate HeightMesh data. Clear this option if you do not want to generate HeightMesh data. For more information, refer to [**Build a HeightMesh for Accurate Character Placement**](./HeightMesh.md). |

## Terrain trees

When a **NavMesh Surface** component collects a [terrain](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-UsingTerrains.html), it also collects the trees painted on that terrain. Each tree contributes the geometry of its tree prefab. The **Use Geometry** property determines whether the **NavMesh Surface** uses the render meshes or the colliders of the prefab. As a result, agents move around the trees instead of through them.

The NavMesh Surface places each tree relative to the position of the terrain's GameObject. A terrain draws itself and its trees along the world axes, at the dimensions that its terrain data defines, and ignores the rotation and scale of its GameObject. The NavMesh Surface bakes the trees the same way, so the obstacles in the NavMesh match the trees you see.

### Include the trees of a terrain

The NavMesh Surface collects the trees of a terrain only when it collects that terrain. The following conditions apply:

- The terrain's own layer must be part of **Include Layers**. A layer assigned to a tree prefab has no effect, because a painted tree isn't a GameObject in the scene.
- If you set **Collect Objects** to **NavMeshModifier Component Only**, you must apply a NavMesh Modifier component to the terrain, even if its tree prefabs have one. Add the component to the terrain itself, or to a parent GameObject with **Apply To Children** enabled.
- If you set **Collect Objects** to **Volume**, the bounding volume must overlap the terrain. The NavMesh Surface excludes a tree that reaches into the volume from a terrain that the volume doesn't overlap.

The **NavMesh Surface** then filters each tree of a collected terrain based on the **Collect Objects** property. If you set **Collect Objects** to **NavMeshModifier Component Only**, the **NavMesh Surface** bakes a tree's geometry only when a **NavMesh Modifier** component in its tree prefab applies to that geometry. To bake all the geometry of a tree prefab, add the component to the root of the prefab and leave **Apply To Children** enabled. If you set **Collect Objects** to **Volume**, the **NavMesh Surface** bakes only the trees that overlap the bounding volume.

### Choose the LOD of a tree prefab

If the root of a tree prefab has an [LOD Group](https://docs.unity3d.com/6000.0/Documentation/Manual/class-LODGroup.html) component, you can choose which level of detail (LOD) of the prefab the NavMesh Surface uses when you set **Use Geometry** to **Render Meshes**. The NavMesh Surface uses the enabled Mesh Renderer components of the selected LOD. For a new tree, **NavMesh LOD Index** defaults to **Last**. If the selected LOD contains only a billboard, the tree adds no geometry to the NavMesh, so choose an LOD that has Mesh Renderer components. When you set **Use Geometry** to **Physics Colliders**, the NavMesh Surface uses the colliders of the prefab and ignores **NavMesh LOD Index**.

For more information, refer to [Add trees to the terrain](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-Trees.html).

To choose the LOD, follow these steps:

1. Select the terrain, and in its **Inspector** window, select the **Paint Trees** tool.
2. To add a tree prefab to the terrain's **Trees** list, select **Edit Trees** > **Add Tree**, then set **Tree Prefab** to the prefab. 
3. To change a tree prefab that's already in the list, select it in the **Trees** list, then select **Edit Trees** > **Edit Tree**.
4. In the window that opens, set **NavMesh LOD Index** to **First**, **Last**, or **Custom**. 
5. If you select **Custom**, set the index of the LOD.
6. Select **Add** or **Apply**.

The property applies to every tree painted from that entry of the **Trees** list, on every terrain GameObject that uses the same terrain data asset, because that asset stores the list.

### Assign area types with tree prefabs

To assign an area type to the NavMesh that forms on the trees of one tree prefab, follow these steps:

1. Add a NavMesh Modifier component to the root of the prefab, and keep **Mode** set to **Add or Modify Object**.
2. Enable **Override Area**.
3. Set **Area Type** to the area type you want.

The modifier applies to every instance of that prefab, on every terrain, for the agent types listed in its **Affected Agents** property.

A tree prefab can also have [NavMesh Modifier Volume](./NavMeshModifierVolume.md) components. For every instance of the tree prefab, the NavMesh inside each volume takes the area type of that volume, including the NavMesh on the terrain around the tree. A tree prefab that contributes no geometry and has only NavMesh Modifier Volume components adds no obstacle, so you can paint it across a terrain to assign area types to the NavMesh.

### Keep trees out of the NavMesh

To keep the trees of one tree prefab out of the NavMesh, add a NavMesh Modifier component to the root of the prefab and set its **Mode** to **Remove Object**. The modifier applies to every instance of that prefab, on every terrain, for the agent types listed in its **Affected Agents** property. **Remove Object** excludes only the geometry of the prefab. If the prefab also has NavMesh Modifier Volume components, the NavMesh inside those volumes still takes their area type. To stop the volumes from changing the area type, disable them or remove them from the prefab.

To keep the trees of every terrain out of the NavMesh across a whole project, go to **Edit** > **Project Settings** > **Player** > **Other Settings**. In the **Script Compilation** section, add `AI_NAVIGATION_IGNORE_TERRAIN_TREES` to **Scripting Define Symbols**. This symbol removes the code that collects terrain trees from the package entirely, so the trees add no time to the bake. When you define this symbol, the **NavMesh Surface** bakes terrains as earlier package versions did, without their trees. By contrast, a **NavMesh Modifier** component excludes trees when it bakes. The **NavMesh Surface** reads each tree prefab and its instances, then leaves out the trees that the modifier removes.

## Additional resources

- [About NavMesh agents](./AboutAgents.md)
- [Build a HeightMesh for Accurate Character Placement](./HeightMesh.md)
- [Links Generation](./BuildingOffMeshLinksAutomatically.md#links-generation)
- [Carving](./AboutObstacles.md#carving)
- [Hierarchy](https://docs.unity3d.com/6000.0/Documentation/Manual/Hierarchy.html)
- [Create a NavMesh agent](./CreateNavMeshAgent.md)
- [Navigation areas and costs](./AreasAndCosts.md)
- [Navigation agent configurations](./NavigationWindow.md#agents-tab)
- [NavMesh Modifier component reference](./NavMeshModifier.md)
- [NavMesh Modifier Volume component reference](./NavMeshModifierVolume.md)
- Physics [Colliders](https://docs.unity3d.com/6000.0/Documentation/Manual/CollidersOverview.html)
- [Terrains](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-UsingTerrains.html)

[1]: ./Glossary.md#hierarchy "Unity uses the concept of parent-child hierarchies, or parenting, to group GameObjects. An object can contain other GameObjects that inherit its properties."

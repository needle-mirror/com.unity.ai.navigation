# NavMesh Obstacle component reference

The NavMesh Obstacle component allows you to define obstacles that [NavMesh Agents](./AboutAgents.md) should avoid as they navigate the world (for example, barrels or crates controlled by the physics system). It contains properties that allow you to define the size, shape, and behavior of the obstacle.

To use the NavMesh component you need to add it to a game object as follows:
1. Select the GameObject you want to use as an obstacle.
2. In the Inspector select **Add Component**, then select **Navigation** &gt; **NavMesh Obstacle**. <br/> The NavMesh Obstacle component is displayed in the Inspector window.

You can use this component to create NavMesh obstacles. For more information, see [Create a NavMesh Obstacle](./CreateNavMeshObstacle.md). For more information on NavMesh obstacles and how to use them, see [About NavMesh obstacles](./AboutObstacles.md).

The following table describes the properties available in the NavMesh Obstacle component.

| Property | Description |
| --- | --- |
| **Shape** | Specify the shape of the obstacle geometry. Choose whichever one best fits the shape of the object. |
| **Shape** > **Box** | Select a cube-shaped geometry for the obstacle. |
| **Shape** > **Capsule** | Select a 3D oval-shaped geometry for the obstacle. |
| **Center** | Specify the center of the box relative to the transform position. |
| **Size** | Specify the size of the box. This property is visible only when Shape is set to Box. |
| **Center** | Specify the center of the capsule relative to the transform position. |
| **Radius** | Specify the radius of the capsule. This property is visible only when Shape is set to Capsule. |
| **Height** | Specify the height of the capsule. This property is visible only when Shape is set to Capsule. |
| **Carve** | Allow the NavMesh Obstacle to create a hole in the NavMesh. When selected, the NavMesh obstacle carves a hole in the NavMesh. When deselected, the NavMesh obstacle does not carve a hole in the NavMesh. |
| **Carve** > **Move Threshold** | Set the threshold distance for updating a moving carved hole. Unity treats the NavMesh obstacle as moving when it has moved more than the distance set by the Move Threshold. This property is available only when Carve is selected. |
| **Carve** > **Time To Stationary** | Specify the time (in seconds) to wait until the obstacle is treated as stationary. This property is available only when Carve is selected. |
| **Carve** > **Carve Only Stationary** | Specify when the obstacle is carved. This property is available only when Carve is selected. |



## Additional resources

- [About NavMesh obstacles](./AboutObstacles.md "Details on how to use NavMesh obstacles.")
- [Create a NavMesh Obstacle](./CreateNavMeshObstacle.md "Guidance on creating NavMesh obstacles.")
- [Inner Workings of the Navigation System](./NavInnerWorkings.md#two-cases-for-obstacles "Learn more about how NavMesh Obstacles are used as part of navigation.")
- [NavMesh Obstacle scripting reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.NavMeshObstacle.html "Full description of the NavMesh Obstacle scripting API.")

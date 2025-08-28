# NavMesh Link component reference

Use the NavMesh Link component to connect different NavMeshes built for the same [agent type](./NavigationWindow.md#agents-tab). The link can be a line from one point to another (no width), or a span (with width). If the link is a span (with width), the Agent uses the nearest location along the entry edge to cross the link. For example, you can use the **NavMesh Link** component to connect a NavMesh that represents a building’s interior to a NavMesh that represents the building’s exterior. You can't overlap separate NavMeshes to create a link between them.

To use the NavMesh Link you can either add it to your scene as a GameObject or add it to an existing GameObject as a component.

To add a NavMesh Link to your scene as a GameObject, do the following:
- From the main menu go to **GameObject** > **AI** > **NavMesh Link**.<br/> The **NavMesh Link** component is displayed in the **Inspector** window.

To add the NavMesh Link component to an existing GameObject, do the following:
1. Select the GameObject you want to add the component to.
2. In the Inspector select **Add Component**, then select **Navigation** &gt; **NavMesh Link**. <br/> The **NavMesh Link** component is displayed in the **Inspector** window.

| Property | Description |
| --- | --- |
| **Agent Type** | Specify which Agent type can use the link. |
| **Start Transform** | Select the GameObject that represents the start location of the link. This object is tracked by the middle of the link's start edge. |
| **Start Point** | Specify the start point of the link, relative to the GameObject's world-space position and orientation. The three values define the point's X, Y, and Z coordinates. Neither transform scale nor shear affect this point. <br/>The link uses this start position only when **Start Transform** does not reference any object. |
| **End Transform** | Select the GameObject that represents the end location of the link. This GameObject is tracked by the middle of the link's end edge. |
| **End Point** | Specify the end point of the link, relative to the GameObject's world-space position and orientation. The three values define the point's X, Y, and Z coordinates. Neither transform scale nor shear affect this point. <br/>The link uses this end position only when **End Transform** does not reference any object. |
| **Swap** | Swap the start and end points and swap the start and end transforms. |
| **Re-Center Origin** | Move the GameObject to the center point of the link and align the transform’s forward axis with the end point. |
| **Width** | Specify the width of the link. You can also drag the handles at the side of the link to adjust the width. <br/>**Note**: The GameObject's scale does not affect the width of the link. |
| **Cost Override** | Choose how to assign the cost to move across the link. <br/> Select **Cost Override** to set the cost value directly in the adjacent number field.<br/> Deselect **Cost Override** for the cost of the Area type to become the cost of moving over the NavMesh link. In this case the adjacent number field is disabled and not used. <br/> Path finding uses the cost in conjunction with the distance between the start and end positions in world space. For more information, refer to [Areas and costs](./AreasAndCosts.html#pathfinding-cost). |
| **Auto Update Positions** | Update the positions of the link's ends automatically when any of the GameObject transform, the start transform or the end transform change position. |
| **Bidirectional** | Control the direction NavMesh Agents move across the link. When you select this checkbox, NavMesh Agents can move across the link in both directions (from the start point to the end point, and from the end point to the start point).<br/>When you clear this checkbox, NavMesh Agents can only move across the link in one direction (from the start point to the end point). |
| **Area Type** | The area type of the NavMesh Link. The area type allows you to apply a common traversal cost to similar area types and prevent certain characters from accessing the NavMesh Link based on the agent’s Area Mask. For more information about area types and traversal costs, refer to [Areas and costs](./AreasAndCosts.html).<br/>The available options are:<ul><li>**Walkable**: Make the link walkable for the affected agent types. This is the default option.</li><li>**Not Walkable**: Prevent the affected agent types from crossing the link. Links with a **Not Walkable** area type do not connect to any NavMesh.</li><li>**Jump**: Change the area type of the link to **Jump**. This is the type that is assigned to all auto-generated NavMesh links.</li><li>**Open Area Settings**: Open the [Areas tab](./NavigationWindow.html#areas-tab) of the Navigation window to define new area types or modify existing ones.</li></ul> |
| **Activated** | Allow agents and queries to use the link in pathfinding. <br/>Select **Activated** to display the link's gizmo in the Scene view with black lines. <br/>Deselect **Activated** to display the link's gizmo in the Scene view with red lines. |

To adjust the ends of the link directly from the scene view you can drag the yellow handler gizmos at each end. A yellow cube represents the **Point** position for that end. In the opposite arrangement where the link references an object, a yellow sphere represents the **Transform** position of the referenced object. If you move the yellow sphere, the referenced object moves along to the same position.
To adjust the width of the link you can drag the orange dot handler gizmos placed on the sides of the link, at one third of the distance from the start to the end.
To display the handles, enable the NavMesh Link gizmo and select the GameObject. For more information on gizmos, refer to [Gizmos menu](https://docs.unity3d.com/6000.0/Documentation/Manual/GizmosMenu.html).

## Additional resources

- [About Agents](./NavigationWindow.md#agents-tab)
- [Areas and costs](./AreasAndCosts.md)
- [OffMesh Link component (deprecated) reference](./OffMeshLink.md)

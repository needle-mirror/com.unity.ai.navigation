# OffMesh Link component reference

> [!Important]
> The OffMesh Link component is deprecated and no longer supported. Use the [NavMesh Link component](./NavMeshLink.md) instead.

Use OffMesh Link components to incorporate navigation shortcuts, which can't be represented using a walkable surface, into your scene. For example, with OffMesh links, an agent can jump over a ditch or a fence, or open a door then walk through it.

OffMesh Links only apply to the Humanoid agent type, so if the NavMeshes in your scene use a different agent type, the OffMesh Link won't create a link between them.

The following table describes the properties available in the OffMesh Link component:

| Property | Description |
|---|---|
| **Start** | Select the GameObject that represents the start location of the link. |
| **End** | Select the GameObject that represents the end location of the link. |
| **Cost Override** | Override the cost to move across the link. <br/>If the Cost Override value is negative, the cost of the Navigation Area type is used. If the Cost Override value is non-negative, the cost of moving over the link is equal to the Cost Override value multiplied by the length of the link. The length of the link is the distance between the start and end points of the link. |
| **Bidirectional** | Control the direction NavMesh Agents move across the link. When you select this checkbox, NavMesh Agents can move across the link in both directions (from the start to the end, and from the end to the start).<br/>When you clear this checkbox, NavMesh Agents can only move across the link in one direction (from the start to the end). |
| **Activated** | Allow the link to be used in pathfinding. |
| **Auto Update Positions** | Reconnect the OffMesh link to the NavMesh if you move the end points. If disabled, the link stays at its start location even if the end points move. |
| **Navigation Area** | Specify the area type of the OffMesh Link. Use the area type to apply a common traversal cost to similar area types and prevent certain characters from crossing the link based on the agent’s Area Mask. |
| **Navigation Area** > **Walkable** | Make the link walkable for the affected agent types. This is the default option. |
| **Navigation Area** > **Not Walkable** | Prevent the affected agent types from crossing the link. |
| **Navigation Area** > **Jump** | Change the area type of the link to Jump. |


## Additional resources

- [NavMesh Link component reference](./NavMeshLink.md)
- [Create Off-Mesh Links (deprecated)](./CreateOffMeshLink.md)
- [Off-Mesh Link (deprecated) scripting reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AI.OffMeshLink.html)

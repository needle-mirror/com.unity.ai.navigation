# Create a NavMesh Link

Use NavMesh Links when you want to create paths that cross outside of the walkable [**navigation mesh**][1] surface. For example, you can set up NavMesh Links to allow agents to jump over a ditch or a fence, or to open a door before walking through it. To do this, you first need to add a NavMesh Link component to a GameObject in your [**scene**][2]. Then you position each end of the link at either a point in the scene or at the position of a GameObject that you choose to reference. When the ends are in place over the NavMesh, the Link creates a navigation connection that the [**NavMesh Agent**](./NavMeshAgent.md) can follow.

We’re going to add an NavMesh Link component to describe a jump from the upper platform to the ground.

1. First create two cylinders: **Game Object** > **3D Object** > **Cylinder**.
2. Scale the cylinders to (0.1, 0.05, 0.1) to make it easier to work with them.
3. Move the first cylinder to the edge of the top platform, close to the [**NavMesh**][1] surface.
4. Place the second cylinder on the ground, close to the NavMesh, at the location where the link should land.
5. Select the first cylinder and add a **NavMesh Link** component to it. In the inspector, select **Add Component** > **Navigation** > **NavMesh Link**.
6. In the **Start Transform** field, assign the first cylinder.
7. In the **End Transform** field, assign the second cylinder.

Now you have a functioning NavMesh Link set up. Pathfinding returns the path going through the NavMesh link if that path is shorter than walking along the NavMesh.

You can use any GameObject in the scene to hold the NavMesh link component, for example a fence [**prefab**][3] can contain the NavMesh link component. Similarly you can use any GameObject with a Transform as the start, or end, marker.

To learn about all the NavMesh Link properties that you can tweak, refer to the [NavMesh Link component reference](./NavMeshLink.md).

The NavMesh bake process can detect and create common jump-across and drop-down links automatically. Refer to  the settings of [**NavMesh Surface**](./NavMeshSurface.md) for more details.

## How to troubleshoot a link that does not work

![On the left, a correctly set up link with two rings. On the right, one ring is missing and the link appears faded.](./Images/OffMeshLinkDebug.svg)

If the agent does not traverse a NavMesh Link make sure that both end points are connected correctly to the NavMesh. To check the state of the connection make sure to enable the **Show NavMesh** debug visualization in the [AI Navigation overlay](./NavigationOverlay.md). When the link has no width, a properly connected end point shows a circle around the access point in the scene view. If the link has width, the link shows a dark segment on the edge that connects properly to the NavMesh, or a gray line if the edge does not connect to the NavMesh. If both ends connect to the NavMesh, the wide link shows an additional solid transparent rectangle that fills the space between the link edges. The NavMesh link also shows an arc line between the ends, with an arrow at each end where the agent can exit the link. The arc line is colored black if at least one end is connected, or it is colored gray if none of the ends is connected to the NavMesh.

No agent or path can traverse a link that has the **Activated** property disabled. In that situation the link shows in the scene with a red color. Make sure to enable the **Activated** property when you want agents to be able to move through the link.

Another common cause of why an agent does not traverse a NavMesh Link is that the NavMesh Agent’s _Area Mask_ does not include the NavMesh Link’s area type.

## Additional resources

- [NavMesh Link component reference](./NavMeshLink.md "Description of all the properties of the NavMesh Link component.")
- [Navigation HowTos](./NavHowTos.md "Common use cases for NavMesh Agent, with source code.")
- [Sample 7 - Dungeon](./Samples.md "An example of NavMesh links connecting NavMeshes at runtime.")
- [NavMesh Link scripting reference](../api/Unity.AI.Navigation.NavMeshLink.html "Full description of the NavMesh Link scripting API.")

[1]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

[2]: ./Glossary.md#scene "A Scene contains the environments and menus of your game. Think of each unique Scene file as a unique level. In each Scene, you place your environments, obstacles, and decorations, essentially designing and building your game in pieces."

[3]: ./Glossary.md#prefab "An asset type that allows you to store a GameObject complete with components and properties. The prefab acts as a template from which you can create new object instances in the scene."

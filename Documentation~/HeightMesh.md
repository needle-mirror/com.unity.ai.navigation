# Build a HeightMesh for Accurate Character Placement

Use a HeightMesh to place your character more accurately on walkable surfaces during navigation.

During navigation, the [**NavMesh Agent**](./AboutAgents.md) is constrained on the surface of the [**NavMesh**][1]. Since the NavMesh is an approximation of the walkable space, some features are evened out when the NavMesh is built. For example, stairs may appear as a slope in the NavMesh. If your game requires accurate placement of the agent, you can either add a **HeightMesh** to your **NavMesh**, or build a **HeightMesh** when you bake the **NavMesh**.

>[!Note]
>It takes extra memory and runtime processing to build a **HeightMesh**,  therefore, it will take longer to bake the NavMesh.

To add a HeightMesh to your NavMesh:

1. Open your scene in the **Editor**.
2. Select the scene geometry or **GameObject** that contains your **NavMesh**.
3. In the Inspector, expand the **NavMesh Surface** component, if necessary.
4. In the **NavMesh Surface** component, expand the **Advanced** section, then select **Build Height Mesh**.
5. When you are finished, click **Bake**. </br> The NavMesh is generated and displayed as a blue overlay and the HeightMesh as a pink overlay.

To build a **HeightMesh** when you bake the **NavMesh**:

1. Follow the instructions to [Create a NavMesh](./CreateNavMesh.md).
2. In the **NavMesh Surface** component, select **Build Height Mesh**.
3. When you are finished, click **Bake**. </br> The NavMesh is generated and displayed as a blue overlay and the HeightMesh as a pink overlay.

![HeightMesh example](./Images/HeightMesh-Example.png "A NavMesh Surface which contains accurate character placement data (HeightMesh). The blue area shows the NavMesh which is used for path finding. The pink area (including the area under the NavMesh) represents the HeightMesh which is used for more accurate placement of the Agent while it moves along the calculated path.")

## Additional resources

- [Create a NavMesh](./CreateNavMesh.md "Workflow for creating a NavMesh.")
- [NavMesh Surface component reference](./NavMeshSurface.md#advanced-settings "Use for specifying the settings for NavMesh baking.")
- [Sample 8 - Heightmesh](./Samples.md "An example of a NavMesh Agent that aligns precisely to the steps of a staircase.")

[1]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

# Create a NavMesh

You need to create a [**NavMesh**][1] to define an area of your scene within which a character can navigate intelligently.

To create a NavMesh do the following:
1. Select the scene geometry where you want to add the NavMesh.
2. In the Inspector window, click **Add Component**.
3. Select **Navigation** > **NavMesh Surface**.
4. In the NavMesh Surface component, specify the necessary settings. For details on the available settings, refer to [NavMesh Surface component](./NavMeshSurface.md).
5. When you are finished, click **Bake**. <br/>
The NavMesh is generated and displayed in the scene as a blue overlay on the underlying scene geometry whenever the Navigation window is open and visible. 

You can bake the NavMesh again to update it each time you make changes to either the scene geometry, the NavMesh [modifiers](./NavMeshModifier.md), the properties of the **NavMesh Surface** component, or [the settings](./NavigationWindow.md#agents-tab) of [the selected agent type](./NavMeshSurface.md#navmesh-surface-main-settings).

To permanently remove a NavMesh from your project, do one of the following: 

* Click the **Clear** button in the **NavMesh Surface** inspector. 
* Delete [the NavMesh asset file](./NavMeshSurface.md#navmesh-surface-asset-file) in the **Project** window. If you choose to remove the **NavMesh Surface** component itself from the GameObject, the asset file is not deleted, even though the NavMesh is no longer present in the scene.

## Additional resources

- [Navigation window](./NavigationWindow.md)
- [Create a NavMeshAgent](./CreateNavMeshAgent.md)
- [NavMesh Surface component](./NavMeshSurface.md)
- [Navigation Areas and Costs](./AreasAndCosts.md)
- [Build a HeightMesh for Accurate Character Placement](./HeightMesh.md)

[1]: ./Glossary.md#NavMesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

# NavMesh Modifier component reference

Use the NavMesh Modifier component to adjust the behavior of a [GameObject][1] when the [NavMesh][2] is baked at runtime. The NavMesh Modifier component affects the NavMesh during the generation process only. This means the NavMesh is updated to reflect any changes to NavMesh Modifier components when you bake the NavMesh. Use the available properties to specify changes in behavior and any limits to those changes.

To use the NavMesh Modifier component, add it to a GameObject as follows:
1. Select the GameObject whose effect on the NavMesh you want to modify.
2. In the Inspector, select **Add Component**, then select **Navigation** &gt; **NavMesh Modifier**. <br/> The NavMesh Modifier component is displayed in the Inspector window.

The NavMesh Modifier can also affect the NavMesh generation process hierarchically. This means that the GameObject the component is attached to, as well as all its children, are affected. In addition, you can place another NavMesh Modifier further down the hierarchy to override the NavMesh Modifier that is further up the hierarchy.

To apply the NavMesh Modifier hierarchically, select the **Apply To Children** property.

> [!Note]
> The NavMesh Modifier component replaces the legacy Navigation Static setting which you could enable from the Objects tab of the Navigation window and the Static flags dropdown on the GameObject. The NavMesh Modifier component is available for baking at runtime, whereas the Navigation Static flags were available in the Editor only.

The following table describes the properties available in the NavMesh Modifier component.

| Property | Description |
| --- | --- |
| **Mode** | Specify whether to consider or ignore the affected GameObject(s). <br/>Available options: <ul><li>**Add or Modify Object**: Consider the affected GameObject(s) when building the NavMesh.</li><li>**Remove Object**: Ignore the affected object(s) when building the NavMesh for the specified agent type.</li></ul> |
| **Affected Agents** | Specify which agents the NavMesh Modifier affects. For example, you can choose to have certain obstacles be ignored by specific agents. <br/>Available options: <ul><li>**All**: Modify the behavior of all agents.</li><li>**None**: Exclude all agents from the modified behavior.</li></ul> |
| **Apply to Children** | Apply the configuration to the child hierarchy of the GameObject. To override this component's influence further down the hierarchy, add another NavMesh Modifier component. |
| **Override Area** | Change the [area type](AreasAndCosts.md) for the affected GameObject(s). To do this, select this checkbox and then select the new area type from the **Area Type** dropdown that appears. |
| **Override Generate Links** | Force the NavMesh bake process to either include or ignore the affected GameObject(s) when you generate links. To do this, select this checkbox and then select or clear the **Generate Links** checkbox that appears. |


## Additional resources
- [Create a NavMesh](./CreateNavMesh.md)
- [Navigation Areas and Costs](./AreasAndCosts.md)
- [Navigation Agent Types](./NavigationWindow.md#agents-tab)

[1]: ./Glossary.md#gameobject "The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more."

[2]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

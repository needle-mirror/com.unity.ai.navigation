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
| **Mode** | Specify whether to consider or ignore the affected GameObject(s). |
| **Mode** > **Add or Modify Object** | Consider the affected GameObject(s) when building the NavMesh. |
| **Mode** > **Remove Object** | Ignore the affected object(s) when building the NavMesh for the specified agent type. |
| **Affected Agents** | Specify which agents the NavMesh Modifier affects. For example, you can choose to have certain obstacles be ignored by specific agents. |
| **Affected Agents** > **All** | Modify the behavior of all agents. |
| **Affected Agents** > **None** | Exclude all agents from the modified behavior. |
| **Apply to Children** | Apply the configuration to the child hierarchy of the GameObject. To override this component's influence further down the hierarchy, add another NavMesh Modifier component. |
| **Override Area** | Change the [area type](AreasAndCosts) for the affected GameObject(s). If you want to change the area type, select the checkbox then select the new area type in the Area Type dropdown. If you do not want to change the area type, clear the checkbox. |
| **Override Area**  > **Area Type** | Select the new area type you want to apply from the dropdown. |
| **Override Generate Links** | Force the NavMesh bake process to either include or ignore the affected GameObject(s) when you generate links. |
| **Override Generate Links** > **Generate Links** | Specify whether or not to include the affected GameObject(s) when you generate links. To include the GameObject(s) when you generate links in the NavMesh bake process, select this checkbox. To ignore the GameObject(s) when you generate links in the NavMesh bake process, clear this checkbox. |


## Additional resources
- [Create a NavMesh](./CreateNavMesh.md)
- [Navigation Areas and Costs](./AreasAndCosts.md)
- [Navigation Agent Types](./NavigationWindow.md#agents-tab)

[1]: ./Glossary.md#gameobject "The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more."

[2]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

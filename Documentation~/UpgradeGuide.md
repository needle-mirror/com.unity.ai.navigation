# Upgrade projects for use with AI Navigation package

Navigation and Pathfinding in Unity is handled by the AI Navigation package as of Unity 2022.2.

If you have projects that were created with the Navigation feature in previous versions of Unity, the AI Navigation package is automatically installed and added to your project. You can then do one of the following:

- Continue to use your projects as they are
- Convert your projects to use the new package

## Remove old component scripts

If your project uses the **NavMesh Surface**, **NavMesh Modifier**, **NavMesh Modifier Volume** or **NavMesh Link** components defined by scripts downloaded from [Unity’s NavMeshComponents GitHub repository](https://github.com/Unity-Technologies/NavMeshComponents), then remove those scripts and any associated files before you add the AI Navigation package to your project. If you don’t remove these scripts, you might get conflicts and errors related to these components in the Console. The new components mirror the same behavior as the old components do in your project except when using the following components:

- The **NavMesh Surface** component now includes an option to use only the objects that have a **NavMesh Modifier** in the baking process.
- You can now specify whether or not to apply the **NavMesh Modifier** component to child objects in the hierarchy.

## Convert your project

If you want to use the new package you need to convert your project(s). As part of the conversion process, the **Navigation Updater** makes the following changes:

- Any NavMesh that was previously baked and embedded in the scene is now referenced from a **NavMesh Surface** component created on a new GameObject
 called Navigation.
- Any object that was marked with Navigation Static now has a **NavMesh Modifier** component with the appropriate settings.

The updater can also convert **OffMesh Link** components to **NavMesh Link** components. Refer to [Convert OffMesh Link to NavMesh Link](#convert-offmesh-link-to-navmesh-link) for more information.

To convert your project do the following:

1. In the main menu go to **Window** > **AI** > **Navigation Updater**.
2. In the **Navigation Updater** window, select which kind of data to convert. 
    * To move NavMeshes from the internals of scenes to their own **NavMesh Surface** component, select **NavMesh Scene Converter**. 
    * To replace **OffMesh Link** components with matching **NavMesh Link** components, select **OffMesh Link Converter**.
3. Click **Initialize Converters** to detect and display the scenes and prefabs that are eligible for conversion.
4. Select the assets you want to convert and verify that their category is also selected.
5. Click **Convert Assets** to complete the conversion.

## Create new agent types

If the NavMeshes in different scenes are baked with different agent settings then you need to create new agent types to match those settings.

To create the agent types do the following:

1. In the main menu go to **Window** > **AI** > **Navigation**.
2. Select **Agents**.
3. Create new entries and specify the relevant settings.

### Assign new agent types
When you have created the new agent types you then need to assign them as follows:

- Assign the newly created agent types to their respective **NavMesh Surfaces** in the Navigation GameObject created for that scene.
- Assign the agent types to the **NavMesh Agents** intended to use that NavMesh.

To find the settings that were used for each existing NavMesh, select the NavMesh `.asset` file in the **Project** window. The NavMesh settings will be displayed in the **Inspector**.

## Create NavMesh Links instead of OffMesh Links

The **OffMesh Link** component was originally designed to work with only the **Humanoid** agent type. Now it has been deprecated. Your project can still use this component but you can no longer add it from the editor. You are encouraged to use the **NavMesh Link** component instead. It has the same properties as **OffMesh Link**, and a few additional ones: agent type, width, and two positions that can define the ends.

To replace an **OffMesh Link** component with a **NavMesh Link** component do the following:

1. Select the GameObject that has the **OffMesh Link** component. The GameObject can be in a scene or a prefab.
2. Add a **NavMesh Link** component.
3. Assign to the **NavMesh Link** the same properties as the **OffMesh Link**.
4. Remove the **OffMesh Link** from the GameObject.
5. Save the scene or prefab as needed.


### Convert OffMesh Links to NavMesh Links

To ease the transition from [OffMesh Link](./OffMeshLink.md) to [NavMesh Link](./NavMeshLink.md), the package comes with an upgrade utility to automatically change any **OffMesh Link** component into a **NavMesh Link** component. The upgrade utility scans all scenes and prefabs in the project to find all instances of **OffMesh Link** components.

To convert **OffMesh Link** components to **NavMesh Link** components do the following:
1. From the main menu go to **Window** > **AI** > **Navigation Updater**.
2. In the **Navigation Updater** window, verify that **OffMesh Link Converter** is checked.
3. Select **Initialize Converters** to detect and display the prefabs and scenes that are eligible for conversion.
4. Deselect any items you do not want to convert.
5. Select **Convert Assets** to complete the conversion.

Do note that the upgrade utility will not replace `OffMeshLink` with `NavMeshLink` in scripts. Refer to the following section for information on how to perform this upgrade manually.

### Replace OffMeshLink with NavMeshLink in scripts

In your scripts you can replace any occurrence of the `OffMeshLink` class with the `NavMeshLink` class. The scripts will continue to work as before, as long as the `NavMeshLink` component exists on the affected GameObjects. The `OffMeshLink` properties `autoUpdatePositions`, `biDirectional`, `costOverride` and the method `UpdatePositions()` have equivalents in the `NavMeshLink` component. You can substitute those class members in places where you use them in scripts, or you can accept the suggestion from the **Script Updating Consent** utility to do the same thing. This utility runs when the editor reloads the scripts in the project.

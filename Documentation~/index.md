# NavMesh Building Components

## Installation

This package is currently experimental and not visible in the [Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).

To install the package, follow [Adding a registry package by name](https://docs.unity3d.com/2021.2/Documentation/Manual/upm-ui-quick.html) instructions and add __com.unity.ai.navigation__.

## Details

NavMesh building components provides you with additional controls for automatically generating and using NavMeshes at run time and in the Unity Editor. For additional information about the process, visit the manual page about the legacy workflow for [baking a NavMesh](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html).


Here we introduce four high level components for the navigation system:

* [NavMeshSurface](NavMeshSurface.md) - Use for building and enabling a NavMesh surface for one type of Agent.
* [NavMeshModifier](NavMeshModifier.md) - Use for affecting the NavMesh generation of NavMesh area types based on the transform hierarchy.
* [NavMeshModifierVolume](NavMeshModifierVolume.md) - Use for affecting the NavMesh generation of NavMesh area types based on volume.
* [NavMeshLink](NavMeshLink.md) - Use for connecting the same or different NavMesh surfaces for one type of Agent.

For more details about NavMesh area types, see documentation on [NavMesh areas](https://docs.unity3d.com/Manual/nav-AreasAndCosts.html).


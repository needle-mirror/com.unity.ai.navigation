# Glossary

## Animation blend tree
Used for continuous blending between similar Animation Clips based on float Animation Parameters. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/class-BlendTree.html)

## Animation clip
Animation data that can be used for animated characters or simple animations. An animation clip is one piece of motion, such as (one specific instance of) “Idle”, “Walk” or “Run”. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/class-AnimationClip.html)

## Animation parameters
Used to communicate between scripting and the Animator Controller. Some parameters can be set in scripting and used by the controller, while other parameters are based on Custom Curves in Animation Clips and can be sampled using the scripting API. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimationParameters.html)

## Animator window
The window where the Animator Controller is visualized and edited. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimatorWindow.html)

## Collider
An invisible shape that is used to handle physical collisions for an object. A collider doesn’t need to be exactly the same shape as the object’s mesh - a rough approximation is often more efficient and indistinguishable in gameplay. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/CollidersOverview.html)

## Collision
A collision occurs when the physics engine detects that the colliders of two GameObjects make contact or overlap, and at least one has a Rigidbody component and is in motion. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/CollidersOverview.html)

## GameObject
The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/class-GameObject.html)

## HeightMesh
A navmesh that contains additional data that is used to more accurately determine the height at any point along the navmesh. [More info](./HeightMesh.md)

## Hierarchy
Unity uses the concept of parent-child hierarchies, or parenting, to group GameObjects. An object can contain other GameObjects that inherit its properties. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/Hierarchy.html)

## Input Geometry
The geometry to consider when baking the navmesh. [More info](./NavMeshSurface.md#Object-Collection)

## Inspector
A Unity window that displays information about the currently selected GameObject, asset or project settings, allowing you to inspect and edit the values. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/UsingTheInspector.html)

## Mesh
The main graphics primitive of Unity. Meshes make up a large part of your 3D worlds. Unity supports triangulated or Quadrangulated polygon meshes. Nurbs, Nurms, Subdiv surfaces must be converted to polygons. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/mesh-introduction.html)

## NavMesh
A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation. [More info](./CreateNavMesh.md)

## Prefab
An asset type that allows you to store a GameObject complete with components and properties. The prefab acts as a template from which you can create new object instances in the scene. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/Prefabs.html)

## Rasterization
The process of generating an image by calculating pixels for each polygon or triangle in the geometry. This is an alternative to ray tracing.

## Rigidbody
A component that allows a GameObject to be affected by simulated gravity and other forces. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/class-Rigidbody.html)

## Root motion
Motion of character’s root node, whether it’s controlled by the animation itself or externally. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/RootMotion.html)

## Scene
 A Scene contains the environments and menus of your game. Think of each unique Scene file as a unique level. In each Scene, you place your environments, obstacles, and decorations, essentially designing and building your game in pieces. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/CreatingScenes.html)

## Scripts
A piece of code that allows you to create your own Components, trigger game events, modify Component properties over time and respond to user input in any way you like. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/CreatingAndUsingScripts.html)

## Terrain
The landscape in your scene. A Terrain GameObject adds a large flat plane to your scene and you can use the Terrain’s Inspector window to create a detailed landscape. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/terrain-UsingTerrains.html)

## Unity unit
The unit size used in Unity projects. By default, 1 Unity unit is 1 meter. To use a different scale, set the Scale Factor in the Import Settings when importing assets. [More info](https://docs.unity3d.com/6000.0/Documentation/Manual/ImportingModelFiles.html#model)

## Voxel
A 3D pixel. [More info](./NavInnerWorkings.md#About-Voxels)

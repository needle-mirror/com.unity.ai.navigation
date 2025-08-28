# Navigation Samples

The following sample scenes are included with the AI Navigation package:

1. **Multiple Agent Sizes**: Demonstrates how a different radius on an agent type can change the way agents navigate through the same scene.

2. **Drop Plank**: Demonstrates dynamically changing walkable paths by allowing the player to add walkable planks by pressing space.

3. **Free Orientation**: Demonstrates a controllable agent that can walk on a tilted plane.

4. **Sliding Window Infinite**: Demonstrates a controllable agent that can walk through a dynamically created world that gets updated to simulate infinity as the agent walks through it. The NavMesh is only built in some set bounds that follow the agent.

5. **Sliding Window Terrain**: Demonstrates a controllable agent that can walk through a terrain for which the NavMesh is only generated within a set distance of the agent.

6. **Modify Mesh**: Demonstrates agents walking aimlessly on planes whose mesh can be modified dynamically by the player.

7. **Dungeon**: Demonstrates a controllable agent that can walk through a maze generated from pre-baked tiles that connect to each other at runtime. The link traversal animation can be modified with some presets (teleport, normal speed, parabola, curve).

8. **Height Mesh**: Demonstrates two agents walking down stairs. The environment on the left uses `NavMeshSurface` with a Height Mesh which allows the agent to snap to each step in the stairs as it goes down. The environment on the right uses a `NavMeshSurface` with no Height Mesh; the agent simply slides down the stairs.

9. **Cornering Speed Control**: Demonstrates how to control an agent's speed based on the sharpness of the upcoming turn in the path.

Install ```com.unity.shadergraph``` when you import the samples into a project that uses the Built-in Render Pipeline.

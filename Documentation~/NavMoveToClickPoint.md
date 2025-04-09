# Move an Agent to a Position Clicked by the Mouse

This script lets you choose the destination point on the [**NavMesh**][1] by clicking the mouse on the object’s surface. The position of the click is determined by a _raycast_, rather like pointing a laser beam at the object to see where it hits (see the page [Rays from the Camera][2] for a full description of this technique). Since the [GetComponent][3] function is fairly slow to execute, the script stores its result in a variable during the _Start_ function rather than call it repeatedly in _Update_.

``` C#
    // MoveToClickPoint.cs
    using UnityEngine;
    using UnityEngine.AI;

    public class MoveToClickPoint : MonoBehaviour {
        NavMeshAgent agent;

        void Start() {
            agent = GetComponent<NavMeshAgent>();
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
                    agent.destination = hit.point;
                }
            }
        }
    }
```

[1]: ./Glossary.md#navmesh "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."

[2]: https://docs.unity3d.com/6000.0/Documentation/Manual/CameraRays.html

[3]: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.GetComponent.html

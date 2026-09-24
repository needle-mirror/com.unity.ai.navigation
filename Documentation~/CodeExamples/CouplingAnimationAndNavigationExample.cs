#region ClickToMove
// ClickToMove.cs
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves the attached NavMeshAgent to the point clicked on in the scene.
/// </summary>
[RequireComponent (typeof (NavMeshAgent))]
public class ClickToMove : MonoBehaviour {
    RaycastHit hitInfo = new RaycastHit();
    NavMeshAgent agent;

    void Start () {
        agent = GetComponent<NavMeshAgent> ();
    }
    void Update () {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
                agent.destination = hitInfo.point;
        }
    }
}
#endregion

// Additional snippets from CouplingAnimationAndNavigation.md that build on
// LocomotionSimpleAgent.cs (see LocomotionSimpleAgentExample.cs) and LookAt.cs
// (see LookAtExample.cs). Kept in their own host class so they compile without
// redeclaring those types.
/// <summary>
/// Hosts standalone code snippets referenced from the
/// CouplingAnimationAndNavigation documentation page.
/// </summary>
[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (Animator))]
public class LocomotionSimpleAgentSnippets : MonoBehaviour {
    Animator anim;
    NavMeshAgent agent;

    void LookAtExampleMethod ()
    {
        #region LookAtExample
        LookAt lookAt = GetComponent<LookAt> ();
        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;
        #endregion
    }

    #region OnAnimatorMove
    void OnAnimatorMove ()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = anim.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
    }
    #endregion

    void PullCharacterTowardsAgentExample ()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        #region PullCharacterTowardsAgent
        // Pull character towards agent
        if (worldDeltaPosition.magnitude > agent.radius)
            transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
        #endregion
    }

    void PullAgentTowardsCharacterExample ()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        #region PullAgentTowardsCharacter
        // Pull agent towards character
        if (worldDeltaPosition.magnitude > agent.radius)
            agent.nextPosition = transform.position + 0.9f * worldDeltaPosition;
        #endregion
    }
}

#region MoveTo
// MoveTo.cs
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sends the attached NavMeshAgent to the assigned goal transform.
/// </summary>
public class MoveTo : MonoBehaviour {

    /// <summary>
    /// The destination the agent will move to.
    /// </summary>
    public Transform goal;

    void Start () {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;
    }
}
#endregion

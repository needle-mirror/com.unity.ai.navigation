#region LookAt
// LookAt.cs
using UnityEngine;
using System.Collections;

/// <summary>
/// Uses Animator IK to make the character's head look towards a target
/// position. Requires IK Pass to be enabled on the Animator Controller layer.
/// </summary>
[RequireComponent (typeof (Animator))]
public class LookAt : MonoBehaviour {
    /// <summary>
    /// The transform of the character's head bone that the IK look-at is applied to.
    /// </summary>
    public Transform head = null;

    /// <summary>
    /// The world-space position the character should look at.
    /// </summary>
    public Vector3 lookAtTargetPosition;

    /// <summary>
    /// The time it takes to blend the look-at weight down when not looking.
    /// </summary>
    public float lookAtCoolTime = 0.2f;

    /// <summary>
    /// The time it takes to blend the look-at weight up when looking.
    /// </summary>
    public float lookAtHeatTime = 0.2f;

    /// <summary>
    /// Whether the character is currently looking at <see cref="lookAtTargetPosition"/>.
    /// </summary>
    public bool looking = true;

    private Vector3 lookAtPosition;
    private Animator animator;
    private float lookAtWeight = 0.0f;

    void Start ()
    {
        if (!head)
        {
            Debug.LogError("No head transform - LookAt disabled");
            enabled = false;
            return;
        }
        animator = GetComponent<Animator> ();
        lookAtTargetPosition = head.position + transform.forward;
        lookAtPosition = lookAtTargetPosition;
    }

    void OnAnimatorIK ()
    {
        lookAtTargetPosition.y = head.position.y;
        float lookAtTargetWeight = looking ? 1.0f : 0.0f;

        Vector3 curDir = lookAtPosition - head.position;
        Vector3 futDir = lookAtTargetPosition - head.position;

        curDir = Vector3.RotateTowards(curDir, futDir, 6.28f * Time.deltaTime, float.PositiveInfinity);
        lookAtPosition = head.position + curDir;

        float blendTime = lookAtTargetWeight > lookAtWeight ? lookAtHeatTime : lookAtCoolTime;
        lookAtWeight = Mathf.MoveTowards (lookAtWeight, lookAtTargetWeight, Time.deltaTime / blendTime);
        animator.SetLookAtWeight (lookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
        animator.SetLookAtPosition (lookAtPosition);
    }
}
#endregion

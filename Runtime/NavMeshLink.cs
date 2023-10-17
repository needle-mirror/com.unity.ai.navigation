using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#pragma warning disable IDE1006 // Unity-specific lower case public property names

namespace Unity.AI.Navigation
{
    /// <summary> Component used to create a navigable link between two NavMesh locations. </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL(HelpUrls.Manual + "NavMeshLink.html")]
    public partial class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;

        [SerializeField]
        Vector3 m_StartPoint = new(0.0f, 0.0f, -2.5f);

        [SerializeField]
        Vector3 m_EndPoint = new(0.0f, 0.0f, 2.5f);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        [SerializeField]
        Transform m_StartTransform;

        [SerializeField]
        Transform m_EndTransform;

        [SerializeField]
        bool m_Activated = true;
#endif
        [SerializeField]
        float m_Width;

        [SerializeField]
        float m_CostModifier = -1;

        [SerializeField]
        bool m_Bidirectional = true;

        [SerializeField]
        bool m_AutoUpdatePosition;

        [SerializeField]
        int m_Area;

        /// <summary> Gets or sets the type of agent that can use the link. </summary>
        public int agentTypeID
        {
            get => m_AgentTypeID;
            set
            {
                if (value == m_AgentTypeID)
                    return;

                m_AgentTypeID = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets the local position at the middle of the link's start edge, relative to the GameObject origin. </summary>
        /// <remarks> The position is translated and rotated by <see cref="startTransform"/> when `startTransform` is `null` or equal to the GameObject transform. Otherwise, the link is only translated by `startTransform`. The scale of the specified transform is never used.</remarks>
        public Vector3 startPoint
        {
            get => m_StartPoint;
            set
            {
                if (value == m_StartPoint)
                    return;

                m_StartPoint = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets the local position at the middle of the link's end edge, relative to the GameObject origin. </summary>
        /// <remarks> The position is translated and rotated by <see cref="endTransform"/> when `endTransform` is `null` or equal to the GameObject transform. Otherwise, the link is only translated by `endTransform`. The scale of the specified transform is never used.</remarks>
        public Vector3 endPoint
        {
            get => m_EndPoint;
            set
            {
                if (value == m_EndPoint)
                    return;

                m_EndPoint = value;
                UpdateLink();
            }
        }

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        /// <summary> Gets or sets the <see cref="Transform"/> tracked by the middle of the link's start edge. </summary>
        /// <remarks> When this property is `null` or equal to the GameObject transform, it applies the GameObject's translation and rotation as a transform to <see cref="startPoint"/> in order to establish the world position of the link's start edge. When this property takes any other value, it applies only its translation to `startPoint`.</remarks>
        public Transform startTransform
        {
            get => m_StartTransform;
            set
            {
                if (value == m_StartTransform)
                    return;

                m_StartTransform = value;
                if (m_StartTransform != null)
                    m_StartPoint = Vector3.zero;

                UpdateLink();
            }
        }

        /// <summary> Gets or sets the Transform tracked by the middle of the link's end edge. </summary>
        /// <remarks> When this property is `null` or equal to the GameObject transform, it applies the GameObject's translation and rotation as a transform to <see cref="endPoint"/> in order to establish the world position of the link's end edge. When this property takes any other value, it applies only its translation to the `endPoint`.</remarks>
        public Transform endTransform
        {
            get => m_EndTransform;
            set
            {
                if (value == m_EndTransform)
                    return;

                m_EndTransform = value;
                if (m_EndTransform != null)
                    m_EndPoint = Vector3.zero;

                UpdateLink();
            }
        }

        internal bool startRelativeToThisGameObject => m_StartTransform == null || m_StartTransform == transform;
        internal bool endRelativeToThisGameObject => m_EndTransform == null || m_EndTransform == transform;
#endif

        // Start position relative to the game object position
        internal Vector3 localStartPosition
        {
            get
            {
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
                if (!startRelativeToThisGameObject)
                    return transform.InverseTransformPoint(m_StartTransform.position + m_StartPoint);

                return m_StartPoint;
#else
                return m_StartPoint;
#endif
            }
        }

        // End position relative to the game object position
        internal Vector3 localEndPosition
        {
            get
            {
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
                if (!endRelativeToThisGameObject)
                    return transform.InverseTransformPoint(m_EndTransform.position + m_EndPoint);

                return m_EndPoint;
#else
                return m_EndPoint;
#endif
            }
        }

        /// <summary> The width of the segments making up the ends of the link. </summary>
        /// <remarks> The segments are created perpendicular to the line from start to end, in the XZ plane of the GameObject. </remarks>
        public float width
        {
            get => m_Width;
            set
            {
                if (value.Equals(m_Width))
                    return;

                m_Width = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets a value that determines the cost of traversing the link.</summary>
        /// <remarks> A negative value implies that the traversal cost is obtained based on the area type.
        /// A positive or zero value applies immediately, overriding the cost associated with the area type.</remarks>
        public float costModifier
        {
            get => m_CostModifier;
            set
            {
                if (value.Equals(m_CostModifier))
                    return;

                m_CostModifier = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets whether the link can be traversed in both directions. </summary>
        /// <remarks> A link that connects to NavMeshes at both ends can always be traversed from the start position to the end position. When this property is set to `true` it allows the agents to traverse the link also in the direction from end to start. When the value is `false` the agents will never move over the link from the end position to the start position.</remarks>
        public bool bidirectional
        {
            get => m_Bidirectional;
            set
            {
                if (value == m_Bidirectional)
                    return;

                m_Bidirectional = value;
                UpdateLink();
            }
        }

        /// <summary> Gets or sets whether the world positions of the link's edges update whenever
        /// the GameObject transform changes at runtime. </summary>
        public bool autoUpdate
        {
            get => m_AutoUpdatePosition;
            set
            {
                if (value == m_AutoUpdatePosition)
                    return;

                m_AutoUpdatePosition = value;

                if (m_AutoUpdatePosition)
                    AddTracking(this);
                else
                    RemoveTracking(this);
            }
        }

        /// <summary> The area type of the link. </summary>
        public int area
        {
            get => m_Area;
            set
            {
                if (value == m_Area)
                    return;

                m_Area = value;
                UpdateLink();
            }
        }

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        /// <summary> Gets or sets whether the link can be traversed by agents. </summary>
        /// <remarks> When this property is set to `true` it allows the agents to traverse the link. When the value is `false` no paths pass through this link and no agent can traverse it as part of their autonomous movement. </remarks>
        public bool activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
            }
        }

        /// <summary> Checks whether any agent occupies the link at this moment in time. </summary>
        /// <remarks> This property evaluates the internal state of the link every time it is used. </remarks>
        public bool occupied => NavMesh.IsLinkOccupied(m_LinkInstance);
#endif

        NavMeshLinkInstance m_LinkInstance;

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        bool m_StartTransformWasEmpty = true;
        bool m_EndTransformWasEmpty = true;
        Vector3 m_LastStartWorldPosition = Vector3.positiveInfinity;
        Vector3 m_LastEndWorldPosition = Vector3.positiveInfinity;
#endif
        Vector3 m_LastPosition = Vector3.positiveInfinity;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && NavMesh.IsLinkValid(m_LinkInstance))
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            NavMesh.RemoveLink(m_LinkInstance);
        }

        /// <summary> Replaces the link with a new one using the current settings. </summary>
        public void UpdateLink()
        {
            if (!isActiveAndEnabled)
                return;

            NavMesh.RemoveLink(m_LinkInstance);
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif
            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (NavMesh.IsLinkValid(m_LinkInstance))
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif
            var link = new NavMeshLinkData
            {
                startPosition = localStartPosition,
                endPosition = localEndPosition,
                width = m_Width,
                costModifier = m_CostModifier,
                bidirectional = m_Bidirectional,
                area = m_Area,
                agentTypeID = m_AgentTypeID,
            };
            m_LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (NavMesh.IsLinkValid(m_LinkInstance))
            {
                NavMesh.SetLinkOwner(m_LinkInstance, this);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
                NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
#endif
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            RecordEndpointTransforms();

            m_LastStartWorldPosition = transform.TransformPoint(localStartPosition);
            m_LastEndWorldPosition = transform.TransformPoint(localEndPosition);
#endif
        }

        internal void RecordEndpointTransforms()
        {
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            m_StartTransformWasEmpty = m_StartTransform == null;
            m_EndTransformWasEmpty = m_EndTransform == null;
#endif
        }

        internal bool HaveTransformsChanged()
        {
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            if (m_StartTransform == null && m_EndTransform == null &&
                m_StartTransformWasEmpty && m_EndTransformWasEmpty &&
                transform.position == m_LastPosition && transform.rotation == m_LastRotation)
                return false;

            var startWorldPos = startRelativeToThisGameObject ? transform.TransformPoint(m_StartPoint) : m_StartTransform!.position + m_StartPoint;
            if (startWorldPos != m_LastStartWorldPosition)
                return true;

            var endWorldPos = endRelativeToThisGameObject ? transform.TransformPoint(m_EndPoint) : m_EndTransform!.position + m_EndPoint;
            return endWorldPos != m_LastEndWorldPosition;
#else
            if (m_LastPosition != transform.position)
                return true;
            if (m_LastRotation != transform.rotation)
                return true;

            return false;
#endif
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HaveTransformsChanged())
                    instance.UpdateLink();

                instance.RecordEndpointTransforms();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Width = Mathf.Max(0.0f, m_Width);

            if (!NavMesh.IsLinkValid(m_LinkInstance))
                return;

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            if (m_StartTransform == null)
                m_StartTransform = transform;

            if (m_EndTransform == null)
                m_EndTransform = transform;
#endif
            if (!UnityEditor.EditorApplication.isPlaying)
                UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}

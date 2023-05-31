using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshLink))]
    class NavMeshLinkEditor : UnityEditor.Editor
    {
        SerializedProperty m_AgentTypeID;
        SerializedProperty m_Area;
        SerializedProperty m_CostModifier;
        SerializedProperty m_AutoUpdatePosition;
        SerializedProperty m_Bidirectional;
        SerializedProperty m_EndPoint;
        SerializedProperty m_StartPoint;
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        SerializedProperty m_EndTransform;
        SerializedProperty m_StartTransform;
        SerializedProperty m_Activated;
#endif
        SerializedProperty m_Width;

        static int s_SelectedID;
        static int s_SelectedPoint = -1;

        static Color s_HandleColor = new Color(255f, 167f, 39f, 210f) / 255;
        static Color s_HandleColorDisabled = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;

        void OnEnable()
        {
            m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            m_Area = serializedObject.FindProperty("m_Area");
            m_CostModifier = serializedObject.FindProperty("m_CostModifier");
            m_AutoUpdatePosition = serializedObject.FindProperty("m_AutoUpdatePosition");
            m_Bidirectional = serializedObject.FindProperty("m_Bidirectional");
            m_EndPoint = serializedObject.FindProperty("m_EndPoint");
            m_StartPoint = serializedObject.FindProperty("m_StartPoint");
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            m_EndTransform = serializedObject.FindProperty("m_EndTransform");
            m_StartTransform = serializedObject.FindProperty("m_StartTransform");
            m_Activated = serializedObject.FindProperty("m_Activated");
#endif
            m_Width = serializedObject.FindProperty("m_Width");

            s_SelectedID = 0;
            s_SelectedPoint = -1;

#if !UNITY_2022_2_OR_NEWER
            NavMeshVisualizationSettings.showNavigation++;
#endif
        }

#if !UNITY_2022_2_OR_NEWER
        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }
#endif

        static Matrix4x4 UnscaledLocalToWorldMatrix(Transform t)
        {
            return Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
        }

        static void AlignTransformToEndPoints(NavMeshLink navLink)
        {
            var mat = UnscaledLocalToWorldMatrix(navLink.transform);

            var worldStartPt = mat.MultiplyPoint(navLink.localStartPosition);
            var worldEndPt = mat.MultiplyPoint(navLink.localEndPosition);

            var forward = worldEndPt - worldStartPt;
            var up = navLink.transform.up;

            // Flatten
            forward -= Vector3.Dot(up, forward) * up;

            var transform = navLink.transform;
            transform.rotation = Quaternion.LookRotation(forward, up);
            transform.position = (worldEndPt + worldStartPt) * 0.5f;
            transform.localScale = Vector3.one;

            if (navLink.startRelativeToThisGameObject)
                navLink.startPoint = transform.InverseTransformPoint(worldStartPt);
            if (navLink.endRelativeToThisGameObject)
                navLink.endPoint = transform.InverseTransformPoint(worldEndPt);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", m_AgentTypeID);
            EditorGUILayout.Space();

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            m_StartPoint.isExpanded = EditorGUILayout.Foldout(m_StartPoint.isExpanded, "Positions");
            if (m_StartPoint.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_StartTransform);
                if (EditorGUI.EndChangeCheck())
                    m_StartPoint.vector3Value = Vector3.zero;

                EditorGUILayout.PropertyField(m_StartPoint);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_EndTransform);
                if (EditorGUI.EndChangeCheck())
                    m_EndPoint.vector3Value = Vector3.zero;

                EditorGUILayout.PropertyField(m_EndPoint);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button(new GUIContent("Swap", "Alter link direction by swapping the start and end")))
                {
                    Swap(targets);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button(new GUIContent("Re-Center Origin",
                        "Place this GameObject at the middle point between the start and end of this link.")))
                {
                    foreach (var nml in targets)
                    {
                        var navLink = (NavMeshLink)nml;
                        Undo.RecordObject(navLink.transform, "Align Transform to End Points");
                        Undo.RecordObject(navLink, "Align Transform to End Points");
                        AlignTransformToEndPoints(navLink);
                    }

                    SceneView.RepaintAll();
                }

                GUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
#else
            EditorGUILayout.PropertyField(m_StartPoint);
            EditorGUILayout.PropertyField(m_EndPoint);

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Swap"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    var tmp = navLink.startPoint;
                    navLink.startPoint = navLink.endPoint;
                    navLink.endPoint = tmp;
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Align Transform"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    Undo.RecordObject(navLink.transform, "Align Transform to End Points");
                    Undo.RecordObject(navLink, "Align Transform to End Points");
                    AlignTransformToEndPoints(navLink);
                }
                SceneView.RepaintAll();
            }
            GUILayout.EndHorizontal();
#endif

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Width);
            EditorGUILayout.PropertyField(m_CostModifier);
            EditorGUILayout.PropertyField(m_AutoUpdatePosition);
            EditorGUILayout.PropertyField(m_Bidirectional);

            NavMeshComponentsGUIUtility.AreaPopup("Area Type", m_Area);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            EditorGUILayout.PropertyField(m_Activated);
#endif
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        private static void Swap(Object[] targets)
        {
            foreach (NavMeshLink navLink in targets)
            {
                var tmpStartPoint = navLink.startPoint;
                var tmpStartTransform = navLink.startTransform;
                navLink.startTransform = navLink.endTransform;
                navLink.startPoint = navLink.endPoint;
                navLink.endTransform = tmpStartTransform;
                navLink.endPoint = tmpStartPoint;
            }
        }
#endif
        static Vector3 CalcLinkRight(NavMeshLink navLink)
        {
            var dir = navLink.localEndPosition - navLink.localStartPosition;
            return (new Vector3(-dir.z, 0.0f, dir.x)).normalized;
        }

        static void DrawLink(NavMeshLink navLink)
        {
            var right = CalcLinkRight(navLink);
            var rad = navLink.width * 0.5f;
            var edgeRadius = right * rad;
            var startPos = navLink.localStartPosition;
            var endPos = navLink.localEndPosition;

            var corners = new Vector3[4]
            {
                startPos - edgeRadius,
                startPos + edgeRadius,
                endPos + edgeRadius,
                endPos - edgeRadius 
            };
            Gizmos.DrawLineStrip(corners, true);
        }

#if !UNITY_2022_2_OR_NEWER
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
#else
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active | GizmoType.Pickable)]
#endif
        static void RenderBoxGizmo(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying && navLink.isActiveAndEnabled && navLink.HaveTransformsChanged())
                navLink.UpdateLink();

            var color = s_HandleColor;
            if (!navLink.enabled)
                color = s_HandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

            Gizmos.color = color;
            DrawLink(navLink);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying && navLink.isActiveAndEnabled && navLink.HaveTransformsChanged())
                navLink.UpdateLink();

#if !UNITY_2022_2_OR_NEWER
            if (NavMeshVisualizationSettings.showNavigation > 0)
#endif
            {
                var color = s_HandleColor;
                if (!navLink.enabled)
                    color = s_HandleColorDisabled;

                var oldColor = Gizmos.color;
                var oldMatrix = Gizmos.matrix;

                Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

                Gizmos.color = color;
                DrawLink(navLink);

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        public void OnSceneGUI()
        {
            var navLink = (NavMeshLink)target;
            if (!navLink.enabled)
                return;

            var mat = UnscaledLocalToWorldMatrix(navLink.transform);

            var worldStartPt = mat.MultiplyPoint(navLink.localStartPosition);
            var worldEndPt = mat.MultiplyPoint(navLink.localEndPosition);
            var worldMidPt = Vector3.Lerp(worldStartPt, worldEndPt, 0.35f);
            var startSize = HandleUtility.GetHandleSize(worldStartPt);
            var endSize = HandleUtility.GetHandleSize(worldEndPt);
            var midSize = HandleUtility.GetHandleSize(worldMidPt);

            var zup = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            var right = mat.MultiplyVector(CalcLinkRight(navLink));

            var oldColor = Handles.color;
            Handles.color = s_HandleColor;

            Vector3 newWorldPos;

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            var startRelativeToThisGameObject = navLink.startRelativeToThisGameObject;
            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 0)
            {
                EditorGUI.BeginChangeCheck();
                if (startRelativeToThisGameObject)
                    Handles.CubeHandleCap(0, worldStartPt, zup, 0.1f * startSize, Event.current.type);
                else
                    Handles.SphereHandleCap(0, worldStartPt, zup, 0.1f * startSize, Event.current.type);

                newWorldPos = Handles.PositionHandle(worldStartPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link start point");
                    if (startRelativeToThisGameObject)
                        navLink.startPoint = mat.inverse.MultiplyPoint(newWorldPos);
                    else
                        navLink.startPoint = newWorldPos - navLink.startTransform.position;
                }
            }
            else
            {
                if (Handles.Button(worldStartPt, zup, 0.1f * startSize, 0.1f * startSize,
                        startRelativeToThisGameObject ? Handles.CubeHandleCap : Handles.SphereHandleCap))
                {
                    s_SelectedPoint = 0;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }
#else
            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 0)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, startPt, zup, 0.1f * startSize, Event.current.type);
                pos = Handles.PositionHandle(startPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.startPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(startPt, zup, 0.1f * startSize, 0.1f * startSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 0;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }
#endif

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            var endRelativeToThisGameObject = navLink.endRelativeToThisGameObject;
            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 1)
            {
                EditorGUI.BeginChangeCheck();
                if (endRelativeToThisGameObject)
                    Handles.CubeHandleCap(0, worldEndPt, zup, 0.1f * endSize, Event.current.type);
                else
                    Handles.SphereHandleCap(0, worldEndPt, zup, 0.1f * endSize, Event.current.type);

                newWorldPos = Handles.PositionHandle(worldEndPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link end point");
                    if (endRelativeToThisGameObject)
                        navLink.endPoint = mat.inverse.MultiplyPoint(newWorldPos);
                    else
                        navLink.endPoint = newWorldPos - navLink.endTransform.position;
                }
            }
            else
            {
                if (Handles.Button(worldEndPt, zup, 0.1f * endSize, 0.1f * endSize,
                        endRelativeToThisGameObject ? Handles.CubeHandleCap : Handles.SphereHandleCap))
                {
                    s_SelectedPoint = 1;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }
#else
            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 1)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, endPt, zup, 0.1f * endSize, Event.current.type);
                pos = Handles.PositionHandle(endPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.endPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(endPt, zup, 0.1f * endSize, 0.1f * endSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 1;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }
#endif
            EditorGUI.BeginChangeCheck();
            newWorldPos = Handles.Slider(worldMidPt + right * navLink.width * 0.5f, right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(right, (newWorldPos - worldMidPt)));
            }

            EditorGUI.BeginChangeCheck();
            newWorldPos = Handles.Slider(worldMidPt - right * navLink.width * 0.5f, -right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(-right, (newWorldPos - worldMidPt)));
            }

            Handles.color = oldColor;
        }

        [MenuItem("GameObject/AI/NavMesh Link", false, 2002)]
        public static void CreateNavMeshLink(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            GameObject go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Link", parent);
            go.AddComponent<NavMeshLink>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}

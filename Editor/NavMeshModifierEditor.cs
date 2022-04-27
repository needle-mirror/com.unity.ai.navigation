using UnityEditor;
using UnityEditor.AI;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifier))]
    class NavMeshModifierEditor : UnityEditor.Editor
    {
        SerializedProperty m_AffectedAgents;
        SerializedProperty m_IgnoreFromBuild;
        SerializedProperty m_OverrideArea;
        SerializedProperty m_Area;
#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
        SerializedProperty m_ApplyToChildren;
        SerializedProperty m_OverrideGenerateLinks;
        SerializedProperty m_GenerateLinks;
#endif

        void OnEnable()
        {
            m_AffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            m_IgnoreFromBuild = serializedObject.FindProperty("m_IgnoreFromBuild");
            m_OverrideArea = serializedObject.FindProperty("m_OverrideArea");
            m_Area = serializedObject.FindProperty("m_Area");
#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
            m_ApplyToChildren = serializedObject.FindProperty("m_ApplyToChildren");
            m_OverrideGenerateLinks = serializedObject.FindProperty("m_OverrideGenerateLinks");
            m_GenerateLinks = serializedObject.FindProperty("m_GenerateLinks");
#endif
#if !ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
            NavMeshVisualizationSettings.showNavigation++;
#endif
        }

#if !ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }
#endif

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int mode = m_IgnoreFromBuild.boolValue ? 1 : 0;
            string[] choices = { "Add or Modify object", "Remove object" };

            mode = EditorGUILayout.Popup("Mode", mode, choices);

            m_IgnoreFromBuild.boolValue = mode == 1;

            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", m_AffectedAgents);

#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
            EditorGUILayout.PropertyField(m_ApplyToChildren);
#endif

            if (!m_IgnoreFromBuild.boolValue)
            {
                EditorGUILayout.PropertyField(m_OverrideArea);
                if (m_OverrideArea.boolValue)
                {
                    EditorGUI.indentLevel++;
                    NavMeshComponentsGUIUtility.AreaPopup("Area Type", m_Area);
                    EditorGUI.indentLevel--;
                }

#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
                EditorGUILayout.PropertyField(m_OverrideGenerateLinks);
                if (m_OverrideGenerateLinks.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_GenerateLinks);
                    EditorGUI.indentLevel--;
                }
#endif
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
using System;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Editor
{
    /// <summary> Class containing a set of utility functions meant for presenting information from the NavMeshComponents into the GUI. </summary>
    public static class NavMeshComponentsGUIUtility
    {
        static readonly GUIContent s_TempContent = new();

        static GUIContent TempContent(string text)
        {
            s_TempContent.image = default;
            s_TempContent.text = text;
            s_TempContent.tooltip = default;
            return s_TempContent;
        }

        internal const string k_PackageEditorResourcesFolder = "Packages/com.unity.ai.navigation/EditorResources/";
        static readonly string k_OpenAreaSettingsText = L10n.Tr("Open Area Settings...");

        /// <summary> Displays a GUI element for selecting the area type used by a <see cref="NavMeshSurface"/>, <see cref="NavMeshLink"/>, <see cref="NavMeshModifier"/> or <see cref="NavMeshModifierVolume"/>.</summary>
        /// <remarks> The dropdown menu lists all of the area types defined in the <a href="../manual/NavigationWindow.html#areas-tab">Areas tab</a> of the Navigation window. </remarks>
        /// <param name="labelName">The label for the field.</param>
        /// <param name="areaProperty">The serialized property that this GUI element displays and modifies. It represents a NavMesh <see cref="NavMeshModifier.area">area type</see> and it needs to store values of type <see cref="SerializedPropertyType.Integer"/>.</param>
        /// <seealso cref="NavMeshSurface.defaultArea">NavMeshSurface.defaultArea</seealso>
        /// <seealso cref="NavMeshBuildSource.area">NavMeshBuildSource.area</seealso>
        public static void AreaPopup(string labelName, SerializedProperty areaProperty) =>
            AreaPopup(TempContent(labelName), areaProperty);

        internal static void AreaPopup(GUIContent label, SerializedProperty areaProperty)
        {
            var areaIndex = -1;
            var areaNames = GetNavMeshAreaNames();
            for (var i = 0; i < areaNames.Length; i++)
            {
                var areaValue = GetNavMeshAreaFromName(areaNames[i]);
                if (areaValue == areaProperty.intValue)
                    areaIndex = i;
            }

            ArrayUtility.Add(ref areaNames, "");
            ArrayUtility.Add(ref areaNames, k_OpenAreaSettingsText);

            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect, GUIContent.none, areaProperty);

            EditorGUI.BeginChangeCheck();
            areaIndex = EditorGUI.Popup(rect, label, areaIndex, areaNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (areaIndex >= 0 && areaIndex < areaNames.Length - 2)
                    areaProperty.intValue = GetNavMeshAreaFromName(areaNames[areaIndex]);
                else if (areaIndex == areaNames.Length - 1)
                    NavMeshEditorHelpers.OpenAreaSettings();
            }

            EditorGUI.EndProperty();
        }

        internal static readonly string k_OpenAgentSettingsText = L10n.Tr("Open Agent Settings...");
        static readonly string k_AgentTypeInvalidText = L10n.Tr("Agent Type invalid.");

        /// <summary> Displays a GUI element for selecting the agent type used by a <see cref="NavMeshSurface"/> or <see cref="NavMeshLink"/>. </summary>
        /// <remarks> The dropdown menu lists all of the agent types defined in the <a href="../manual/NavigationWindow.html#agents-tab">Agents tab</a> of the Navigation window. </remarks>
        /// <param name="labelName">The label for the field.</param>
        /// <param name="agentTypeID">The serialized property that this GUI element displays and modifies. It stores an <see cref="SerializedPropertyType.Integer"/> value that represents a NavMesh <see cref="NavMeshSurface.agentTypeID">agent type ID</see>.<br/>
        /// The selected item is displayed as the <a href="https://docs.unity3d.com/ScriptReference/AI.NavMesh.GetSettingsNameFromID.html">name</a> that corresponds to the stored ID.</param>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-agentTypeID.html">NavMeshAgent.agentTypeID</seealso>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/AI.NavMeshBuildSettings-agentTypeID.html">NavMeshBuildSettings.agentTypeID</seealso>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/AI.NavMesh.GetSettingsNameFromID.html">NavMesh.GetSettingsNameFromID</seealso>
        public static void AgentTypePopup(string labelName, SerializedProperty agentTypeID) =>
            AgentTypePopup(TempContent(labelName), agentTypeID);

        internal static void AgentTypePopup(GUIContent label, SerializedProperty agentTypeID)
        {
            var index = -1;
            var count = NavMesh.GetSettingsCount();
            var agentTypeNames = new string[count + 2];
            for (var i = 0; i < count; i++)
            {
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                var name = NavMesh.GetSettingsNameFromID(id);
                agentTypeNames[i] = name;
                if (id == agentTypeID.intValue)
                    index = i;
            }

            agentTypeNames[count] = "";
            agentTypeNames[count + 1] = k_OpenAgentSettingsText;

            bool validAgentType = index != -1;
            if (!validAgentType)
            {
                EditorGUILayout.HelpBox(k_AgentTypeInvalidText, MessageType.Warning);
            }

            var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(rect, GUIContent.none, agentTypeID);

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(rect, label, index, agentTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0 && index < count)
                {
                    var id = NavMesh.GetSettingsByIndex(index).agentTypeID;
                    agentTypeID.intValue = id;
                }
                else if (index == count + 1)
                {
                    NavMeshEditorHelpers.OpenAgentSettings(-1);
                }
            }

            EditorGUI.EndProperty();
        }

        // Agent mask is a set (internally array/list) of agentTypeIDs.
        // It is used to describe which agents modifiers apply to.
        // There is a special case of "None" which is an empty array.
        // There is a special case of "All" which is an array of length 1, and value of -1.
        /// <summary> Displays a GUI element for selecting multiple agent types for which a <see cref="NavMeshModifier"/> or <see cref="NavMeshModifierVolume"/> can influence the NavMesh.</summary>
        /// <remarks> The dropdown menu lists all of the agent types defined in the <a href="../manual/NavigationWindow.html#agents-tab">Agents tab</a> of the Navigation window. </remarks>
        /// <param name="labelName">The label for the field.</param>
        /// <param name="agentMask">The serialized property that holds the <a href="https://docs.unity3d.com/ScriptReference/SerializedProperty-isArray.html">array</a> of NavMesh <see cref="NavMeshSurface.agentTypeID">agent type</see> values that are selected from the items defined in the <a href="../manual/NavigationWindow.html#agents-tab">Agents tab</a> of the Navigation window. The items are stored as <see cref="SerializedPropertyType.Integer"/> <see cref="NavMeshBuildSettings.agentTypeID">ID values</see> and are displayed as their corresponding <a href="https://docs.unity3d.com/ScriptReference/AI.NavMesh.GetSettingsNameFromID.html">names</a>.</param>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/AI.NavMesh.GetSettingsByIndex.html">NavMesh.GetSettingsByIndex</seealso>
        /// <seealso href="Unity.AI.Navigation.NavMeshModifier.AffectsAgentType.html">NavMeshModifier.AffectsAgentType</seealso>
        /// <seealso href="Unity.AI.Navigation.NavMeshModifierVolume.AffectsAgentType.html">NavMeshModifierVolume.AffectsAgentType</seealso>
        public static void AgentMaskPopup(string labelName, SerializedProperty agentMask) =>
            AgentMaskPopup(TempContent(labelName), agentMask);

        internal static void AgentMaskPopup(GUIContent label, SerializedProperty agentMask)
        {
            // Contents of the dropdown box.
            string popupContent = "";

            if (agentMask.hasMultipleDifferentValues)
                popupContent = "\u2014";
            else
                popupContent = GetAgentMaskLabelName(agentMask);

            var content = TempContent(popupContent);
            var popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);

            EditorGUI.BeginProperty(popupRect, GUIContent.none, agentMask);
            popupRect = EditorGUI.PrefixLabel(popupRect, 0, label);
            bool pressed = GUI.Button(popupRect, content, EditorStyles.popup);

            if (pressed)
            {
                var show = !agentMask.hasMultipleDifferentValues;
                var showNone = show && agentMask.arraySize == 0;
                var showAll = show && IsAll(agentMask);

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), showNone, SetAgentMaskNone, agentMask);
                menu.AddItem(new GUIContent("All"), showAll, SetAgentMaskAll, agentMask);
                menu.AddSeparator("");

                var count = NavMesh.GetSettingsCount();
                for (var i = 0; i < count; i++)
                {
                    var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                    var sname = NavMesh.GetSettingsNameFromID(id);

                    var showSelected = show && AgentMaskHasSelectedAgentTypeID(agentMask, id);
                    var userData = new object[] { agentMask, id, !showSelected };
                    menu.AddItem(new GUIContent(sname), showSelected, ToggleAgentMaskItem, userData);
                }

                menu.DropDown(popupRect);
            }

            EditorGUI.EndProperty();
        }

        /// <summary> Creates and selects a new GameObject as a child of another GameObject. </summary>
        /// <param name="suggestedName">The name given to the created child GameObject. If necessary, this method <a href="https://docs.unity3d.com/ScriptReference/GameObjectUtility.GetUniqueNameForSibling.html">modifies</a> the name in order to distinguish it from the other children of the same parent object.</param>
        /// <param name="parent">The GameObject to which the created GameObject is attached as a child object.</param>
        /// <returns>A new GameObject that is a child of the specified parent GameObject.</returns>
        public static GameObject CreateAndSelectGameObject(string suggestedName, GameObject parent)
        {
            return CreateAndSelectGameObject(suggestedName, parent, Array.Empty<Type>());
        }

        internal static GameObject CreateAndSelectGameObject(string suggestedName, GameObject parent, params Type[] components)
        {
            var child = ObjectFactory.CreateGameObject(suggestedName, components);
            GOCreationCommands.Place(child, parent);
            return child;
        }

        /// <summary> Checks whether a serialized property has all the bits set when interpreted as a bitmask. </summary>
        /// <param name="agentMask"></param>
        /// <returns></returns>
        static bool IsAll(SerializedProperty agentMask)
        {
            return agentMask.arraySize == 1 && agentMask.GetArrayElementAtIndex(0).intValue == -1;
        }

        /// <summary> Marks one agent type as being selected or not. </summary>
        /// <param name="userData"></param>
        static void ToggleAgentMaskItem(object userData)
        {
            var args = (object[])userData;
            var agentMask = (SerializedProperty)args[0];
            var agentTypeID = (int)args[1];
            var value = (bool)args[2];

            ToggleAgentMaskItem(agentMask, agentTypeID, value);
        }

        /// <summary> Marks one agent type as being selected or not. </summary>
        /// <param name="agentMask"></param>
        /// <param name="agentTypeID"></param>
        /// <param name="value"></param>
        static void ToggleAgentMaskItem(SerializedProperty agentMask, int agentTypeID, bool value)
        {
            if (agentMask.hasMultipleDifferentValues)
            {
                agentMask.ClearArray();
                agentMask.serializedObject.ApplyModifiedProperties();
            }

            // Find which index this agent type is in the agentMask array.
            int idx = -1;
            for (var j = 0; j < agentMask.arraySize; j++)
            {
                var elem = agentMask.GetArrayElementAtIndex(j);
                if (elem.intValue == agentTypeID)
                    idx = j;
            }

            // Handle "All" special case.
            if (IsAll(agentMask))
            {
                agentMask.DeleteArrayElementAtIndex(0);
            }

            // Toggle value.
            if (value)
            {
                if (idx == -1)
                {
                    agentMask.InsertArrayElementAtIndex(agentMask.arraySize);
                    agentMask.GetArrayElementAtIndex(agentMask.arraySize - 1).intValue = agentTypeID;
                }
            }
            else
            {
                if (idx != -1)
                {
                    agentMask.DeleteArrayElementAtIndex(idx);
                }
            }

            agentMask.serializedObject.ApplyModifiedProperties();
        }

        /// <summary> Marks all agent types as not being selected. </summary>
        /// <param name="data"></param>
        static void SetAgentMaskNone(object data)
        {
            var agentMask = (SerializedProperty)data;
            agentMask.ClearArray();
            agentMask.serializedObject.ApplyModifiedProperties();
        }

        /// <summary> Marks all agent types as being selected. </summary>
        /// <param name="data"></param>
        static void SetAgentMaskAll(object data)
        {
            var agentMask = (SerializedProperty)data;
            agentMask.ClearArray();
            agentMask.InsertArrayElementAtIndex(0);
            agentMask.GetArrayElementAtIndex(0).intValue = -1;
            agentMask.serializedObject.ApplyModifiedProperties();
        }

        static readonly string k_AgentMaskNoneText = L10n.Tr("None");
        static readonly string k_AgentTypeAllText = L10n.Tr("All");
        static readonly string k_AgentTypeMixedText = L10n.Tr("Mixed...");

        /// <summary> Obtains one string that represents the current selection of agent types. </summary>
        /// <param name="agentMask"></param>
        /// <returns> One string that represents the current selection of agent types.</returns>
        static string GetAgentMaskLabelName(SerializedProperty agentMask)
        {
            if (agentMask.arraySize == 0)
                return k_AgentMaskNoneText;

            if (IsAll(agentMask))
                return k_AgentTypeAllText;

            if (agentMask.arraySize <= 3)
            {
                var labelName = "";
                for (var j = 0; j < agentMask.arraySize; j++)
                {
                    var elem = agentMask.GetArrayElementAtIndex(j);
                    var settingsName = NavMesh.GetSettingsNameFromID(elem.intValue);
                    if (string.IsNullOrEmpty(settingsName))
                        continue;

                    if (labelName.Length > 0)
                        labelName += ", ";
                    labelName += settingsName;
                }

                return labelName;
            }

            return k_AgentTypeMixedText;
        }

        /// <summary> Checks whether a certain agent type is selected. </summary>
        /// <param name="agentMask"></param>
        /// <param name="agentTypeID"></param>
        /// <returns></returns>
        static bool AgentMaskHasSelectedAgentTypeID(SerializedProperty agentMask, int agentTypeID)
        {
            for (var j = 0; j < agentMask.arraySize; j++)
            {
                var elem = agentMask.GetArrayElementAtIndex(j);
                if (elem.intValue == agentTypeID)
                    return true;
            }

            return false;
        }

        static string[] GetNavMeshAreaNames()
        {
#if EDITOR_ONLY_NAVMESH_BUILDER_DEPRECATED
            return NavMesh.GetAreaNames();
#else
            return GameObjectUtility.GetNavMeshAreaNames();
#endif
        }

        static int GetNavMeshAreaFromName(string name)
        {
#if EDITOR_ONLY_NAVMESH_BUILDER_DEPRECATED
            return NavMesh.GetAreaFromName(name);
#else
            return GameObjectUtility.GetNavMeshAreaFromName(name);
#endif
        }
    }
}

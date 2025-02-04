using Unity.AI.Navigation.Editor.Converter;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Updater
{
    internal static class NavigationUpdaterEditor
    {
        [MenuItem("Window/AI/Navigation Updater", false, 50)]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow<SystemConvertersEditor>();
            wnd.titleContent = new GUIContent("Navigation Updater");
            wnd.DontSaveToLayout(wnd);
            wnd.maxSize = new Vector2(650f, 4000f);
            wnd.minSize = new Vector2(650f, 400f);
            wnd.Show();
        }
    }
}

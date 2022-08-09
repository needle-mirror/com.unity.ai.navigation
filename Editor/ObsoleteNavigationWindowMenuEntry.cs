#if ENABLE_NAVIGATION_UI_REQUIRES_PACKAGE

using UnityEditor.AI;
using UnityEditor;

namespace Unity.AI.Navigation.Editor
{
    internal static class ObsoleteNavigationWindowMenuEntry
    {
        [MenuItem("Window/AI/Navigation (Obsolete)", false, 1)]
        static void SetupWindow()
        {
            NavMeshEditorHelpers.SetupLegacyNavigationWindow();
        }
    }
}

#endif

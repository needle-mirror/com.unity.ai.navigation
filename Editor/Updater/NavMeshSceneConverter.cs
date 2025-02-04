using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation.Editor.Converter;
using UnityEditor;

namespace Unity.AI.Navigation.Updater
{
    internal class NavMeshSceneConverter : SystemConverter
    {
        public override string name => "NavMesh Scene Converter";
        public override string info => "Reassigns the legacy baked NavMesh to a NavMeshSurface on a game object named 'Navigation'.\nAdds a NavMeshModifier component to each game object marked as Navigation Static.";
        public override Type container => typeof(NavigationConverterContainer);

        List<string> m_AssetsToConvert = new List<string>();

        public override void OnInitialize(InitializeConverterContext context, Action callback)
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string path in allAssetPaths)
            {
                if (NavMeshUpdaterUtility.IsSceneReferencingLegacyNavMesh(path))
                {
                    ConverterItemDescriptor desc = new ConverterItemDescriptor()
                    {
                        name = Path.GetFileNameWithoutExtension(path),
                        info = path,
                        warningMessage = String.Empty,
                    };

                    m_AssetsToConvert.Add(path);
                    context.AddAssetToConvert(desc);
                }
            }

            callback.Invoke();
        }

        public override void OnRun(ref RunItemContext context)
        {
            var items = context.items;
            for (var i = 0; i < items.Length; ++i)
            {
                var success = NavMeshUpdaterUtility.ConvertScene(items[i].descriptor.info);
                if (!success)
                {
                    var index = items[i].index;
                    context.didFail[index] = true;
                    context.info[index] = "Failed to convert scene.";
                }
            }
        }
    }
}

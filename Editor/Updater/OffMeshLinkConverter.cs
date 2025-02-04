using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation.Editor.Converter;
using UnityEditor;

namespace Unity.AI.Navigation.Updater
{
    internal sealed class OffMeshLinkConverter : SystemConverter
    {
        public override string name => "OffMesh Link Converter";
        public override string info => "Creates NavMesh Link components that match and replace existing OffMesh Link components. \n" +
            "Ensure the selected scene or prefab files are writable prior to running the Converter.";
        public override Type container => typeof(NavigationConverterContainer);

        public override void OnInitialize(InitializeConverterContext context, Action callback)
        {
            var objectsToConvert = OffMeshLinkUpdaterUtility.FindObjectsToConvert();
            foreach (var guid in objectsToConvert)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var desc = new ConverterItemDescriptor()
                {
                    name = Path.GetFileNameWithoutExtension(path),
                    info = path,
                    additionalData = guid
                };
                context.AddAssetToConvert(desc);
            }

            callback.Invoke();
        }

        public override void OnRun(ref RunItemContext context)
        {
            var convertList = new List<string>(context.items.Length);
            for (var i = 0; i < context.items.Length; ++i)
            {
                var guid = context.items[i].descriptor.additionalData;
                convertList.Add(guid);
            }
            OffMeshLinkUpdaterUtility.Convert(convertList, out var failedConversions);

            foreach (var conversionItem in failedConversions)
            {
                var index = conversionItem.itemIndex;
                context.didFail[index] = true;
                context.info[index] = conversionItem.failureMessage;
            }
        }
    }
}

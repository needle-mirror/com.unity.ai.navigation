#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
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
		public override string info => "Converts the legacy baked NavMesh with a NavMeshSurface on a GameObject named 'Navigation'.\nReplaces Navigation static values with NavMeshModifier components.";
		public override Type container => typeof(BuiltInToNavMeshSurfaceConverterContainer);
		
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
						helpLink = String.Empty
					};

					m_AssetsToConvert.Add(path);
					context.AddAssetToConvert(desc);
				}
			}

			callback.Invoke();
		}


		public override void OnRun(ref RunItemContext context)
		{
			bool success = NavMeshUpdaterUtility.ConvertScene(context.item.descriptor.info);

			context.hasConverted = success;
		}
	}
}
#endif

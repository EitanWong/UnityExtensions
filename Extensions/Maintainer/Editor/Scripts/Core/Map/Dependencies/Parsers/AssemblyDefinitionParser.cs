#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using Tools;
	using UnityEditor;
	using UnityEditor.Compilation;
	using UnityEngine;
	
	//TODO: check if bug 1020737 is fixed and this can be removed
	public class AssemblyDefinitionParser : IDependenciesParser
	{
		public Type Type
		{
			get
			{
				return CSReflectionTools.assemblyDefinitionAssetType;
			}
		}

		public List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path)
		{
			if (kind != AssetKind.Regular)
			{
				return null;
			}
			
			return GetAssetsReferencedFromAssemblyDefinition(path);
		}
		
		private static List<string> GetAssetsReferencedFromAssemblyDefinition(string assetPath)
		{
			var result = new List<string>();

			var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(assetPath);
			var data = JsonUtility.FromJson<AssemblyDefinitionData>(asset.text);

			if (data.references != null && data.references.Length > 0)
			{
				foreach (var reference in data.references)
				{
#if !UNITY_2019_1_OR_NEWER
					var assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(reference);
#else
					var assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyReference(reference);
#endif
					if (!string.IsNullOrEmpty(assemblyDefinitionFilePathFromAssemblyName))
					{
						assemblyDefinitionFilePathFromAssemblyName = CSPathTools.EnforceSlashes(assemblyDefinitionFilePathFromAssemblyName);
						var guid = AssetDatabase.AssetPathToGUID(assemblyDefinitionFilePathFromAssemblyName);
						if (!string.IsNullOrEmpty(guid))
						{
							result.Add(guid);
						}
					}
				}
			}

			data.references = null;

			return result;
		}
		
		private class AssemblyDefinitionData
		{
			public string[] references;
		}
	}
}
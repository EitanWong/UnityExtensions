#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Tools;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Does looks for assets dependencies used at the Maintainer's Assets Map.
	/// </summary>
	public static class AssetDependenciesSearcher
	{
		private static readonly List<IDependenciesParser> InternalDependenciesParsers = new List<IDependenciesParser>
		{
			new AssemblyDefinitionParser(),
#if UNITY_2019_2_OR_NEWER
			new AssemblyDefinitionReferenceParser(),
#endif
			new SpriteAtlasParser(),
			new ShaderParser(),
			new TextAssetParser(),
			new AddressableAssetGroupParser(),
		};
		
		private static List<IDependenciesParser> externalDependenciesParsers;

		/// <summary>
		/// Register any custom dependencies parsers here. Allows parsing unknown types dependencies.
		/// </summary>
		/// Passed parsers will be used in the Assets Dependencies Map and will affect all Maintainer references-related functionality.<br/>
		/// Call this before running Maintainer first time (before Assets Map is created).<br/>
		/// For example, call it from the [InitializeOnLoad] class' static constructor.
		/// <param name="parsers">List of new parsers.</param>
		public static void AddExternalDependenciesParsers(List<IDependenciesParser> parsers)
		{
			if (parsers == null || parsers.Count == 0)
			{
				Debug.LogError(Maintainer.ConstructWarning("Empty list passed to AddExternalDependenciesParsers!"));
				return;
			}

			if (externalDependenciesParsers == null || externalDependenciesParsers.Count == 0)
			{
				externalDependenciesParsers = parsers;
			}
			else
			{
				foreach (var newParser in parsers)
				{
					if (!externalDependenciesParsers.Contains(newParser))
					{
						externalDependenciesParsers.Add(newParser);
					}
				}
			}
		}
		
		internal static string[] FindDependencies(AssetSettingsKind settingsKind, Type type, AssetKind kind, string path)
		{
			var dependenciesGUIDs = new HashSet<string>();
			
			if (settingsKind == AssetSettingsKind.NotSettings)
			{
				/* pre-regular dependenciesGUIDs additions */
				FillDependencies(InternalDependenciesParsers, ref dependenciesGUIDs, kind, type, path);

				/* regular dependenciesGUIDs additions */
				var dependencies = AssetDatabase.GetDependencies(path, false);
				var guids = CSAssetTools.GetAssetsGUIDs(dependencies);
				if (guids != null && guids.Length > 0)
				{
					dependenciesGUIDs.UnionWith(guids);
				}
			}
			else
			{
				FillSettingsAssetDependencies(ref dependenciesGUIDs, path, settingsKind);
			}
			
			if (externalDependenciesParsers != null && externalDependenciesParsers.Count > 0)
			{
				FillDependencies(externalDependenciesParsers, ref dependenciesGUIDs, kind, type, path);
			}

			// kept for debugging purposes
			/*if (Path.Contains("1.unity"))
			{
				Debug.Log("1.unity non-recursive dependenciesGUIDs:");
				foreach (var reference in references)
				{
					Debug.Log(reference);
				}
			}*/

			return dependenciesGUIDs.ToArray();
		}

		private static void FillDependencies(List<IDependenciesParser> dependenciesParsers, ref HashSet<string> dependenciesGUIDs, AssetKind kind, Type type, string path)
		{
			foreach (var parser in dependenciesParsers)
			{
				if (parser.Type != null && parser.Type != type)
				{
					continue;
				}
				
				var foundDependencies = parser.GetDependenciesGUIDs(kind, type, path);
				if (foundDependencies == null || foundDependencies.Count == 0)
				{
					continue;
				}
				
				dependenciesGUIDs.UnionWith(foundDependencies);
			}
		}

		private static void FillSettingsAssetDependencies(ref HashSet<string> dependenciesGUIDs, string assetPath, AssetSettingsKind settingsKind)
		{
			if (settingsKind == AssetSettingsKind.EditorBuildSettings)
			{
				var scenesInBuildGUIDs = CSSceneTools.GetScenesInBuildGUIDs(true);
				if (scenesInBuildGUIDs != null)
				{
					dependenciesGUIDs.UnionWith(scenesInBuildGUIDs);
				}
			}
			else
			{
				var settingsAsset = AssetDatabase.LoadAllAssetsAtPath(assetPath);
				if (settingsAsset != null && settingsAsset.Length > 0)
				{
					var settingsAssetSerialized = new SerializedObject(settingsAsset[0]);

					var sp = settingsAssetSerialized.GetIterator();
					while (sp.Next(true))
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							var instanceId = sp.objectReferenceInstanceIDValue;
							if (instanceId != 0)
							{
								var path = CSPathTools.EnforceSlashes(AssetDatabase.GetAssetPath(instanceId));
								if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
								{
									var guid = AssetDatabase.AssetPathToGUID(path);
									if (!string.IsNullOrEmpty(guid))
									{
										dependenciesGUIDs.Add(guid);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
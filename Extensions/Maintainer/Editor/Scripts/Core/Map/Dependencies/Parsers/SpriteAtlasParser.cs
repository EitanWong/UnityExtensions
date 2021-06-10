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
	using UnityEngine;

	public class SpriteAtlasParser : IDependenciesParser
	{
		public Type Type
		{
			get
			{
				return CSReflectionTools.spriteAtlasType; 
			}
		} 
		public List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path)
		{
			return GetAssetsGUIDsInFoldersReferencedFromSpriteAtlas(path);
		}
		
		private static List<string> GetAssetsGUIDsInFoldersReferencedFromSpriteAtlas(string assetPath)
		{
			var result = new List<string>();

			var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(assetPath);
			var so = new SerializedObject(asset);

			// source: SpriteAtlasInspector
			var packablesProperty = so.FindProperty("m_EditorData.packables");
			if (packablesProperty == null || !packablesProperty.isArray)
			{
				Debug.LogError(Maintainer.LogPrefix + "Can't parse UnityEngine.U2D.SpriteAtlas, please report to " + Maintainer.SupportEmail);
			}
			else
			{
				var count = packablesProperty.arraySize;
				for (var i = 0; i < count; i++)
				{
					var packable = packablesProperty.GetArrayElementAtIndex(i);
					var objectReferenceValue = packable.objectReferenceValue;
					if (objectReferenceValue != null)
					{
						var path = AssetDatabase.GetAssetOrScenePath(objectReferenceValue);
						if (AssetDatabase.IsValidFolder(path))
						{
							var packableGUIDs = CSPathTools.GetAllPackableAssetsGUIDsRecursive(path);
							if (packableGUIDs != null && packableGUIDs.Length > 0)
							{
								result.AddRange(packableGUIDs);
							}
						}
					}
				}
			}

			return result;
		}
	}
}
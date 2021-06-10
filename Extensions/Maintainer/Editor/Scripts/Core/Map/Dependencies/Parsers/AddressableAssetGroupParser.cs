#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public class AddressableAssetGroupParser : IDependenciesParser
	{
		public Type Type
		{
			get
			{
				return null;
			}
		}
		
		public List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path)
		{
			// checking by name since addressables are in optional external package
			if (type == null || type.Name != "AddressableAssetGroup")
				return null;
			
			var assetGroup = AssetDatabase.LoadMainAssetAtPath(path);
			return assetGroup == null ? null : ExtractReferencedAssets(assetGroup);
		}
		
		private static List<string> ExtractReferencedAssets(Object assetGroup)
		{
			var so = new SerializedObject(assetGroup);

			var serializedEntries = so.FindProperty("m_SerializeEntries");
			if (serializedEntries == null)
			{
				// legacy package version used this name
				serializedEntries = so.FindProperty("m_serializeEntries");

				if (serializedEntries == null)
				{
					Debug.LogError(Maintainer.ConstructError("Can't reach serialize entries in AddressableAssetGroup!"));
					return null;
				}
			}

			if (!serializedEntries.isArray)
			{
				Debug.LogError(Maintainer.ConstructError("Can't find serialize entries array in AddressableAssetGroup!"));
				return null;
			}

			var result = new List<string>();

			var count = serializedEntries.arraySize;
			for (var i = 0; i < count; i++)
			{
				var item = serializedEntries.GetArrayElementAtIndex(i);
				if (item == null)
				{
					Debug.LogWarning(Maintainer.ConstructWarning("Serialize entry from AddressableAssetGroup is null!"));
					continue;
				}

				var referencedGUID = item.FindPropertyRelative("m_GUID");
				if (referencedGUID == null || referencedGUID.propertyType != SerializedPropertyType.String)
				{
					Debug.LogError(Maintainer.ConstructError("Can't reach Serialize entry GUID of AddressableAssetGroup!"));
					return null;
				}

				var path = AssetDatabase.GUIDToAssetPath(referencedGUID.stringValue);
				if (!path.StartsWith("Assets"))
				{
					continue;
				}

				var guid = AssetDatabase.AssetPathToGUID(path);
				if (!string.IsNullOrEmpty(guid))
				{
					result.Add(guid);
				}
			}

			return result;
		}
	}
}
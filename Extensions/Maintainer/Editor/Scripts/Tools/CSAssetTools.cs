#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	internal static class CSAssetTools
	{
		public static bool IsAssetScriptableObjectWithMissingScript(string path)
		{
			var extension = Path.GetExtension(path);
			return string.Equals(extension, ".asset", StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(extension, ".playable", StringComparison.OrdinalIgnoreCase);
		}

		public static int GetMainAssetInstanceID(string path)
		{
			var mi = CSReflectionTools.GetGetMainAssetInstanceIDMethodInfo();
			if (mi != null)
			{
				return (int)mi.Invoke(null, new object[] { path });
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve InstanceID From path via reflection!"));
			return -1;
		}

		public static string[] GetAssetsGUIDs(string[] paths)
		{
			if (paths == null || paths.Length == 0)
			{
				return null;
			}
			
			var guids = new List<string>(paths.Length);
			foreach (var path in paths)
			{
				var guid = AssetDatabase.AssetPathToGUID(path);
				if (!string.IsNullOrEmpty(guid))
				{
					guids.Add(guid);
				}
			}

			return guids.ToArray();
		}
		
		public static string[] GetAssetImporterDependencies(string path)
		{
			var importer = AssetImporter.GetAtPath(path);
			if (importer == null)
			{
				Debug.LogWarning(Maintainer.ConstructWarning("Couldn't find AssetImporter for " + path));
				return null;
			}
			
			var method = importer.GetType().GetMethod("GatherDependenciesFromSourceFile", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				// this importer does not implement optional GatherDependenciesFromSourceFile message (starting from Unity 2020.1)
				return null;
			}
			
			var items = (string[])method.Invoke(null, new []{path});
			if (items != null && items.Length > 0)
				return items;
			
			return null;
		}
	}
}
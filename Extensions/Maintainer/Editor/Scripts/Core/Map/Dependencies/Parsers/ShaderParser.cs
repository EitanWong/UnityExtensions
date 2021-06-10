#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Tools;
	using UnityEditor;

	public class ShaderParser : IDependenciesParser
	{
		public Type Type
		{
			get
			{
				return CSReflectionTools.shaderType;
			}
		}

		public List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path)
		{
			var referencesGUIDs = new List<string>();
			var extension = Path.GetExtension(path);

			// processing Shader Graph dependencies
			if (extension.Equals(".shadergraph", StringComparison.InvariantCultureIgnoreCase))
			{
				var dependencies = CSAssetTools.GetAssetImporterDependencies(path);
				if (dependencies != null)
				{
					referencesGUIDs.AddRange(CSAssetTools.GetAssetsGUIDs(dependencies));
				}
			}
			else if (extension.Equals(".shadersubgraph", StringComparison.InvariantCultureIgnoreCase))
			{
				var dependencies = CSAssetTools.GetAssetImporterDependencies(path);
				if (dependencies != null)
					referencesGUIDs.AddRange(CSAssetTools.GetAssetsGUIDs(dependencies));
			}
			// processing usual Shader dependencies
			else
			{
				// below is an another workaround for dependenciesGUIDs not include #include-ed files, like *.cginc
				referencesGUIDs.AddRange(ScanFileForIncludes(path));
			}
			
			return referencesGUIDs;
		}

		internal static List<string> ScanFileForIncludes(string filePath)
		{
			var references = new List<string>();
			var fileLines = File.ReadAllLines(filePath);
			
			foreach (var line in fileLines)
			{
				var includeIndex = line.IndexOf("include", StringComparison.Ordinal);
				if (includeIndex == -1) continue;

				var noSharp = line.IndexOf('#', 0, includeIndex) == -1;
				if (noSharp) continue;

				var indexOfFirstQuote = line.IndexOf('"', includeIndex);
				if (indexOfFirstQuote == -1) continue;

				var indexOfLastQuote = line.IndexOf('"', indexOfFirstQuote + 1);
				if (indexOfLastQuote == -1) continue;

				var path = line.Substring(indexOfFirstQuote + 1, indexOfLastQuote - indexOfFirstQuote - 1);
				path = CSPathTools.EnforceSlashes(path);

				string assetPath;

				if (path.StartsWith("Assets/"))
				{
					assetPath = path;
				}
				else if (path.IndexOf('/') != -1)
				{
					var folder = System.IO.Path.GetDirectoryName(filePath);
					if (folder == null) continue;

					var combinedPath = System.IO.Path.Combine(folder, path);
					var fullPath = CSPathTools.EnforceSlashes(System.IO.Path.GetFullPath(combinedPath));
					var assetsIndex = fullPath.IndexOf("Assets/", StringComparison.Ordinal);
					if (assetsIndex == -1) continue;

					assetPath = fullPath.Substring(assetsIndex, fullPath.Length - assetsIndex);
				}
				else
				{
					var folder = System.IO.Path.GetDirectoryName(filePath);
					if (folder == null) continue;

					assetPath = CSPathTools.EnforceSlashes(System.IO.Path.Combine(folder, path));
				}

				if (!File.Exists(assetPath)) continue;

				var guid = AssetDatabase.AssetPathToGUID(assetPath);
				
				if (references.IndexOf(guid) != -1) continue;
				{
					references.Add(guid);
				}
			}

			return references;
		}
	}
}
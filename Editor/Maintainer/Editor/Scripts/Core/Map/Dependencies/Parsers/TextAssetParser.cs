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

	public class TextAssetParser : IDependenciesParser
	{
		public Type Type
		{
			get
			{
				return CSReflectionTools.textAssetType;
			}
		}

		public List<string> GetDependenciesGUIDs(AssetKind kind, Type type, string path)
		{
			if (path.EndsWith(".cginc"))
			{
				// below is an another workaround for dependenciesGUIDs not include #include-ed files, like *.cginc
				return ShaderParser.ScanFileForIncludes(path);
			}

			return null;
		}
	}
}
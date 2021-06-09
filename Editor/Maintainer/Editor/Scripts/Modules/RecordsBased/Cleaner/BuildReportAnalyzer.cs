#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

#if UNITY_2019_3_OR_NEWER
namespace CodeStage.Maintainer.Cleaner
{
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor.Build.Reporting;

	public static class BuildReportAnalyzer
	{
		private static bool inited;
		private static BuildReport lastBuildReport;
		private static List<string> builtAssets = new List<string>();

		public static void Init()
		{
			if (IsReportExists())
			{
				var loaded = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget("Library/LastBuild.buildreport");
				if (loaded != null && loaded.Length > 0)
				{
					foreach (var loadedObject in loaded)
					{
						if (loadedObject is BuildReport)
						{
							lastBuildReport = loadedObject as BuildReport;
						}
					}
					
					if (lastBuildReport != null)
					{
						foreach (var file in lastBuildReport.packedAssets)
						{
							foreach (var packedAssetInfo in file.contents)
							{
								if (packedAssetInfo.sourceAssetPath.StartsWith("Assets/"))
								{
									builtAssets.Add(packedAssetInfo.sourceAssetGUID.ToString());
								}
							}
						}
						
						inited = true;
					}
				}
			}
		}

		public static bool IsReportExists()
		{
			return File.Exists("Library/LastBuild.buildreport");
		}

		public static bool IsFileInBuildReport(string assetGUID)
		{
			if (!inited)
				return false;

			foreach (var asset in builtAssets)
			{
				if (asset == assetGUID)
				{
					return true;
				}
			}

			return false;
		}
	}
}
#endif
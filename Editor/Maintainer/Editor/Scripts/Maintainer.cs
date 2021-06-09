#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("CodeStage.Maintainer.Debug")]

namespace CodeStage.Maintainer
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Entry point class with few common APIs.
	/// </summary>
	public static class Maintainer
	{
		/// <summary>
		/// Current Maintainer version.
		/// </summary>
		public const string Version = "1.7.4";

		internal const string LogPrefix = "<b>[Maintainer]</b> ";
		internal const string SupportEmail = "support@codestage.net";

		internal const string DataLossWarning = "Make sure you've made a backup of your project before proceeding.\nAuthor is not responsible for any data loss due to use of the Maintainer!";

		private static string directory;

		/// <summary>
		/// Path to the Maintainer Directory in your project or packages.
		/// </summary>
		public static string Directory
		{
			get
			{
				if (!string.IsNullOrEmpty(directory)) return directory;

				directory = MaintainerMarker.GetAssetPath();

				if (!string.IsNullOrEmpty(directory))
				{
					if (directory.IndexOf("Scripts/MaintainerMarker.cs", StringComparison.Ordinal) >= 0)
					{
						directory = directory.Replace("Scripts/MaintainerMarker.cs", "");
					}
					else
					{
						directory = null;
						Debug.LogError(ConstructError("Looks like Maintainer is placed in project incorrectly!"));
					}
				}
				else
				{
					directory = null;
					Debug.LogError(ConstructError("Can't locate the Maintainer directory!"));
				}
				return directory;
			}
		}

		internal static string ConstructError(string errorText, string moduleName = null)
		{
			return LogPrefix + (string.IsNullOrEmpty(moduleName) ? "" : moduleName + ": ") + errorText + "\nPlease report to " + SupportEmail;
		}

		internal static string ConstructWarning(string warningText, string moduleName = null)
		{
			return LogPrefix + (string.IsNullOrEmpty(moduleName) ? "" : moduleName + ": ") + warningText;
		}

		internal static string ConstructReportWarning(string warningText, string moduleName = null)
		{
			return LogPrefix + (string.IsNullOrEmpty(moduleName) ? "" : moduleName + ": ") + warningText + "\nPlease report to " + SupportEmail;
		}
	}
}
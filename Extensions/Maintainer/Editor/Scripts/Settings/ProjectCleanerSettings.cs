#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;

	using UnityEditor;
	using UnityEngine;

	using Core;

	/// <summary>
	/// Project Cleaner module settings saved in ProjectSettings folder.
	/// </summary>
	/// Contains various settings for this module, see IDE hints for all list.
	[Serializable]
	public class ProjectCleanerSettings
	{
		/* filtering */

		// for backward compatibility
		public string[] pathIgnores = new string[0];

		// for backward compatibility
		public string[] sceneIgnores = new string[0];

		public FilterItem[] pathIgnoresFilters = new FilterItem[0];
		public FilterItem[] sceneIgnoresFilters = new FilterItem[0];

		public bool ignoreScenesInBuild = true;
		public bool ignoreOnlyEnabledScenesInBuild = true;

		/* what to find */

		public bool findUnreferencedAssets;
		public bool findUnreferencedScripts;
		public bool findEmptyFolders;
		public bool findEmptyFoldersAutomatically;

		/* misc */

		public bool useTrashBin = true;
		public bool rescanAfterContextIgnore = true;

		[NonSerialized]
		private FilterItem[] mandatoryFilters;

		public FilterItem[] MandatoryFilters
		{
			get
			{
				if (mandatoryFilters == null)
				{
					mandatoryFilters = GetMandatoryFilters();
				}

				return mandatoryFilters;
			}
		}

		public ProjectCleanerSettings()
		{
			Reset();
		}

		public int GetFiltersCount()
		{
			return sceneIgnoresFilters.Length + pathIgnoresFilters.Length;
		}

		internal void Reset()
		{
			useTrashBin = true;
			rescanAfterContextIgnore = true;

			findUnreferencedAssets = true;
			findUnreferencedScripts = false;
			findEmptyFolders = true;
			findEmptyFoldersAutomatically = false;
		}

		public void AddDefaultFilters()
		{
			Debug.Log(Maintainer.LogPrefix + "Please check your Project Cleaner Path Ignores, new default filters were added.");
			ArrayUtility.AddRange(ref pathIgnoresFilters, GetDefaultFilters());
		}

		public void SetDefaultFilters()
		{
			pathIgnoresFilters = GetDefaultFilters();
		}

		public static FilterItem[] GetDefaultFilters()
		{
			return new[]
			{
				FilterItem.Create("/Plugins/", FilterKind.Path, true),
				FilterItem.Create(".preset", FilterKind.Extension, true),
			};
		}

		private static FilterItem[] GetMandatoryFilters()
		{
			return new[]
			{
				FilterItem.Create("/Gizmos/", FilterKind.Path, true),
				FilterItem.Create("/Editor/", FilterKind.Path, true),
				FilterItem.Create("/Resources/", FilterKind.Path, true),
				FilterItem.Create("/Editor Resources/", FilterKind.Path, true), // example: Amplify
				FilterItem.Create("/EditorResources/", FilterKind.Path, true), // example: TextMeshPro
				FilterItem.Create("/StreamingAssets/", FilterKind.Path, true),
				FilterItem.Create("/Editor Default Resources/", FilterKind.Path, true),
			};
		}
	}
}
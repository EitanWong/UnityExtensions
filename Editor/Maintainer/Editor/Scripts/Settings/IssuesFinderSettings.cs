#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;

	using Core;

	/// <summary>
	/// Issues Finder module settings saved in ProjectSettings folder.
	/// </summary>
	/// Contains various settings for this module, availability of some settings depends on Unity version.
	/// See IDE hints for all list.
	[Serializable]
	public class IssuesFinderSettings
	{
		[Serializable]
		public enum ScenesSelection
		{
			AllScenes,
			IncludedScenes,
			OpenedScenesOnly
		}

		// -----------------------------------------------------------------------------
		// filtering
		// -----------------------------------------------------------------------------

		public bool includeScenesInBuild = true;
		public bool includeOnlyEnabledScenesInBuild = true;

		public string[] sceneIncludes = new string[0];
		public string[] pathIgnores = new string[0];
		public string[] pathIncludes = new string[0];
		public string[] componentIgnores = new string[0];

		public FilterItem[] sceneIncludesFilters = new FilterItem[0];
		public FilterItem[] pathIgnoresFilters = new FilterItem[0];
		public FilterItem[] pathIncludesFilters = new FilterItem[0];
		public FilterItem[] componentIgnoresFilters = new FilterItem[0];

		// -----------------------------------------------------------------------------
		// where to look
		// -----------------------------------------------------------------------------

		public bool lookInScenes;
		public bool lookInAssets;
		public bool lookInProjectSettings;

		public ScenesSelection scenesSelection;

		public bool scanGameObjects;
		public bool touchInactiveGameObjects;
		public bool touchDisabledComponents;
		
		// -----------------------------------------------------------------------------
		// what to look for
		// -----------------------------------------------------------------------------

		/* project-wide  */

		public bool missingReferences;
#if UNITY_2019_1_OR_NEWER
		public bool shadersWithErrors;
#endif

		/* game objects common  */

		public bool missingComponents;
		public bool missingPrefabs;

		public bool duplicateComponents;
		public bool inconsistentTerrainData;

		/* game objects neatness */

		public bool unnamedLayers;
		public bool hugePositions;

		/* project settings */

		public bool duplicateLayers;

		public IssuesFinderSettings()
		{
            Reset();
		}

		public int GetFiltersCount()
		{
			return sceneIncludesFilters.Length + pathIgnoresFilters.Length + pathIncludesFilters.Length +
			       componentIgnoresFilters.Length;
		}

		internal void SwitchAll(bool enable)
		{
			missingReferences = enable;

#if UNITY_2019_1_OR_NEWER
			shadersWithErrors = enable;
#endif

			SwitchCommon(enable);
			SwitchNeatness(enable);
			SwitchProjectSettings(enable);
		}

		internal void SwitchCommon(bool enable)
		{
			missingComponents = enable;
			missingPrefabs = enable;
			duplicateComponents = enable;
			inconsistentTerrainData = enable;
		}

		internal void SwitchNeatness(bool enable)
		{
			unnamedLayers = enable;
			hugePositions = enable;
		}

		internal void SwitchProjectSettings(bool enable)
		{
			duplicateLayers = enable;
		}

		internal void Reset()
		{
			scanGameObjects = true;
			lookInProjectSettings = true;
			lookInScenes = true;
			scenesSelection = ScenesSelection.AllScenes;
			lookInAssets = true;
			touchInactiveGameObjects = true;
			touchDisabledComponents = true;
			missingComponents = true;
			duplicateComponents = true;
			missingReferences = true;
#if UNITY_2019_1_OR_NEWER
			shadersWithErrors = true;
#endif
			inconsistentTerrainData = true;
			missingPrefabs = true;
			unnamedLayers = true;
			hugePositions = true;
			duplicateLayers = true;
		}
	}
}
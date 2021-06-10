#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using Cleaner;
	using Core;
	using Settings;

	internal class CleanerFiltersWindow : FiltersWindow
	{
		private static CleanerFiltersWindow instance;

		internal static CleanerFiltersWindow Create()
		{
			var window = GetWindow<CleanerFiltersWindow>(true);
			window.Focus();

			return window;
		}

		internal static void Refresh()
		{
			if (instance == null) return;

			instance.InitOnEnable();
			instance.Focus();
		}

		protected override void InitOnEnable()
		{
			TabBase[] tabs =
			{
				new SceneFiltersTab(
					FilterType.Ignores,
					"Ignored scenes will be considered as needed and both scenes and everything referenced in them will be excluded from the garbage search.",
					ProjectSettings.Cleaner.sceneIgnoresFilters,
					ProjectSettings.Cleaner.ignoreScenesInBuild,
					ProjectSettings.Cleaner.ignoreOnlyEnabledScenesInBuild,
					OnSceneIgnoresSettingsChange, OnSceneIgnoresChange),

				new PathFiltersTab(
					FilterType.Ignores,
					"Ignored items will be considered as needed and both items and everything referenced in them will be excluded from the garbage search.",
					ProjectSettings.Cleaner.pathIgnoresFilters,
					false,
					OnPathIgnoresChange, OnGetDefaults),
			};

			Init(ProjectCleaner.ModuleName, tabs, UserSettings.Cleaner.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(FilterItem[] collection)
		{
			ProjectSettings.Cleaner.pathIgnoresFilters = collection;
		}

		private static void OnSceneIgnoresChange(FilterItem[] collection)
		{
			ProjectSettings.Cleaner.sceneIgnoresFilters = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			ProjectSettings.Cleaner.ignoreScenesInBuild = ignoreScenesInBuild;
			ProjectSettings.Cleaner.ignoreOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnTabChange(int newTab)
		{
			UserSettings.Cleaner.filtersTabIndex = newTab;
		}

		private FilterItem[] OnGetDefaults()
		{
			return ProjectCleanerSettings.GetDefaultFilters();
		}
	}
}
#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using Core;
	using Issues;
	using Settings;

	internal class IssuesFiltersWindow : FiltersWindow
	{
		private static IssuesFiltersWindow instance;

		internal static IssuesFiltersWindow Create()
		{
			var window = GetWindow<IssuesFiltersWindow>(true);
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
				new SceneFiltersTab(FilterType.Includes,
									"Only these included scenes will be checked for issues of you'll choose 'Included Scenes' dropdown option at the Scene filtering options.",
									ProjectSettings.Issues.sceneIncludesFilters,
									ProjectSettings.Issues.includeScenesInBuild,
									ProjectSettings.Issues.includeOnlyEnabledScenesInBuild,
									OnSceneIgnoresSettingsChange, OnSceneIncludesChange),

				new PathFiltersTab(FilterType.Includes, null, ProjectSettings.Issues.pathIncludesFilters, true, OnPathIncludesChange),
				new PathFiltersTab(FilterType.Ignores, null, ProjectSettings.Issues.pathIgnoresFilters, true, OnPathIgnoresChange),
				new ComponentFiltersTab(FilterType.Ignores, ProjectSettings.Issues.componentIgnoresFilters, OnComponentIgnoresChange)
			};

			Init(IssuesFinder.ModuleName, tabs, UserSettings.Issues.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnSceneIncludesChange(FilterItem[] collection)
		{
			ProjectSettings.Issues.sceneIncludesFilters = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			ProjectSettings.Issues.includeScenesInBuild = ignoreScenesInBuild;
			ProjectSettings.Issues.includeOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnPathIgnoresChange(FilterItem[] collection)
		{
			ProjectSettings.Issues.pathIgnoresFilters = collection;
		}

		private void OnPathIncludesChange(FilterItem[] collection)
		{
			ProjectSettings.Issues.pathIncludesFilters = collection;
		}

		private void OnComponentIgnoresChange(FilterItem[] collection)
		{
			ProjectSettings.Issues.componentIgnoresFilters = collection;
		}

		private void OnTabChange(int newTab)
		{
			UserSettings.Issues.filtersTabIndex = newTab;
		}
	}
}
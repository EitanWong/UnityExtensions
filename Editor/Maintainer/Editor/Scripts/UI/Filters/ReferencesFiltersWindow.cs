#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using Core;
	using References;
	using Settings;

	internal class ReferencesFiltersWindow : FiltersWindow
	{
		private static ReferencesFiltersWindow instance;

		internal static ReferencesFiltersWindow Create()
		{
			var window = GetWindow<ReferencesFiltersWindow>(true);
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
				new PathFiltersTab(
					FilterType.Includes,
					"Included items will filter root level to have only items added to the includes.",
					ProjectSettings.References.pathIncludesFilters,
					true, OnPathIncludesChange),

				new PathFiltersTab(
					FilterType.Ignores,
					"Ignored items will not be searched for references both as source and a target of the reference.",
					ProjectSettings.References.pathIgnoresFilters,
					false, OnPathIgnoresChange),
			};

			Init(ReferencesFinder.ModuleName, tabs, 0, null);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(FilterItem[] collection)
		{
			ProjectSettings.References.pathIgnoresFilters = collection;
		}

		private void OnPathIncludesChange(FilterItem[] collection)
		{
			ProjectSettings.References.pathIncludesFilters = collection;
		}
	}
}
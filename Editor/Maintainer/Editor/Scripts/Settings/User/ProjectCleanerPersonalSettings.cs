#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;
	using Cleaner;
	using UI;
	using UnityEngine;

	/// <summary>
	/// Project Cleaner module settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// </summary>
	/// Contains user-specific settings for this module.
	/// See IDE hints for all list.
	[Serializable]
	public class ProjectCleanerPersonalSettings
	{
		public int filtersTabIndex = 0;

		public bool firstTime = true;
		public bool trashBinWarningShown = false;
		public bool deletionPromptShown = false;

		public bool muteNoIgnoredScenesWarning;

		[SerializeField] internal RecordsTabState tabState = new RecordsTabState();
		/* sorting */
		[SerializeField] internal CleanerSortingType sortingType = CleanerSortingType.BySize;
		[SerializeField] internal SortingDirection sortingDirection = SortingDirection.Ascending;
	}
}
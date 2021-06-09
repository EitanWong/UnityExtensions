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
	/// Issues Finder module settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// </summary>
	/// Contains user-specific settings for this module.
	/// See IDE hints for all list.
	[Serializable]
	public class IssuesFinderPersonalSettings
	{
		public int filtersTabIndex = 0;

		[SerializeField] internal RecordsTabState tabState = new RecordsTabState();
		/* sorting */
		[SerializeField] internal IssuesSortingType sortingType = IssuesSortingType.BySeverity;
		[SerializeField] internal SortingDirection sortingDirection = SortingDirection.Ascending;
		
		[SerializeField] internal bool scanGameObjectsFoldout;
		[SerializeField] internal bool gameObjectsFoldout;
		[SerializeField] internal bool commonFoldout;
		[SerializeField] internal bool neatnessFoldout;
		[SerializeField] internal bool projectSettingsFoldout;

		public IssuesFinderPersonalSettings()
		{
			Reset();
		}
		
		internal void Reset()
		{
			gameObjectsFoldout = true;
			commonFoldout = false;
			neatnessFoldout = false;
			projectSettingsFoldout = true;
		}
	}
}
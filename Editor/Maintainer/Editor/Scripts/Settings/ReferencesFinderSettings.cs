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
	/// References Finder module settings saved in ProjectSettings folder.
	/// </summary>
	/// Contains only filtering settings so far.
	[Serializable]
	public class ReferencesFinderSettings
	{
		public FilterItem[] pathIgnoresFilters = new FilterItem[0];
		public FilterItem[] pathIncludesFilters = new FilterItem[0];
	}
}
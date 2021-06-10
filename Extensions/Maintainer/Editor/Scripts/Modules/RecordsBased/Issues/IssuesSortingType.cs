#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Cleaner
{
	internal enum IssuesSortingType : byte
	{
		Unsorted,
		ByIssueType,
		BySeverity,
		ByPath
	}
}
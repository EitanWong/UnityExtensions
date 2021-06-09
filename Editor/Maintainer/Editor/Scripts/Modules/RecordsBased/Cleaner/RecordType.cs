#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Cleaner
{
	/// <summary>
	/// Type of the ProjectCleaner result item.
	/// </summary>
	public enum RecordType
	{
		EmptyFolder = 0,
		UnreferencedAsset = 20,
		Warning = 4000,
		Error = 5000,
		Other = 100000
	}
}
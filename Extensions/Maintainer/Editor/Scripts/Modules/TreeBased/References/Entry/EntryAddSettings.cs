#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	internal class EntryAddSettings
	{
		public string prefix;
		public string suffix;

		public string componentName;
		public string propertyPath;

		public int componentIndex = -1;
		public int componentInstanceId = -1;
	}
}
#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Detectors
{
	using System.Collections.Generic;

	internal abstract class IssueDetectorBase
	{
		protected readonly List<IssueRecord> issues;

		protected IssueDetectorBase(List<IssueRecord> issues)
		{
			this.issues = issues;
		}
	}
}
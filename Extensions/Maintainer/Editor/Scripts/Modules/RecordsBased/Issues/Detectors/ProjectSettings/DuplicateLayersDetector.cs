#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Detectors
{
	using System.Collections.Generic;
	using Settings;

	internal class DuplicateLayersDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.duplicateLayers;

		public DuplicateLayersDetector(List<IssueRecord> issues) : base(issues) { }

		public void TryDetectIssue()
		{
			if (!enabled) return;

			var issue = SettingsChecker.CheckTagsAndLayers();
			if (issue != null)
			{
				issues.Add(issue);
			}
		}
	}
}
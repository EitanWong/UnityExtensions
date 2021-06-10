#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Detectors
{
	using System.Collections.Generic;
	using Settings;
	using UnityEngine;

	internal class EmptyLayerNameDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.unnamedLayers;

		public EmptyLayerNameDetector(List<IssueRecord> issues) : base(issues) { }

		public void TryDetectIssue(RecordLocation location, string assetPath, GameObject target)
		{
			if (!enabled) return;

			var layerIndex = target.layer;
			if (!string.IsNullOrEmpty(LayerMask.LayerToName(layerIndex))) return;

			var issue = GameObjectIssueRecord.Create(IssueKind.UnnamedLayer, location, assetPath, target);
			issue.headerExtra = "(index: " + layerIndex + ")";
			issues.Add(issue);
		}
	}
}
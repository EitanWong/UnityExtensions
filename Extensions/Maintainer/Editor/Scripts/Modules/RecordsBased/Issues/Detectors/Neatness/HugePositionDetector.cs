#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Detectors
{
	using System;
	using System.Collections.Generic;
	using Settings;
	using Tools;
	using UnityEngine;

	internal class HugePositionDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.hugePositions;

		public HugePositionDetector(List<IssueRecord> issues) : base(issues) { }

		public void TryDetectIssue(RecordLocation location, string assetPath, GameObject target)
		{
			if (!enabled) return;

			if (IsTransformHasHugePosition(target.transform))
			{
				var record = GameObjectIssueRecord.Create(IssueKind.HugePosition, location, assetPath, target,
					CSReflectionTools.transformType, "Transform", 0, "Position");
				issues.Add(record);
			}
		}

		private bool IsTransformHasHugePosition(Transform transform)
		{
			var position = transform.position;
			return Math.Abs(position.x) > 100000f || Math.Abs(position.y) > 100000f || Math.Abs(position.z) > 100000f;
		}
	}
}
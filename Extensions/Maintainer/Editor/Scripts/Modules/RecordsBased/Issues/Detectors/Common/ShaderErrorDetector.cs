#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

#if UNITY_2019_1_OR_NEWER
namespace CodeStage.Maintainer.Issues.Detectors
{
	using System.Collections.Generic;
	using Core;
	using Settings;
	using UnityEditor;
	using UnityEngine;

	internal class ShaderErrorDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.shadersWithErrors;

		public ShaderErrorDetector(List<IssueRecord> issues) : base(issues) { }

		public bool TryDetectIssue(AssetInfo asset, Shader shader)
		{
			if (!enabled) return false;

			if (ShaderUtil.ShaderHasError(shader))
			{
				issues.Add(ShaderIssueRecord.Create(asset.Path));
			}

			return true;
		}
	}
}
#endif
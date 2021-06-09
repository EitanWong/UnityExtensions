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
	using UnityEngine;

	internal class InconsistentTerrainDataDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.inconsistentTerrainData;

		private TerrainData terrainTerrainData;
		private TerrainData terrainColliderTerrainData;
		private bool terrainChecked;
		private bool terrainColliderChecked;

		private Type componentType;
		private string componentName;
		private int componentIndex;

		public InconsistentTerrainDataDetector(List<IssueRecord> issues) : base(issues) { }

		public void ProcessTerrainComponent(Terrain component, Type type, string name, int index)
		{
			if (!enabled) return;

			componentType = type;
			componentName = name;
			componentIndex = index;

			terrainTerrainData = component.terrainData;
			terrainChecked = true;
		}

		public void ProcessTerrainColliderComponent(TerrainCollider component)
		{
			if (!enabled) return;

			terrainColliderTerrainData = component.terrainData;
			terrainColliderChecked = true;
		}

		public void TryDetectIssue(RecordLocation currentLocation, string assetPath, GameObject target)
		{
			if (!enabled) return;

			if (terrainChecked && terrainColliderChecked && terrainColliderTerrainData != terrainTerrainData)
			{
				issues.Add(GameObjectIssueRecord.Create(IssueKind.InconsistentTerrainData, currentLocation, assetPath, target, componentType, componentName, componentIndex));
			}

			terrainChecked = false;
			terrainColliderChecked = false;

			terrainTerrainData = null;
			terrainColliderTerrainData = null;
		}
	}
}
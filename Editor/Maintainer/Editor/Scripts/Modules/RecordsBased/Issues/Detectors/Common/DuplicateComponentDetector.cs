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

	internal class DuplicateComponentDetector : IssueDetectorBase
	{
		private class ComponentDuplicateInfo
		{
			public readonly Component reference;
			public long hash;

			public ComponentDuplicateInfo(Component reference)
			{
				this.reference = reference;
			}
		}

		private readonly bool enabled = ProjectSettings.Issues.duplicateComponents;

		private Dictionary<Type, List<ComponentDuplicateInfo>> uniqueTypes;
		private List<ComponentDuplicateInfo> existingData;
		private long newHash;

		public DuplicateComponentDetector(List<IssueRecord> issues) : base(issues) { }

		public void StartGameObject()
		{
			if (!enabled) return;

			if (uniqueTypes == null)
			{
				uniqueTypes = new Dictionary<Type, List<ComponentDuplicateInfo>>();
			}
			else
			{
				uniqueTypes.Clear();
			}
		}

		public void ProcessComponent(Component component, Type componentType)
		{
			if (!enabled) return;

			existingData = null;
			newHash = 0;

			if (!uniqueTypes.TryGetValue(componentType, out existingData))
			{
				var newData = new List<ComponentDuplicateInfo>
				{
					new ComponentDuplicateInfo(component)
				};

				uniqueTypes.Add(componentType, newData);
			}
			else
			{
				foreach (var duplicateInfo in existingData)
				{
					if (duplicateInfo.hash == 0)
					{
						var initialInfo = new SerializedObjectTraverseInfo(duplicateInfo.reference);
						var duplicateInfoInClosure = duplicateInfo;
						CSTraverseTools.TraverseObjectProperties(initialInfo, (info, property) =>
						{
							duplicateInfoInClosure.hash += CSEditorTools.GetPropertyHash(property);
						});
					}
				}

				existingData.Add(new ComponentDuplicateInfo(component));
			}
		}

		public void SkipComponent()
		{
			existingData = null;
			newHash = 0;
		}

		public bool IsPropertiesHashCalculationRequired()
		{
			return existingData != null;
		}

		public void ProcessProperty(UnityEditor.SerializedProperty property)
		{
			newHash += CSEditorTools.GetPropertyHash(property);
		}

		public void TryDetectIssue(RecordLocation location, string assetPath, GameObject target, Type componentType, string componentName, int orderIndex)
		{
			foreach (var duplicateInfo in existingData)
			{
				if (duplicateInfo.hash == newHash)
				{
					var issue = GameObjectIssueRecord.Create(IssueKind.DuplicateComponent, location,
						assetPath, target, componentType, componentName, orderIndex);
					issues.Add(issue);
					break;
				}
			}
		}
	}
}
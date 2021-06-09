#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Text;
	using Core;
	using Tools;
	using UI;
	using UnityEditor;
	using UnityEngine;

	[Serializable]
	public class ScriptableObjectIssueRecord : AssetIssueRecord, IShowableRecord
	{
		public string propertyPath;
		public string typeName;

		[SerializeField]
		private bool missingEventMethod;

		public override bool IsFixable
		{
			get
			{
				return Kind == IssueKind.MissingReference && !missingEventMethod;
			}
		}

		public void Show()
		{
			if (!CSSelectionTools.RevealAndSelectFileAsset(Path))
			{
				MaintainerWindow.ShowNotification("Can't show it properly");
			}
		}

		public static ScriptableObjectIssueRecord Create(IssueKind type, string path)
		{
			return new ScriptableObjectIssueRecord(type, path);
		}

		public static ScriptableObjectIssueRecord Create(IssueKind type, string path, string typeName)
		{
			return new ScriptableObjectIssueRecord(type, path, typeName);
		}

		public static ScriptableObjectIssueRecord Create(IssueKind type, string path, string typeName, string property)
		{
			return new ScriptableObjectIssueRecord(type, path, typeName, property);
		}

		internal override bool MatchesFilter(FilterItem newFilter)
		{
			var filters = new[] { newFilter };

			switch (newFilter.kind)
			{
				case FilterKind.Path:
				case FilterKind.Directory:
				case FilterKind.FileName:
				case FilterKind.Extension:
					return !string.IsNullOrEmpty(Path) && CSFilterTools.IsValueMatchesAnyFilterOfKind(Path, filters, newFilter.kind);
				case FilterKind.Type:
				{
					return !string.IsNullOrEmpty(typeName) && CSFilterTools.IsValueMatchesAnyFilterOfKind(typeName, filters, newFilter.kind);
				}
				case FilterKind.NotSet:
					return false;
				default:
					Debug.LogWarning(Maintainer.LogPrefix + "Unknown filter kind: " + newFilter.kind);
					return false;
			}
		}

		protected ScriptableObjectIssueRecord(IssueKind kind, string path) : base(kind, RecordLocation.Asset, path)
		{

		}

		protected ScriptableObjectIssueRecord(IssueKind kind, string path, string typeName) : this(kind, path)
		{
			this.typeName = typeName;
		}

		protected ScriptableObjectIssueRecord(IssueKind kind, string path, string typeName, string propertyPath) : this(kind, path, typeName)
		{
			this.propertyPath = propertyPath;

			if (propertyPath.EndsWith("].m_MethodName", StringComparison.OrdinalIgnoreCase))
			{
				missingEventMethod = true;
			}
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Scriptable Object:</b> ");
			text.Append(CSPathTools.NicifyAssetPath(Path, true));

			if (!string.IsNullOrEmpty(typeName))
			{
				text.Append("\n<b>Type:</b>").Append(typeName);
			}

			if (!string.IsNullOrEmpty(propertyPath))
			{
				var propertyName = CSObjectTools.GetNicePropertyPath(propertyPath);
				text.Append("\n<b>Property:</b> ").Append(propertyName);
			}
		}

		internal override FixResult PerformFix(bool batchMode)
		{
			FixResult result;
			var scriptableObjectAsset = AssetDatabase.LoadMainAssetAtPath(Path);

			if (scriptableObjectAsset == null)
			{
				result = new FixResult(false);
				if (batchMode)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Can't find Scriptable Object for issue:\n" + this);
				}
				else
				{
					result.SetErrorText("Couldn't find Scriptable Object\n" + Path);
				}
				return result;
			}

			result = IssuesFixer.FixMissingReference(scriptableObjectAsset, propertyPath, RecordLocation.Asset);
			return result;
		}
	}
}
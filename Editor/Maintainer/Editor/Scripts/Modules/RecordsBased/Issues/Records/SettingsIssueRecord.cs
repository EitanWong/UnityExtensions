#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Text;
	using Core;
	using Tools;
	using UnityEditor;
	using UnityEngine;

	[Serializable]
	public class SettingsIssueRecord : AssetIssueRecord, IShowableRecord
	{
		public string PropertyPath { get; private set; }
		internal AssetSettingsKind SettingsKind { get; private set; }

		public override bool IsFixable
		{
			get
			{
				return Kind == IssueKind.MissingReference;
			}
		}

		public void Show()
		{
			CSEditorTools.RevealInSettings(SettingsKind);
		}

		internal static SettingsIssueRecord Create(AssetSettingsKind settingsKind, IssueKind type, string body)
		{
			return new SettingsIssueRecord(settingsKind, type, body);
		}

		internal static SettingsIssueRecord Create(AssetSettingsKind settingsKind, IssueKind type, string path, string propertyPath)
		{
			return new SettingsIssueRecord(settingsKind, type, path, propertyPath);
		}

		internal override bool MatchesFilter(FilterItem newFilter)
		{
			return false;
		}

		protected SettingsIssueRecord(AssetSettingsKind settingsKind, IssueKind kind, string body):base(kind, RecordLocation.Asset, null)
		{
			SettingsKind = settingsKind;
			bodyExtra = body;
		}

		protected SettingsIssueRecord(AssetSettingsKind settingsKind, IssueKind kind, string path, string propertyPath) : base(kind, RecordLocation.Asset, path)
		{
			SettingsKind = settingsKind;
			PropertyPath = propertyPath;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Settings: </b>" + SettingsKind);
			if (!string.IsNullOrEmpty(PropertyPath))
			{
				var propertyName = CSObjectTools.GetNicePropertyPath(PropertyPath);
				text.Append("\n<b>Property:</b> ").Append(propertyName);
			}
		}

		internal override FixResult PerformFix(bool batchMode)
		{
			FixResult result;
			var assetObject = AssetDatabase.LoadMainAssetAtPath(Path);

			// workaround for Unity 5.6 issue: LoadMainAssetAtPath returns null for settings assets
			if (assetObject == null)
			{
				var allObjects = AssetDatabase.LoadAllAssetsAtPath(Path);
				if (allObjects != null && allObjects.Length > 0)
				{
					assetObject = allObjects[0];
				}
			}

			if (assetObject == null)
			{
				result = new FixResult(false);
				if (batchMode)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Couldn't find settings asset for issue:\n" + this);
				}
				else
				{
					result.SetErrorText("Couldn't find settings asset at\n" + Path);
				}
				return result;
			}

			result = IssuesFixer.FixMissingReference(assetObject, PropertyPath, RecordLocation.Asset);
			return result;
		}
	}
}
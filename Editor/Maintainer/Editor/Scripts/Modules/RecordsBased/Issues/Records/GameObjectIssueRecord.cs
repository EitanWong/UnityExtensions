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
	using UnityEngine;
	using Object = UnityEngine.Object;
	using Tools;
	using UI;

	[Serializable]
	public class GameObjectIssueRecord : AssetIssueRecord, IShowableRecord
	{
		public string transformPath;
		public long objectId;
		public string componentName;
		public string componentNamePostfix;
		public long componentIndex = -1;
		public string propertyPath;

		[SerializeField]
		private bool missingEventMethod;

		public override bool IsFixable
		{
			get
			{
				return (Kind == IssueKind.MissingComponent || Kind == IssueKind.MissingReference) && !missingEventMethod;
			}
		}

		public void Show()
		{
			if (!CSSelectionTools.RevealAndSelectGameObject(Path, transformPath, objectId, componentIndex))
			{
				MaintainerWindow.ShowNotification("Can't show it properly");
			}
		}

		internal static GameObjectIssueRecord Create(IssueKind type, RecordLocation location, string path, GameObject gameObject)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject);
		}

		internal static GameObjectIssueRecord Create(IssueKind type, RecordLocation location, string path, GameObject gameObject, Type componentType, string componentName, int componentIndex)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, componentType, componentName, componentIndex);
		}

		internal static GameObjectIssueRecord Create(IssueKind type, RecordLocation location, string path, GameObject gameObject, Type componentType, string componentName, int componentIndex, string propertyPath)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, componentType, componentName, componentIndex, propertyPath);
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
					return !string.IsNullOrEmpty(componentName) && CSFilterTools.IsValueMatchesAnyFilterOfKind(componentName, filters, newFilter.kind);
				}
				case FilterKind.NotSet:
					return false;
				default:
					Debug.LogWarning(Maintainer.LogPrefix + "Unknown filter kind: " + newFilter.kind);
					return false;
			}
		}

		protected GameObjectIssueRecord(IssueKind kind, RecordLocation location, string path, GameObject gameObject):base(kind, location, path)
		{
			var transform = gameObject.transform;
			transformPath = CSEditorTools.GetFullTransformPath(transform);

			if (kind != IssueKind.MissingPrefab)
			{
				objectId = CSObjectTools.GetUniqueObjectId(gameObject);
			}
		}

		protected GameObjectIssueRecord(IssueKind kind, RecordLocation location, string path, GameObject gameObject, Type componentType, string componentName, int componentIndex) : this(kind, location, path, gameObject)
		{
			this.componentName = componentName;
			this.componentIndex = componentIndex;
			if (this.componentIndex > 0 && componentType != null && gameObject.GetComponents(componentType).Length > 1)
			{
				componentNamePostfix = " (#" + this.componentIndex + ")";
			}
		}

		protected GameObjectIssueRecord(IssueKind kind, RecordLocation location, string path, GameObject gameObject, Type componentType, string componentName, int componentIndex, string propertyPath) :this(kind, location, path, gameObject, componentType, componentName, componentIndex)
		{
			this.propertyPath = propertyPath;

			if (propertyPath.EndsWith("].m_MethodName", StringComparison.OrdinalIgnoreCase))
			{
				missingEventMethod = true;
			}
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append(Location == RecordLocation.Scene ? "<b>Scene:</b> " : "<b>Prefab:</b> ");

			var nicePath = CSPathTools.NicifyAssetPath(Path, true);

			text.Append(nicePath);

			if (!string.IsNullOrEmpty(transformPath)) text.Append("\n<b>Object:</b> ").Append(transformPath);
			if (!string.IsNullOrEmpty(componentName)) text.Append("\n<b>Component:</b> ").Append(componentName);
			if (!string.IsNullOrEmpty(componentNamePostfix)) text.Append(componentNamePostfix);
			if (!string.IsNullOrEmpty(propertyPath))
			{
				var propertyName = CSObjectTools.GetNicePropertyPath(propertyPath);
				text.Append("\n<b>Property:</b> ").Append(propertyName);
			}
		}

		internal override FixResult PerformFix(bool batchMode)
		{
			Component component = null;
			FixResult result;

			CSSceneTools.OpenSceneResult openSceneResult = null;

			if (!batchMode && Location == RecordLocation.Scene)
			{
				openSceneResult = CSSceneTools.OpenScene(Path);
				if (!openSceneResult.success)
				{
					return FixResult.CreateError("Couldn't open scene");
				}
			}

			var obj = GetObjectWithThisIssue();
			if (obj == null)
			{
				result = new FixResult(false);
				if (batchMode)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Can't find Object for issue:\n" + this);
				}
				else
				{
					result.SetErrorText("Couldn't find Object\n" + transformPath);
				}
				return result;
			}

			if (!string.IsNullOrEmpty(componentName) && obj is GameObject)
			{
				component = GetComponentWithThisIssue(obj as GameObject);

				if (component == null)
				{
					result = new FixResult(false);
					if (batchMode)
					{
						Debug.LogWarning(Maintainer.LogPrefix + "Can't find component for issue:\n" + this);
					}
					else
					{
						result.SetErrorText("Can't find component\n" + componentName);
					}

					return result;
				}
			}

			result = IssuesFixer.FixObjectIssue(this, obj, component, Kind);

			if (!batchMode && Location == RecordLocation.Scene && openSceneResult != null)
			{
				CSSceneTools.SaveScene(openSceneResult.scene);
				CSSceneTools.CloseOpenedSceneIfNeeded(openSceneResult);
			}

			return result;
		}

		private Object GetObjectWithThisIssue()
		{
			Object result = null;

			if (Location == RecordLocation.Scene)
			{
				var scene = CSSceneTools.GetSceneByPath(Path);
				result = CSObjectTools.FindGameObjectInScene(scene, objectId, transformPath);
			}
			else
			{
				//var assetFile = AssetsMap.GetAssetInfoWithPath(path);
				var prefabRoot = CSPrefabTools.GetPrefabAssetRoot(Path);
				if (prefabRoot != null)
				{
					result = CSObjectTools.FindChildGameObjectRecursive(prefabRoot.transform, objectId, prefabRoot.transform.name, transformPath);
				}
			}
			return result;
		}

		private Component GetComponentWithThisIssue(GameObject go)
		{
			Component component = null;
			var components = go.GetComponents<Component>();
			for (var i = 0; i < components.Length; i++)
			{
				if (i == componentIndex)
				{
					component = components[i];
					break;
				}
			}

			return component;
		}
	}
}
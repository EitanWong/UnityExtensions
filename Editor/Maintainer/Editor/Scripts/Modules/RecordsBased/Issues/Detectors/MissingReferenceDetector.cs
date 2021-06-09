#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Detectors
{
	using System;
	using System.Collections.Generic;
	using Core;
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;
	using UnityEngine.Events;

	internal class MissingReferenceDetector : IssueDetectorBase
	{
		private readonly bool enabled = ProjectSettings.Issues.missingReferences;
		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		public MissingReferenceDetector(List<IssueRecord> issues) : base(issues) { }

		public void TryDetectIssuesInSceneSettings(AssetInfo currentAsset)
		{
			if (!enabled) return;

			var newIssues = SettingsChecker.CheckSceneSettingsForMissingReferences(currentAsset);
			if (newIssues.Count > 0)
			{
				issues.AddRange(newIssues);
			}
		}

		public void TryDetectIssuesInSettingsAsset(AssetInfo asset, AssetSettingsKind kind)
		{
			if (!enabled) return;

			var newIssues = SettingsChecker.CheckSettingsAssetForMissingReferences(asset, kind);
			if (newIssues.Count > 0)
			{
				issues.AddRange(newIssues);
			}
		}

		public bool TryDetectScriptableObjectIssue(string assetPath, string typeName, SerializedProperty property)
		{
			return TryDetectIssue(RecordLocation.Asset, assetPath, null, null, typeName, -1, property);
		}

		public bool TryDetectIssue(RecordLocation location, string assetPath, GameObject target, Type componentType, string componentName, int orderIndex, SerializedProperty property)
		{
			if (!enabled || !IsPropertyHasMissingReference(property)) return false;

			AddIssue(location, assetPath, target, componentType, componentName, orderIndex, property.propertyPath);
			return true;
		}

		public void TryDetectScriptableObjectUnityEventIssue(string assetPath, string typeName, SerializedProperty property)
		{
			TryDetectUnityEventIssues(RecordLocation.Asset, assetPath, null, null, typeName, -1, property);
		}

		public void TryDetectUnityEventIssues(RecordLocation location, string assetPath, GameObject target, Type componentType, string componentName, int orderIndex, SerializedProperty property)
		{
			if (!enabled) return;

			var callbacks = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
			if (callbacks == null || callbacks.isArray == false)
			{
				Maintainer.ConstructReportWarning("Couldn't find m_PersistentCalls in serialized UnityEvent!", IssuesFinder.ModuleName);
				return;
			}

			IssuesDetector.duplicateComponentDetector.ProcessProperty(callbacks);

			var callsCount = callbacks.arraySize;
			for (var i = 0; i < callsCount; i++)
			{
				var call = callbacks.GetArrayElementAtIndex(i);
				var callTarget = call.FindPropertyRelative("m_Target");
				if (callTarget == null || callTarget.propertyType != SerializedPropertyType.ObjectReference)
				{
					Maintainer.ConstructReportWarning("Couldn't find m_Target in serialized UnityEvent's call!", IssuesFinder.ModuleName);
					return;
				}

				if (IsPropertyHasMissingReference(callTarget))
				{
					AddIssue(location, assetPath, target, componentType, componentName, orderIndex,
						callTarget.propertyPath);
					return;
				}

				var callTargetObject = callTarget.objectReferenceValue;

				// no target set
				if (callTargetObject == null) continue;

				IssuesDetector.duplicateComponentDetector.ProcessProperty(callTarget);

				var methodName = call.FindPropertyRelative("m_MethodName");
				if (methodName == null || methodName.propertyType != SerializedPropertyType.String)
				{
					Maintainer.ConstructReportWarning("Couldn't find m_MethodName in serialized UnityEvent's call!", IssuesFinder.ModuleName);
					return;
				}

				IssuesDetector.duplicateComponentDetector.ProcessProperty(methodName);

				var methodNameValue = methodName.stringValue;

				// no function set
				if (string.IsNullOrEmpty(methodNameValue)) continue;

				var arguments = call.FindPropertyRelative("m_Arguments");
				if (arguments == null)
				{
					Maintainer.ConstructReportWarning("Couldn't find m_Arguments in serialized UnityEvent's call!", IssuesFinder.ModuleName);
					return;
				}

				var objectArgumentAssemblyTypeName = arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
				if (objectArgumentAssemblyTypeName == null || objectArgumentAssemblyTypeName.propertyType != SerializedPropertyType.String)
				{
					Maintainer.ConstructReportWarning("Couldn't find m_ObjectArgumentAssemblyTypeName in m_Arguments!", IssuesFinder.ModuleName);
					return;
				}

				IssuesDetector.duplicateComponentDetector.ProcessProperty(objectArgumentAssemblyTypeName);

				var mode = call.FindPropertyRelative("m_Mode");
				if (mode == null || mode.propertyType != SerializedPropertyType.Enum)
				{
					Maintainer.ConstructReportWarning("Couldn't find m_Mode in serialized UnityEvent's call!", IssuesFinder.ModuleName);
					return;
				}

				IssuesDetector.duplicateComponentDetector.ProcessProperty(mode);

				var modeValue = (PersistentListenerMode)mode.enumValueIndex;

				var dummyEvent = CSReflectionTools.GetDummyEvent(property);
				if (dummyEvent == null)
				{
					Maintainer.ConstructReportWarning("Couldn't get something from GetDummyEvent!", IssuesFinder.ModuleName);
					return;
				}

				var type = CSReflectionTools.objectType;
				var stringValue = objectArgumentAssemblyTypeName.stringValue;

				if (!string.IsNullOrEmpty(stringValue))
					type = Type.GetType(stringValue, false) ?? typeof(UnityEngine.Object);

				if (!UnityEventDrawer.IsPersistantListenerValid(dummyEvent, methodNameValue, callTargetObject, modeValue, type))
				{
					AddIssue(location, assetPath, target, componentType, componentName, orderIndex,
						methodName.propertyPath);
					return;
				}
			}
		}

		public static bool IsPropertyHasMissingReference(SerializedProperty currentProperty)
		{
			if (currentProperty.propertyType != SerializedPropertyType.ObjectReference) return false;
			if (currentProperty.objectReferenceValue != null) return false;
			
			if (currentProperty.objectReferenceInstanceIDValue != 0)
			{
				return true;
			}

			var fileId = currentProperty.FindPropertyRelative("m_FileID");
			if (fileId != null)
			{
				if (fileId.intValue != 0) return true;
			}
			else
			{
				Maintainer.ConstructReportWarning(
					"Property seems to be missing reference but m_FileID could not be found!",
					IssuesFinder.ModuleName);
			}

			return false;
		}

		private void AddIssue(RecordLocation location, string assetPath, GameObject target, Type componentType, string componentName, int orderIndex, string propertyPath)
		{
			IssueRecord record;

			if (location != RecordLocation.Asset)
			{
				record = GameObjectIssueRecord.Create(IssueKind.MissingReference, location, assetPath, target, componentType, componentName, orderIndex, propertyPath);
			}
			else
			{
				record = ScriptableObjectIssueRecord.Create(IssueKind.MissingReference, assetPath, componentName, propertyPath);
			}

			issues.Add(record);
		}
	}
}
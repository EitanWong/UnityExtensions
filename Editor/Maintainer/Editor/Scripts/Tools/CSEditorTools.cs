#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;

	using UnityEditor;
	using UnityEngine;

	using Core;

	internal static class CSEditorTools
	{
		private static readonly string[] sizes = { "B", "KB", "MB", "GB" };
		private static TextInfo textInfo;

		internal static CSSceneTools.OpenSceneResult lastRevealSceneOpenResult;

		public static string FormatBytes(double bytes)
		{
			var order = 0;

			while (bytes >= 1024 && order + 1 < 4)
			{
				order++;
				bytes = bytes / 1024;
			}

			return string.Format("{0:0.##} {1}", bytes, sizes[order]);
		}

		public static int GetPropertyHash(SerializedProperty sp)
		{
			var stringHash = new StringBuilder();

			stringHash.Append(sp.type);

			if (sp.isArray)
			{
				stringHash.Append(sp.arraySize);
			}
			else
			{
				switch (sp.propertyType)
				{
					case SerializedPropertyType.AnimationCurve:
						if (sp.animationCurveValue != null)
						{
							stringHash.Append(sp.animationCurveValue.length);
							if (sp.animationCurveValue.keys != null)
							{
								foreach (var key in sp.animationCurveValue.keys)
								{
									stringHash.Append(key.value)
											  .Append(key.time)
											  .Append(key.outTangent)
											  .Append(key.inTangent);
								}
							}
						}
						break;
					case SerializedPropertyType.ArraySize:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Boolean:
						stringHash.Append(sp.boolValue);
						break;
					case SerializedPropertyType.Bounds:
						stringHash.Append(sp.boundsValue.GetHashCode());
						break;
					case SerializedPropertyType.Character:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Generic: // looks like arrays which we already walk through
						break;
					case SerializedPropertyType.Gradient: // unsupported
						break;
					case SerializedPropertyType.ObjectReference:
						if (sp.objectReferenceValue != null)
						{
							stringHash.Append(sp.objectReferenceValue.name);
						}
						break;
					case SerializedPropertyType.Color:
						stringHash.Append(sp.colorValue.GetHashCode());
						break;
					case SerializedPropertyType.Enum:
						stringHash.Append(sp.enumValueIndex);
						break;
					case SerializedPropertyType.Float:
						stringHash.Append(sp.floatValue);
						break;
					case SerializedPropertyType.Integer:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.LayerMask:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Quaternion:
						stringHash.Append(sp.quaternionValue.GetHashCode());
						break;
					case SerializedPropertyType.Rect:
						stringHash.Append(sp.rectValue.GetHashCode());
						break;
					case SerializedPropertyType.String:
						stringHash.Append(sp.stringValue);
						break;
					case SerializedPropertyType.Vector2:
						stringHash.Append(sp.vector2Value.GetHashCode());
						break;
					case SerializedPropertyType.Vector3:
						stringHash.Append(sp.vector3Value.GetHashCode());
						break;
					case SerializedPropertyType.Vector4:
						stringHash.Append(sp.vector4Value.GetHashCode());
						break;
					case SerializedPropertyType.ExposedReference:
						if (sp.exposedReferenceValue != null)
						{
							stringHash.Append(sp.exposedReferenceValue.name);
						}
						break;
					case SerializedPropertyType.Vector2Int:
						stringHash.Append(sp.vector2IntValue.GetHashCode());
						break;
					case SerializedPropertyType.Vector3Int:
						stringHash.Append(sp.vector3IntValue.GetHashCode());
						break;
					case SerializedPropertyType.RectInt:
						stringHash.Append(sp.rectIntValue.position.GetHashCode()).Append(sp.rectIntValue.size.GetHashCode());
						break;
					case SerializedPropertyType.BoundsInt:
						stringHash.Append(sp.boundsIntValue.GetHashCode());
						break;
					case SerializedPropertyType.FixedBufferSize:
						stringHash.Append(sp.fixedBufferSize);
						break;
					default:
						Debug.LogWarning(Maintainer.LogPrefix + "Unknown SerializedPropertyType: " + sp.propertyType);
						break;
				}
			}

			return stringHash.ToString().GetHashCode();
		}

		public static string GetFullTransformPath(Transform transform, Transform stopAt = null)
		{
			var path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				if (transform == stopAt) break;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static EditorWindow GetInspectorWindow()
		{
			if (CSReflectionTools.inspectorWindowType == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't find UnityEditor.InspectorWindow type!"));
				return null;
			}

			var inspectorWindow = EditorWindow.GetWindow(CSReflectionTools.inspectorWindowType);
			if (inspectorWindow == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get an InspectorWindow!"));
				return null;
			}

			return inspectorWindow;
		}

		public static ActiveEditorTracker GetActiveEditorTrackerForSelectedObject()
		{
			var inspectorWindow = GetInspectorWindow();
			if (inspectorWindow == null) return null;
			if (CSReflectionTools.inspectorWindowType == null) return null;

			inspectorWindow.Repaint();

			ActiveEditorTracker result = null;

			var trackerProperty = CSReflectionTools.GetPropertyInfo(CSReflectionTools.inspectorWindowType, "tracker");
			if (trackerProperty != null)
			{
				result = (ActiveEditorTracker)trackerProperty.GetValue(inspectorWindow, null);
			}
			else
			{
				Debug.LogError(Maintainer.ConstructError("Can't get ActiveEditorTracker from the InspectorWindow!"));
			}

			return result;
		}

		public static bool RevealInSettings(AssetSettingsKind settingsKind, string path = null)
		{
			var result = true;

			switch (settingsKind)
			{
				case AssetSettingsKind.AudioManager:
				case AssetSettingsKind.ClusterInputManager:
				case AssetSettingsKind.InputManager:
				case AssetSettingsKind.NavMeshAreas:
				case AssetSettingsKind.NavMeshLayers:
				case AssetSettingsKind.NavMeshProjectSettings:
				case AssetSettingsKind.NetworkManager:
					break;

				case AssetSettingsKind.NotSettings:
					Debug.LogWarning(Maintainer.ConstructWarning("Can't open settings of kind NotSettings Oo"));
					result = false;
					break;
				case AssetSettingsKind.DynamicsManager:
					if (!CSMenuTools.ShowProjectSettingsPhysics())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Physics Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.EditorBuildSettings:
					if (!CSMenuTools.ShowEditorBuildSettings())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open EditorBuildSettings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.EditorSettings:
					if (!CSMenuTools.ShowEditorSettings())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Editor Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.GraphicsSettings:
					if (!CSMenuTools.ShowProjectSettingsGraphics())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open GraphicsSettings!"));
						result = false;
					}
					break;

				case AssetSettingsKind.Physics2DSettings:
					if (!CSMenuTools.ShowProjectSettingsPhysics2D())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Physics2D Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.ProjectSettings:
					if (!CSMenuTools.ShowProjectSettingsPlayer())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Player Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.PresetManager:
					if (!CSMenuTools.ShowProjectSettingsPresetManager())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Preset Manager!"));
						result = false;
					}
					break;
				case AssetSettingsKind.QualitySettings:
					break;
				case AssetSettingsKind.TagManager:
					if (!CSMenuTools.ShowProjectSettingsTagsAndLayers())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Tags and Layers Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.TimeManager:
					break;
				case AssetSettingsKind.UnityAdsSettings:
					break;
				case AssetSettingsKind.UnityConnectSettings:
					break;
				case AssetSettingsKind.VFXManager:
					if (!CSMenuTools.ShowProjectSettingsVFX())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open VFX Settings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.Unknown:
					if (!string.IsNullOrEmpty(path)) EditorUtility.RevealInFinder(path);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return result;
		}

		public static string NicifyName(string name)
		{
			var nicePropertyName = ObjectNames.NicifyVariableName(name);
			if (textInfo == null) textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(nicePropertyName);
		}

		public static AssetKind GetAssetKind(string path)
		{
			if (!Path.IsPathRooted(path))
			{
				if (path.IndexOf("Assets/", StringComparison.Ordinal) == 0)
				{
					return AssetKind.Regular;
				}

				if (path.IndexOf("ProjectSettings/", StringComparison.Ordinal) == 0)
				{
					return AssetKind.Settings;
				}

				if (path.IndexOf("Packages/", StringComparison.Ordinal) == 0)
				{
					return AssetKind.FromPackage;
				}
			}
			else
			{
				if (path.IndexOf("/unity/cache/packages/", StringComparison.OrdinalIgnoreCase) > 0)
				{
					return AssetKind.FromPackage;
				}
			}

			return AssetKind.Unsupported;
		}
	}
}
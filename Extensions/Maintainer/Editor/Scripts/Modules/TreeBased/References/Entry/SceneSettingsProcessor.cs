#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using System.Collections.Generic;
	using Core;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.AI;
	using UnityEngine.Rendering;

	internal static class SceneSettingsProcessor
	{
		private static Object lightmapSettings;
		private static SerializedObject lightmapSettingsSo;
		private static SerializedProperty lightmapParametersField;
		private static SerializedObject navMeshSettingsSo;
		private static SerializedProperty navMeshDataField;

		private static Object renderSettings;
		private static SerializedObject renderSettingsSo;
		private static SerializedProperty renderHaloField;
		private static SerializedProperty renderSpotField;

		public static void Process(List<TreeConjunction> conjunctions)
		{
			lightmapSettings = null;
			lightmapSettingsSo = null;
			lightmapParametersField = null;

			navMeshSettingsSo = null;
			navMeshDataField = null;

			renderSettings = null;
			renderSettingsSo = null;
			renderHaloField = null;
			renderSpotField = null;

			foreach (var conjunction in conjunctions)
			{
				var referencedAsset = conjunction.referencedAsset;
				var referencedAssetObjects = referencedAsset.GetAllAssetObjects();

				foreach (var candidateInstanceId in referencedAssetObjects)
				{
					var candidate = EditorUtility.InstanceIDToObject(candidateInstanceId);

					if (candidate is LightingDataAsset)
					{
						CheckLightingDataAsset(conjunction, candidateInstanceId);
					}
#if UNITY_2020_1_OR_NEWER
					else if (candidate is LightingSettings)
					{
						CheckLightingSettingsAsset(conjunction, candidateInstanceId);
					}
#endif
					else if (candidate is Material)
					{
						CheckSkybox(conjunction, candidateInstanceId);
					}
					else if (candidate is LightmapParameters)
					{
						CheckLightMapSettings(conjunction, candidateInstanceId);
					}
					else if (candidate is Cubemap)
					{
						CheckLightingCubemap(conjunction, candidateInstanceId);
					}
					else if (candidate is NavMeshData)
					{
						CheckNavMesh(conjunction, candidateInstanceId);
					}
					else if (candidate is Light)
					{
						CheckSun(conjunction, candidateInstanceId);
					}
					else if (candidate is Texture2D)
					{
						CheckRenderSettingsTexture(conjunction, candidateInstanceId);
					}
				}
			}
		}

		private static void CheckNavMesh(TreeConjunction conjunction, int candidateInstanceId)
		{
			navMeshSettingsSo = navMeshSettingsSo ?? new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
			navMeshDataField = navMeshDataField ?? navMeshSettingsSo.FindProperty("m_NavMeshData");

			if (navMeshDataField != null && navMeshDataField.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (navMeshDataField.objectReferenceInstanceIDValue == candidateInstanceId)
				{
					var entry = new ReferencingEntryData
					{
						location = Location.SceneNavigationSettings,
						prefixLabel = "Navigation settings of scene"
					};

					conjunction.referencedAtInfo.AddNewEntry(entry);
				}
			}
			else
			{
				Debug.LogError(
					Maintainer.ConstructError("Can't find m_NavMeshData at the navMeshSettingsObject!"));
			}
		}

		private static void CheckLightingDataAsset(TreeConjunction conjunction, int candidateInstanceId)
		{
			if (Lightmapping.lightingDataAsset == null || Lightmapping.lightingDataAsset.GetInstanceID() != candidateInstanceId) return;

			var entry = new ReferencingEntryData
			{
				location = Location.SceneLightingSettings,
				prefixLabel = "Lighting settings (Global maps tab)"
			};

			conjunction.referencedAtInfo.AddNewEntry(entry);
		}
#if UNITY_2020_1_OR_NEWER
		private static void CheckLightingSettingsAsset(TreeConjunction conjunction, int candidateInstanceId)
		{
			if (Lightmapping.lightingSettings == null || Lightmapping.lightingSettings.GetInstanceID() != candidateInstanceId) return;

			var entry = new ReferencingEntryData
			{
				location = Location.SceneLightingSettings,
				prefixLabel = "Lighting settings (Scene tab)"
			};

			conjunction.referencedAtInfo.AddNewEntry(entry);
		}
#endif
		private static void CheckSkybox(TreeConjunction conjunction, int candidateInstanceId)
		{
			if (RenderSettings.skybox == null || RenderSettings.skybox.GetInstanceID() != candidateInstanceId) return;

			var entry = new ReferencingEntryData
			{
				location = Location.SceneLightingSettings,
				prefixLabel = "Lighting settings (Scene tab > Environment > Skybox Material)"
			};

			conjunction.referencedAtInfo.AddNewEntry(entry);
		}

		private static void CheckLightMapSettings(TreeConjunction conjunction, int candidateInstanceId)
		{
			lightmapSettings = lightmapSettings ? lightmapSettings : CSSettingsTools.GetInSceneLightmapSettings();

			if (lightmapSettings == null) return;

			lightmapSettingsSo = lightmapSettingsSo ?? new SerializedObject(lightmapSettings);
			lightmapParametersField = lightmapParametersField ??
									  lightmapSettingsSo.FindProperty(
										  "m_LightmapEditorSettings.m_LightmapParameters");
			if (lightmapParametersField != null && lightmapParametersField.propertyType ==
				SerializedPropertyType.ObjectReference)
			{
				if (lightmapParametersField.objectReferenceInstanceIDValue == candidateInstanceId)
				{
					var entry = new ReferencingEntryData
					{
						location = Location.SceneLightingSettings,
						prefixLabel =
							"Lighting settings (Scene tab > Lightmapping Settings > Lightmap Parameters)"
					};

					conjunction.referencedAtInfo.AddNewEntry(entry);
				}
			}
			else
			{
				Debug.LogError(
					Maintainer.ConstructError(
						"Can't find m_LightmapParameters at the LightmapSettings!"));
			}
		}

		private static void CheckLightingCubemap(TreeConjunction conjunction, int candidateInstanceId)
		{
			if (RenderSettings.customReflection == null || RenderSettings.customReflection.GetInstanceID() != candidateInstanceId) return;

			var entry = new ReferencingEntryData { location = Location.SceneLightingSettings };

			if (RenderSettings.defaultReflectionMode == DefaultReflectionMode.Custom)
			{
				entry.prefixLabel = "Lighting settings (Scene tab > Environment > Cubemap)";
			}
			else
			{
				entry.prefixLabel =
					"Lighting settings (Scene tab > Environment > Cubemap), set Reflections > Source > Custom to see";
			}

			conjunction.referencedAtInfo.AddNewEntry(entry);
		}

		private static void CheckSun(TreeConjunction conjunction, int candidateInstanceId)
		{
			if (RenderSettings.sun == null || RenderSettings.sun.GetInstanceID() != candidateInstanceId) return;

			var entry = new ReferencingEntryData
			{
				location = Location.SceneLightingSettings,
				prefixLabel = "Lighting settings (Scene tab > Environment > Sun Source)"
			};

			conjunction.referencedAtInfo.AddNewEntry(entry);
		}

		private static void CheckRenderSettingsTexture(TreeConjunction conjunction, int candidateInstanceId)
		{
			renderSettings = renderSettings ? renderSettings : CSSettingsTools.GetInSceneRenderSettings();
			if (renderSettings == null) return;

			renderSettingsSo = renderSettingsSo ?? new SerializedObject(renderSettings);
			renderHaloField = renderHaloField ?? renderSettingsSo.FindProperty("m_HaloTexture");

			if (renderHaloField != null && renderHaloField.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (renderHaloField.objectReferenceInstanceIDValue == candidateInstanceId)
				{
					var entry = new ReferencingEntryData
					{
						location = Location.SceneLightingSettings,
						prefixLabel = "Lighting settings (Scene tab > Other Settings > Halo Texture)"
					};

					conjunction.referencedAtInfo.AddNewEntry(entry);
				}
			}
			else
			{
				Debug.LogError(Maintainer.ConstructError("Can't find m_HaloTexture at the RenderSettings!"));
			}

			renderSpotField = renderSpotField ?? renderSettingsSo.FindProperty("m_SpotCookie");
			if (renderSpotField != null && renderSpotField.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (renderSpotField.objectReferenceInstanceIDValue == candidateInstanceId)
				{
					var entry = new ReferencingEntryData
					{
						location = Location.SceneLightingSettings,
						prefixLabel = "Lighting settings (Scene tab > Other Settings > Spot Cookie)"
					};

					conjunction.referencedAtInfo.AddNewEntry(entry);
				}
			}
			else
			{
				Debug.LogError(Maintainer.ConstructError("Can't find m_SpotCookie at the RenderSettings!"));
			}

			/*var iterator = renderSettingsSo.GetIterator();
			while (iterator.Next(true))
			{
				if (iterator.propertyType == SerializedPropertyType.ObjectReference)
				{
					Debug.Log(iterator.propertyPath + " [" + iterator.objectReferenceValue + "]");
				}
			}*/
		}
	}
}
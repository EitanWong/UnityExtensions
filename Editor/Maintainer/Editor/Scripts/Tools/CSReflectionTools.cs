#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using System;
	using System.Reflection;

	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	using Core;
	using UnityEngine.Events;
	using Object = UnityEngine.Object;

	internal static class CSReflectionTools
	{
		private static readonly Type EditorWindowType = typeof(EditorWindow);
		
		// assets
		public static readonly Type defaultAssetType = typeof(DefaultAsset);
		public static readonly Type lightingDataAsset = typeof(LightingDataAsset);
#if UNITY_2020_1_OR_NEWER
		public static readonly Type lightingSettings = typeof(LightingSettings);
#endif
		public static readonly Type sceneAssetType = typeof(SceneAsset);
		public static readonly Type textAssetType = typeof(TextAsset);
		public static readonly Type fontType = typeof(Font);
		public static readonly Type shaderType = typeof(Shader);
		public static readonly Type monoScriptType = typeof(MonoScript);
		public static readonly Type scriptableObjectType = typeof(ScriptableObject);
		public static readonly Type spriteAtlasType = typeof(UnityEngine.U2D.SpriteAtlas);
		public static readonly Type assemblyDefinitionAssetType = typeof(AssemblyDefinitionAsset);
#if UNITY_2019_2_OR_NEWER
		public static readonly Type assemblyDefinitionReferenceAssetType = typeof(AssemblyDefinitionReferenceAsset);
#endif

		// ui
		public static readonly Type splitterGUILayoutType = EditorWindowType.Assembly.GetType("UnityEditor.SplitterGUILayout");
		public static readonly Type splitterStateType = EditorWindowType.Assembly.GetType("UnityEditor.SplitterState");

		public static readonly Type buildPlayerWindowType = EditorWindowType.Assembly.GetType("UnityEditor.BuildPlayerWindow");
		public static readonly Type inspectorWindowType = EditorWindowType.Assembly.GetType("UnityEditor.InspectorWindow", false);

		// other
		public static readonly Type assetImporterType = typeof(AssetImporter);

/*#if UNITY_2018_2_OR_NEWER
		public static readonly Type addressableAssetGroupType = FindTypeInAssembly("Unity.Addressables.Editor", "UnityEditor.AddressableAssets.AddressableAssetGroup");
		private static Type FindTypeInAssembly(string assemblyName, string typeName)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (assembly != null)
				{
					if (assembly.GetName().Name == assemblyName)
					{
						var type = assembly.GetType(typeName);
						return type;
					}
				}
			}

			return null;
		}
#endif*/

		public static readonly Type settingsWindowType = EditorWindowType.Assembly.GetType("UnityEditor.SettingsWindow", false);
		public static readonly Type assetSettingsKindType = typeof(AssetSettingsKind);
		public static readonly Type monoBehaviourType = typeof(MonoBehaviour);
		public static readonly Type textureType = typeof(Texture);
		public static readonly Type texture2DType = typeof(Texture2D);
		public static readonly Type componentType = typeof(Component);
		public static readonly Type transformType = typeof(Transform);
		public static readonly Type gameObjectType = typeof(GameObject);
		public static readonly Type objectType = typeof(Object);

		// for caching
		private static PropertyInfo sortingLayersPropertyInfo;
		private static MethodInfo getLightmapSettingsMethodInfo;
		private static MethodInfo getRenderSettingsMethodInfo;
		private static MethodInfo getMainAssetInstanceIDMethodInfo;
		private static MethodInfo openProjectSettingsMethodInfo;
		private static MethodInfo beginVerticalSplitMethodInfo;
		private static MethodInfo endVerticalSplitMethodInfo;
		private static MethodInfo getDummyEventMethodInfo;

		private static Action<SerializedObject, InspectorMode> inspectorModeCachedSetter;

		public static void SetInspectorToDebug(SerializedObject serializedObject)
		{
			if (inspectorModeCachedSetter == null)
			{
				var pi = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

				if (pi != null)
				{
					var mi = pi.GetSetMethod(true);
					if (mi != null)
					{
						inspectorModeCachedSetter = (Action<SerializedObject, InspectorMode>)Delegate.CreateDelegate(typeof(Action<SerializedObject, InspectorMode>), mi);
					}
					else
					{
						Debug.LogError(Maintainer.ConstructError("Can't get the setter for the SerializedObject.inspectorMode property!"));
						return;
					}
				}
				else
				{
					Debug.LogError(Maintainer.ConstructError("Can't get the SerializedObject.inspectorMode property!"));
					return;
				}
			}

			inspectorModeCachedSetter.Invoke(serializedObject, InspectorMode.Debug);
		}

		public static void BeginVerticalSplit(object spl, GUILayoutOption[] guiLayoutOptions)
		{
			if (beginVerticalSplitMethodInfo == null)
			{
				beginVerticalSplitMethodInfo = splitterGUILayoutType.GetMethod("BeginVerticalSplit", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Standard, new []{splitterStateType, typeof(GUILayoutOption[])}, null);
			}

			if (beginVerticalSplitMethodInfo == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get SplitterGUILayout.BeginVerticalSplit method!"));
				return;
			}

			beginVerticalSplitMethodInfo.Invoke(null, new []{spl, guiLayoutOptions});
		}

		public static void EndVerticalSplit()
		{
			if (endVerticalSplitMethodInfo == null)
			{
				endVerticalSplitMethodInfo = splitterGUILayoutType.GetMethod("EndVerticalSplit", BindingFlags.Static | BindingFlags.Public);
			}

			if (endVerticalSplitMethodInfo == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get SplitterGUILayout.EndVerticalSplit method!"));
				return;
			}

			endVerticalSplitMethodInfo.Invoke(null, null);
		}

		public static PropertyInfo GetSortingLayersPropertyInfo()
		{
			if (sortingLayersPropertyInfo == null)
			{
				sortingLayersPropertyInfo = typeof(InternalEditorUtility).GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			}

			return sortingLayersPropertyInfo;
		}

		public static MethodInfo GetGetLightmapSettingsMethodInfo()
		{
			if (getLightmapSettingsMethodInfo == null)
			{
				getLightmapSettingsMethodInfo = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getLightmapSettingsMethodInfo;
		}

		public static MethodInfo GetGetRenderSettingsMethodInfo()
		{
			if (getRenderSettingsMethodInfo == null)
			{
				getRenderSettingsMethodInfo = typeof(RenderSettings).GetMethod("GetRenderSettings", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getRenderSettingsMethodInfo;
		}

		public static MethodInfo GetOpenProjectSettingsMethodInfo()
		{
			if (openProjectSettingsMethodInfo == null)
			{
				openProjectSettingsMethodInfo = settingsWindowType.GetMethod("OpenProjectSettings", BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, new Type[0], null);
			}

			return openProjectSettingsMethodInfo;
		}

		public static MethodInfo GetGetMainAssetInstanceIDMethodInfo()
		{
			if (getMainAssetInstanceIDMethodInfo == null)
			{
				getMainAssetInstanceIDMethodInfo = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getMainAssetInstanceIDMethodInfo;
		}

		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			PropertyInfo propInfo;
			do
			{
				propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				type = type.BaseType;
			}
			while (propInfo == null && type != null);

			return propInfo;
		}

		public static UnityEventBase GetDummyEvent(SerializedProperty property)
		{
			if (getDummyEventMethodInfo == null)
			{
				getDummyEventMethodInfo = typeof(UnityEventDrawer).GetMethod("GetDummyEvent", BindingFlags.NonPublic | BindingFlags.Static);
			}

			if (getDummyEventMethodInfo == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get UnityEventDrawer.GetDummyEvent method!"));
				return null;
			}

			return (UnityEventBase)getDummyEventMethodInfo.Invoke(null, new []{property});
		}
	}
}
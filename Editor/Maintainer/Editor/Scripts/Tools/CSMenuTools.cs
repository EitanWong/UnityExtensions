#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	using UnityEditor;

	internal static class CSMenuTools
	{

		public static bool ShowEditorSettings()
		{
			return CallUnifiedSettings("Editor");
		}

		public static bool ShowProjectSettingsGraphics()
		{
			return CallUnifiedSettings("Graphics");
		}

		public static bool ShowProjectSettingsPhysics()
		{
			return CallUnifiedSettings("Physics");
		}

		public static bool ShowProjectSettingsPhysics2D()
		{
			return CallUnifiedSettings("Physics 2D");
		}

		public static bool ShowProjectSettingsPresetManager()
		{
			return CallUnifiedSettings("Preset Manager");
		}

		public static bool ShowProjectSettingsPlayer()
		{
			return CallUnifiedSettings("Player");
		}

		public static bool ShowProjectSettingsTagsAndLayers()
		{
			return CallUnifiedSettings("Tags and Layers");
		}

		public static bool ShowProjectSettingsVFX()
		{
			return CallUnifiedSettings("VFX");
		}

		public static bool ShowSceneSettingsLighting()
		{
#if UNITY_2020_1_OR_NEWER
			return EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting");
#else
			return EditorApplication.ExecuteMenuItem("Window/Rendering/Lighting Settings");
#endif
			
		}

		public static bool ShowSceneSettingsNavigation()
		{
			return EditorApplication.ExecuteMenuItem("Window/AI/Navigation");
		}

		private static bool CallUnifiedSettings(string providerName)
		{
			SettingsService.OpenProjectSettings("Project/" + providerName);
			return true;
		}
		
		public static bool ShowEditorBuildSettings()
		{
			return (EditorWindow.GetWindow(CSReflectionTools.buildPlayerWindowType, true) != null);
		}
	}
}
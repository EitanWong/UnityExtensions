using System;
using UnityEngine;
using UnityEditor;

namespace NightOwl
{
	class UserPreferences : SettingsProvider
	{
		public enum AutoThemeMode
		{
			System,
			Time
		}

		internal class Styles
		{
			public static readonly GUIContent enabled = EditorGUIUtility.TrTextContent("Enabled");
			public static readonly GUIContent mode = EditorGUIUtility.TrTextContent("Change theme based on");
		}


		public static readonly bool DEFAULT_IS_ENABLED = true;
		public static readonly AutoThemeMode DEFAULT_MODE = AutoThemeMode.System;
		public static readonly TimeSpan DEFAULT_LIGHT_THEME_TIME = new TimeSpan(07, 00, 00);
		public static readonly TimeSpan DEFAULT_DARK_THEME_TIME = new TimeSpan(18, 00, 00);

		private static readonly string KEY_EDITORPREF_IS_ENABLED = "AutoDarkTheme.IsEnabled";
		private static readonly string KEY_EDITORPREF_MODE = "AutoDarkTheme.Mode";
		private static readonly string KEY_EDITORPREF_LIGHT_THEME_TIME = "AutoDarkTheme.LightThemeTime";
		private static readonly string KEY_EDITORPREF_DARK_THEME_TIME = "AutoDarkTheme.DarkThemeTime";

		private static bool s_prefsLoaded = false;
		private static bool s_isEnabled;
		private static AutoThemeMode s_mode;
		private static TimeSpan s_lightThemeTime;
		private static TimeSpan s_darkThemeTime;

		private static string gui_lightThemeTime;
		private static string gui_darkThemeTime;


		public static bool IsEnabled
		{
			get
			{
				LoadAllPreferenceValues();
				return s_isEnabled;
			}
			set
			{
				s_isEnabled = value;
				SaveAllPreferenceValues();
			}
		}

		public static AutoThemeMode Mode
		{
			get
			{
				LoadAllPreferenceValues();
				return s_mode;
			}
			set
			{
				s_mode = value;
				SaveAllPreferenceValues();
			}
		}

		public static TimeSpan LightThemeTime
        {
			get
            {
				LoadAllPreferenceValues();
				return s_lightThemeTime;
            }
			set
            {
				s_lightThemeTime = value;
				SaveAllPreferenceValues();
            }
        }

		public static TimeSpan DarkThemeTime
		{
			get
			{
				LoadAllPreferenceValues();
				return s_darkThemeTime;
			}
			set
			{
				s_darkThemeTime = value;
				SaveAllPreferenceValues();
			}
		}


		public static event EventHandler PreferencesChanged;
		private static void RaisePreferencesChanged()
        {
			var handler = PreferencesChanged;
			handler?.Invoke(typeof(UserPreferences), EventArgs.Empty);
        }


		private static void LoadAllPreferenceValues()
		{
			if (!s_prefsLoaded)
			{
				s_isEnabled = EditorPrefs.GetBool(KEY_EDITORPREF_IS_ENABLED, DEFAULT_IS_ENABLED);
				s_mode = (AutoThemeMode)EditorPrefs.GetInt(KEY_EDITORPREF_MODE, (int)DEFAULT_MODE);
				s_lightThemeTime = TimeSpan.Parse(EditorPrefs.GetString(KEY_EDITORPREF_LIGHT_THEME_TIME, DEFAULT_LIGHT_THEME_TIME.ToString()));
				s_darkThemeTime = TimeSpan.Parse(EditorPrefs.GetString(KEY_EDITORPREF_DARK_THEME_TIME, DEFAULT_DARK_THEME_TIME.ToString()));

				gui_lightThemeTime = s_lightThemeTime.ToString();
				gui_darkThemeTime = s_darkThemeTime.ToString();

				s_prefsLoaded = true;
			}
		}

		private static void SaveAllPreferenceValues()
		{
			EditorPrefs.SetBool(KEY_EDITORPREF_IS_ENABLED, s_isEnabled);
			EditorPrefs.SetInt(KEY_EDITORPREF_MODE, (int)s_mode);
			EditorPrefs.SetString(KEY_EDITORPREF_LIGHT_THEME_TIME, s_lightThemeTime.ToString());
			EditorPrefs.SetString(KEY_EDITORPREF_DARK_THEME_TIME, s_darkThemeTime.ToString());

			RaisePreferencesChanged();
		}

		public UserPreferences(string path, SettingsScope scope = SettingsScope.User)
			: base(path, scope)
		{
			LoadAllPreferenceValues();
		}

		public override void OnGUI(string searchContext)
		{
			bool areValuesValid = true;

			s_isEnabled = EditorGUILayout.Toggle("启用", s_isEnabled);

			if (s_isEnabled)
            {
				s_mode = (AutoThemeMode)EditorGUILayout.EnumPopup("模式", s_mode);

				if (s_mode == AutoThemeMode.Time)
				{
					gui_lightThemeTime = EditorGUILayout.DelayedTextField("Enable light theme at", gui_lightThemeTime);
					gui_darkThemeTime = EditorGUILayout.DelayedTextField("Enable dark theme at", gui_darkThemeTime);

					EditorGUILayout.Space();

					try
					{
						s_lightThemeTime = TimeSpan.Parse(gui_lightThemeTime);
					}
					catch (Exception e) when (e is FormatException || e is OverflowException)
					{
						areValuesValid = false;
						EditorGUILayout.HelpBox($"Invalid light theme time! Must be hh:mm:ss (24-hour format).", MessageType.Error);
					}

					try
					{
						s_darkThemeTime = TimeSpan.Parse(gui_darkThemeTime);
					}
					catch (Exception e) when (e is FormatException || e is OverflowException)
					{
						areValuesValid = false;
						EditorGUILayout.HelpBox($"Invalid dark theme time! Must be hh:mm:ss (24-hour format).", MessageType.Error);
					}
				}
            }

            if (GUI.changed && areValuesValid)
			{ 
				SaveAllPreferenceValues();            
            }
        }

        [SettingsProvider]
		public static SettingsProvider CreateAutoDarkThemeUserPreference()
		{
			var provider = new UserPreferences("Preferences/UnityExtensions/NightOwl")
			{
				keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
			};

			// Automatically extract all keywords from the Styles.
			return provider;
		}
	}

}
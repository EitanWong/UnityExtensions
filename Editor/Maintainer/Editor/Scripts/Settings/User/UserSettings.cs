#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;
	using System.IO;
	using Tools;
	using UI;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// All user-specific settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// Make sure to call Save() after changing settings to make sure changes will persist.
	/// </summary>
	[Serializable]
	public class UserSettings : ScriptableObject
	{
#if UNITY_2020_1_OR_NEWER
		private const string Directory = "UserSettings";
#else
		private const string Directory = "Library";
#endif
		private const string Path = Directory + "/MaintainerPersonalSettings.asset";
		private static UserSettings instance;

		[SerializeField]
		private IssuesFinderPersonalSettings issuesFinderSettings;
		[FormerlySerializedAs("referencesFinderSettings")]
		public ReferencesFinderPersonalSettings referencesFinder;

		[SerializeField]
		private ProjectCleanerPersonalSettings projectCleanerSettings;

		[SerializeField]
		private ReferencesFinderPersonalSettings referencesFinderSettings;

		[SerializeField]
		internal MaintainerWindow.MaintainerTab selectedTab = MaintainerWindow.MaintainerTab.Issues;

		[SerializeField]
		internal Vector2 scroll;

		[SerializeField]
		private string version = Maintainer.Version; // for backward compatibility in future versions

		internal static UserSettings Instance
		{
			get
			{
				if (instance != null) return instance;
				instance = LoadOrCreate();
				return instance;
			}
		}

		/// <summary>
		/// Issues Finder module settings.
		/// </summary>
		public static IssuesFinderPersonalSettings Issues
		{
			get
			{
				if (Instance.issuesFinderSettings == null)
				{
					Instance.issuesFinderSettings = new IssuesFinderPersonalSettings();
				}
				return Instance.issuesFinderSettings;
			}
		}

		/// <summary>
		/// Project Cleaner module settings.
		/// </summary>
		public static ProjectCleanerPersonalSettings Cleaner
		{
			get
			{
				if (Instance.projectCleanerSettings == null)
				{
					Instance.projectCleanerSettings = new ProjectCleanerPersonalSettings();
				}
				return Instance.projectCleanerSettings;
			}
		}

		/// <summary>
		/// References Finder module settings.
		/// </summary>
		public static ReferencesFinderPersonalSettings References
		{
			get
			{
				if (Instance.referencesFinder == null)
				{
					Instance.referencesFinder = new ReferencesFinderPersonalSettings();
				}
				return Instance.referencesFinder;
			}
		}

		/// <summary>
		/// Call to remove all personal settings.
		/// </summary>
		public static void Delete()
		{
			instance = null;
			CSFileTools.DeleteFile(Path);
		}

		/// <summary>
		/// Call to save any changes in personal settings.
		/// </summary>
		public static void Save()
		{
			SaveInstance(Instance);
		}

		private static UserSettings LoadOrCreate()
		{
			UserSettings settings;

			if (!File.Exists(Path))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = LoadInstance();

				if (settings == null)
				{
					CSFileTools.DeleteFile(Path);
					settings = CreateNewSettingsFile();
				}
			}

			settings.version = Maintainer.Version;

			return settings;
		}

		private static UserSettings CreateNewSettingsFile()
		{
			var settingsInstance = CreateInstance();

			SaveInstance(settingsInstance);

			return settingsInstance;
		}

		private static void SaveInstance(UserSettings settingsInstance)
		{
			if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);

			try
			{
				UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { settingsInstance }, Path, true);
			}
			catch (Exception ex)
			{
				Debug.LogError(Maintainer.ConstructError("Can't save personal settings!\n" + ex));
			}
		}

		private static UserSettings LoadInstance()
		{
			UserSettings settingsInstance;

			try
			{
				settingsInstance = (UserSettings)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(Path)[0];
			}
			catch (Exception ex)
			{
				Debug.Log(Maintainer.LogPrefix + "Can't read personal settings, resetting them to defaults.\nThis is a harmless message in most cases and can be ignored.\n" + ex);
				settingsInstance = null;
			}

			return settingsInstance;
		}

		private static UserSettings CreateInstance()
		{
			var newInstance = CreateInstance<UserSettings>();
			newInstance.issuesFinderSettings = new IssuesFinderPersonalSettings();
			newInstance.projectCleanerSettings = new ProjectCleanerPersonalSettings();
			newInstance.referencesFinder = new ReferencesFinderPersonalSettings();
			return newInstance;
		}
	}
}
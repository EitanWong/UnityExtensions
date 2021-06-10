#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEngine;
	using Settings;

	internal abstract class FiltersWindow : EditorWindow
	{
		internal delegate void TabChangeCallback(int newTab);

		private static bool needToRepaint;

		private event TabChangeCallback TabChangedCallback;

		private TabBase[] tabs;
		private GUIContent[] tabsCaptions;
        private TabBase currentTab;
        private int currentTabIndex;

		private Event currentEvent;
		private EventType currentEventType;

		protected void Init(string caption, TabBase[] windowTabs, int initialTab, TabChangeCallback tabChangeCallback)
		{
			titleContent = new GUIContent(caption + " Filters");

			minSize = new Vector2(600f, 300f);

			TabChangedCallback = tabChangeCallback;

			if (windowTabs != null && windowTabs.Length > 0)
			{
				tabs = windowTabs;

				currentTabIndex = windowTabs.Length > initialTab ? initialTab : 0;

				currentTab = windowTabs[currentTabIndex];
				currentTab.Show(this);

				var captions = new GUIContent[windowTabs.Length];

				for (var i = 0; i < windowTabs.Length; i++)
				{
					captions[i] = windowTabs[i].caption;
				}

				tabsCaptions = captions;
			}
			else
			{
				Debug.LogError(Maintainer.LogPrefix + "no tabs were passed to the Filters Window!");
			}
		}

		protected abstract void InitOnEnable();
		protected abstract void UnInitOnDisable();

		protected virtual void OnGUI()
		{
			UIHelpers.SetupStyles();

			currentEvent = Event.current;
			currentEventType = currentEvent.type;

			EditorGUI.BeginChangeCheck();
			{
				EditorGUI.BeginChangeCheck();
				currentTabIndex = GUILayout.Toolbar(currentTabIndex, tabsCaptions, UIHelpers.richButton, GUILayout.Height(21));
				if (EditorGUI.EndChangeCheck())
				{
					RemoveNotification();
				}
				currentTab = tabs[currentTabIndex];
			}
			if (EditorGUI.EndChangeCheck())
			{
				currentTab.Show(this);
				if (TabChangedCallback != null)
				{
					TabChangedCallback.Invoke(currentTabIndex);
				}
			}

			currentTab.currentEvent = currentEvent;
			currentTab.currentEventType = currentEventType;

			currentTab.ProcessDrags();
			currentTab.Draw();
		}

		[DidReloadScripts]
		private static void OnScriptsRecompiled()
		{
			needToRepaint = true;
		}

		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			InitOnEnable();
		}

		private void OnDisable()
		{
			ProjectSettings.Save();
			UnInitOnDisable();
		}

		private void OnInspectorUpdate()
		{
			if (needToRepaint)
			{
				needToRepaint = false;
				Repaint();
			}
		}
	}
}
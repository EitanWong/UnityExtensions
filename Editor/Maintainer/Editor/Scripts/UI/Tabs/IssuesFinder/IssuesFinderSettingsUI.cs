#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using Filters;
	using Settings;
	using UnityEditor;
	using UnityEngine;

	internal static class IssuesFinderSettingsUI
	{
		public static void Draw(ref Vector2 settingsSectionScrollPosition)
		{
			// -----------------------------------------------------------------------------
			// filtering settings
			// -----------------------------------------------------------------------------

			DrawWhereSection();

			GUILayout.Space(5);
			DrawWhatSection(ref settingsSectionScrollPosition);
			GUILayout.Space(10);

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				if (UIHelpers.ImageButton("Reset Settings", "Resets settings to defaults.", CSIcons.Restore))
				{
					ProjectSettings.Issues.Reset();
					UserSettings.Issues.Reset();
				}
				GUILayout.Space(10);
			}
			GUILayout.Space(10);
		}

		private static void DrawWhereSection(/*ref Vector2 settingsSectionScrollPosition*/)
		{
			// -----------------------------------------------------------------------------
			// where to look
			// -----------------------------------------------------------------------------

			using (new GUILayout.VerticalScope(/*UIHelpers.panelWithBackground*/))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);
					using (new GUILayout.VerticalScope())
					{
						GUILayout.Label("<b><size=16>Where</size></b>", UIHelpers.richLabel);
						UIHelpers.Separator();
					}
					GUILayout.Space(10);
				}

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);

					using (new GUILayout.VerticalScope())
					{
						GUILayout.Space(10);

						if (UIHelpers.ImageButton("Filters (" + ProjectSettings.Issues.GetFiltersCount() + ")", CSIcons.Filter))
						{
							IssuesFiltersWindow.Create();
						}

						GUILayout.Space(10);

						using (new GUILayout.HorizontalScope())
						{
							ProjectSettings.Issues.lookInScenes = EditorGUILayout.ToggleLeft(new GUIContent("Scenes",
									"Uncheck to exclude all scenes from search or select filtering level:\n\n" +
									"All Scenes: all project scenes with respect to configured filters.\n" +
									"Included Scenes: scenes included via Manage Filters > Scene Includes.\n" +
									"Current Scene: currently opened scene including any additional loaded scenes."),
								ProjectSettings.Issues.lookInScenes, GUILayout.Width(70));
							GUI.enabled = ProjectSettings.Issues.lookInScenes;
							ProjectSettings.Issues.scenesSelection = (IssuesFinderSettings.ScenesSelection)EditorGUILayout.EnumPopup(ProjectSettings.Issues.scenesSelection);
							GUI.enabled = true;
						}

						ProjectSettings.Issues.lookInAssets = EditorGUILayout.ToggleLeft(new GUIContent("File assets", "Uncheck to exclude all file assets like prefabs, ScriptableObjects and such from the search. Check readme for additional details."), ProjectSettings.Issues.lookInAssets);
						ProjectSettings.Issues.lookInProjectSettings = EditorGUILayout.ToggleLeft(new GUIContent("Project Settings", "Uncheck to exclude all file assets like prefabs, ScriptableObjects and such from the search. Check readme for additional details."), ProjectSettings.Issues.lookInProjectSettings);

						UIHelpers.Separator(5);

						var canScanGamObjects = ProjectSettings.Issues.lookInScenes || ProjectSettings.Issues.lookInAssets;
						GUI.enabled = canScanGamObjects;
						var scanGameObjects = UIHelpers.ToggleFoldout(ref ProjectSettings.Issues.scanGameObjects, ref UserSettings.Issues.scanGameObjectsFoldout, new GUIContent("Game Objects", "Specify if you wish to look for GameObjects issues."), GUILayout.Width(110));
						GUI.enabled = scanGameObjects && canScanGamObjects;
						if (UserSettings.Issues.scanGameObjectsFoldout)
						{
							UIHelpers.IndentLevel();
							ProjectSettings.Issues.touchInactiveGameObjects = EditorGUILayout.ToggleLeft(new GUIContent("Inactive Game Objects", "Uncheck to exclude all inactive Game Objects from the search."), ProjectSettings.Issues.touchInactiveGameObjects);
							ProjectSettings.Issues.touchDisabledComponents = EditorGUILayout.ToggleLeft(new GUIContent("Disabled Components", "Uncheck to exclude all disabled Components from the search."), ProjectSettings.Issues.touchDisabledComponents);
							UIHelpers.UnIndentLevel();
						}

						GUI.enabled = true;
					}

					GUILayout.Space(10);
				}

				GUILayout.Space(10);
			}
		}

		private static void DrawWhatSection(ref Vector2 settingsSectionScrollPosition)
		{
			// -----------------------------------------------------------------------------
			// what to look for
			// -----------------------------------------------------------------------------

			using (new GUILayout.VerticalScope(/*UIHelpers.panelWithBackground*/))
			{
				DrawSettingsSearchSectionHeader(SettingsSearchSection.All, "<b><size=16>What</size></b>");
				settingsSectionScrollPosition = GUILayout.BeginScrollView(settingsSectionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);

					using (new GUILayout.VerticalScope())
					{
						GUILayout.Space(10);

						// -----------------------------------------------------------------------------
						// Common Issues
						// -----------------------------------------------------------------------------

						ProjectSettings.Issues.missingReferences = EditorGUILayout.ToggleLeft(new GUIContent("Missing references", "Search for any missing references in Components, Project Settings, Scriptable Objects, and so on."), ProjectSettings.Issues.missingReferences);

#if UNITY_2019_1_OR_NEWER
						ProjectSettings.Issues.shadersWithErrors = EditorGUILayout.ToggleLeft(new GUIContent("Shaders errors", "Search for shaders with compilation errors."), ProjectSettings.Issues.shadersWithErrors);
#endif
						UIHelpers.Separator(5);

						// -----------------------------------------------------------------------------
						// Game Object Issues
						// -----------------------------------------------------------------------------

						using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
						{
							UIHelpers.Foldout(ref UserSettings.Issues.gameObjectsFoldout, "<b>Game Object Issues</b>");
						}

						if (UserSettings.Issues.gameObjectsFoldout)
						{
							GUILayout.Space(-2);
							UIHelpers.IndentLevel();
							if (DrawSettingsFoldoutSectionHeader(SettingsSearchSection.Common, ref UserSettings.Issues.commonFoldout))
							{
								UIHelpers.IndentLevel();
								ProjectSettings.Issues.missingComponents = EditorGUILayout.ToggleLeft(new GUIContent("Missing components", "Search for the missing components on the Game Objects."), ProjectSettings.Issues.missingComponents);
								ProjectSettings.Issues.missingPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Missing prefabs", "Search for instances of prefabs which were removed from project."), ProjectSettings.Issues.missingPrefabs);
								ProjectSettings.Issues.duplicateComponents = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate components", "Search for the multiple instances of the same component with same values on the same object."), ProjectSettings.Issues.duplicateComponents);
								ProjectSettings.Issues.inconsistentTerrainData = EditorGUILayout.ToggleLeft(new GUIContent("Inconsistent Terrain Data", "Search for Game Objects where Terrain and TerrainCollider have different Terrain Data."), ProjectSettings.Issues.inconsistentTerrainData);
								UIHelpers.UnIndentLevel();
							}

							if (DrawSettingsFoldoutSectionHeader(SettingsSearchSection.Neatness, ref UserSettings.Issues.neatnessFoldout))
							{
								UIHelpers.IndentLevel();
								ProjectSettings.Issues.unnamedLayers = EditorGUILayout.ToggleLeft(new GUIContent("Objects with unnamed layers", "Search for GameObjects with unnamed layers."), ProjectSettings.Issues.unnamedLayers);
								ProjectSettings.Issues.hugePositions = EditorGUILayout.ToggleLeft(new GUIContent("Objects with huge positions", "Search for GameObjects with huge world positions (> |100 000| on any axis)."), ProjectSettings.Issues.hugePositions);
								UIHelpers.UnIndentLevel();
							}
							UIHelpers.UnIndentLevel();
							GUILayout.Space(5);
						}

						GUI.enabled = true;

						// -----------------------------------------------------------------------------
						// Project Settings Issues
						// -----------------------------------------------------------------------------

						using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
						{
							UIHelpers.Foldout(ref UserSettings.Issues.projectSettingsFoldout, "<b>Project Settings Issues</b>");
						}

						if (UserSettings.Issues.projectSettingsFoldout)
						{
							UIHelpers.IndentLevel();
							ProjectSettings.Issues.duplicateLayers = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate Layers", "Search for the duplicate layers and sorting layers at the 'Tags and Layers' Project Settings."), ProjectSettings.Issues.duplicateLayers);
							UIHelpers.UnIndentLevel();
						}
						GUI.enabled = true;

						GUILayout.Space(10);
					}
					GUILayout.Space(10);
				}
				GUILayout.EndScrollView();
			}
		}

		private static bool DrawSettingsFoldoutSectionHeader(SettingsSearchSection section, ref bool foldout)
		{
			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope())
			{
				foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(true, GUILayout.Width(100)), foldout, ObjectNames.NicifyVariableName(section.ToString()), true, UIHelpers.richFoldout);
				GUILayout.FlexibleSpace();

				if (UIHelpers.IconButton(CSIcons.SelectAll, "Select all"))
				{
					SettingsSectionGroupSwitch(section, true);
				}

				if (UIHelpers.IconButton(CSIcons.SelectNone, "Clear selection"))
				{
					SettingsSectionGroupSwitch(section, false);
				}

				GUILayout.Space(5);
			}

			/*if (foldout)
			{
				UIHelpers.Separator();
			}*/

			return foldout;
		}

		private static void DrawSettingsSearchSectionHeader(SettingsSearchSection section, string caption)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);

				GUILayout.Label(caption, UIHelpers.richLabel, GUILayout.Width(100));

				GUILayout.FlexibleSpace();
				using (new GUILayout.VerticalScope())
				{
					//GUILayout.Space(-3);
					using (new GUILayout.HorizontalScope())
					{
						if (UIHelpers.IconButton(CSIcons.SelectAll, "Select all"))
						{
							SettingsSectionGroupSwitch(section, true);
						}

						if (UIHelpers.IconButton(CSIcons.SelectNone, "Clear selection"))
						{
							SettingsSectionGroupSwitch(section, false);
						}
					}
				}
				GUILayout.Space(10);
			}

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				UIHelpers.Separator();
				GUILayout.Space(10);
			}
		}

		private static void SettingsSectionGroupSwitch(SettingsSearchSection section, bool enable)
		{
			switch (section)
			{
				case SettingsSearchSection.Common:
					ProjectSettings.Issues.SwitchCommon(enable);
					break;
				case SettingsSearchSection.Neatness:
					ProjectSettings.Issues.SwitchNeatness(enable);
					break;
				case SettingsSearchSection.All:
					ProjectSettings.Issues.SwitchAll(enable);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
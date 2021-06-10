#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using Filters;
	using References;
	using Settings;
	using UnityEditor;
	using UnityEngine;

	internal class ProjectReferencesTab : ReferencesChildTab
	{
		public static string AutoSelectPath { get; set; }

		protected override string CaptionName
		{
			get { return "Project Assets"; }
		}

		protected override Texture CaptionIcon
		{
			get { return CSEditorIcons.ProjectViewIcon; }
		}

		private readonly ProjectReferencesTreePanel treePanel;

		public ProjectReferencesTab(MaintainerWindow window) : base(window)
		{
			treePanel = new ProjectReferencesTreePanel(window);
		}

		public void DrawLeftColumnHeader()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				using (new GUILayout.VerticalScope())
				{
					GUILayout.Space(10);
					GUILayout.Label("<size=16><b>Search references of:</b></size>", UIHelpers.richLabel);
					UIHelpers.Separator();
					GUILayout.Space(10);

					if (UIHelpers.ImageButton("All assets",
						"Traverses whole project to find where all assets are referenced.", CSIcons.Find))
					{
						if (Event.current.control && Event.current.shift)
						{
							ReferencesFinder.debugMode = true;
							AssetsMap.Delete();
							Event.current.Use();
						}
						else
						{
							ReferencesFinder.debugMode = false;
						}

						EditorApplication.delayCall += StartProjectReferencesScan;
					}

					if (ProjectScopeReferencesFinder.GetSelectedAssets().Length == 0)
					{
						GUI.enabled = false;
					}

					if (UIHelpers.ImageButton("Selected assets",
						"Adds selected Project View assets to the current search results.", CSIcons.Find))
					{
						EditorApplication.delayCall += () => ReferencesFinder.FindSelectedAssetsReferences();
					}
				}
				GUILayout.Space(10);
			}

			GUI.enabled = true;
		}

		public void DrawSettings()
		{
			GUILayout.Space(10);
			using (new GUILayout.HorizontalScope(/*UIHelpers.panelWithBackground*/))
			{
				GUILayout.Space(10);
				using (new GUILayout.VerticalScope())
				{
					GUILayout.Label("<size=16><b>Settings:</b></size>", UIHelpers.richLabel);
					UIHelpers.Separator();
					GUILayout.Space(10);

					if (UIHelpers.ImageButton("Filters (" +
											  (ProjectSettings.References.pathIgnoresFilters.Length +
											   ProjectSettings.References.pathIncludesFilters.Length) + ")",
						CSIcons.Filter))
					{
						ReferencesFiltersWindow.Create();
					}

					using (new GUILayout.VerticalScope())
					{
						GUILayout.Space(5);

						UserSettings.References.showAssetsWithoutReferences = GUILayout.Toggle(
							UserSettings.References.showAssetsWithoutReferences,
							new GUIContent("Show assets without references",
								"Check to see all scanned assets in the list even if there was no any references to the asset found in project."));

						UserSettings.References.selectedFindClearsProjectResults = GUILayout.Toggle(
							UserSettings.References.selectedFindClearsProjectResults,
							new GUIContent(@"Clear previous results",
								"Check to automatically clear last results on selected assets find both from context menu and main window.\n" +
								"Uncheck to add new results to the last results."));

						GUILayout.Space(3);
					}

					GUILayout.Space(10);
				}

				GUILayout.Space(10);
			}
		}

		public void Refresh(bool newData)
		{
			treePanel.Refresh(newData);

			if (newData)
			{
				if (!string.IsNullOrEmpty(AutoSelectPath))
				{
					EditorApplication.delayCall += () =>
					{
						SelectItemWithPath(AutoSelectPath);
						AutoSelectPath = null;
					};
				}
			}
		}

		public void DrawRightColumn()
		{
			treePanel.Draw();
		}

		public void DrawFooter()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);

				if (SearchResultsStorage.ProjectReferencesLastSearched.Length == 0)
				{
					GUI.enabled = false;
				}

				if (UIHelpers.ImageButton("Refresh", "Restarts references search for the previous results.",
					CSIcons.Repeat))
				{
					if (Event.current.control && Event.current.shift)
					{
						ReferencesFinder.debugMode = true;
						AssetsMap.Delete();
						Event.current.Use();
					}
					else
					{
						ReferencesFinder.debugMode = false;
					}

					EditorApplication.delayCall += () =>
					{
						ProjectScopeReferencesFinder.FindAssetsReferences(SearchResultsStorage.ProjectReferencesLastSearched, null);
					};
				}

				GUI.enabled = true;

				if (SearchResultsStorage.ProjectReferencesSearchResults.Length == 0)
				{
					GUI.enabled = false;
				}

				if (UIHelpers.ImageButton("Collapse all", "Collapses all tree items.", CSIcons.Collapse))
				{
					treePanel.CollapseAll();
				}

				if (UIHelpers.ImageButton("Expand all", "Expands all tree items.", CSIcons.Expand))
				{
					treePanel.ExpandAll();
				}

				if (UIHelpers.ImageButton("Clear results", "Clears results tree and empties cache.", CSIcons.Clear))
				{
					SearchResultsStorage.ProjectReferencesSearchResults = null;
					SearchResultsStorage.ProjectReferencesLastSearched = null;
					Refresh(true);
				}

				GUI.enabled = true;

				GUILayout.Space(10);
			}
			GUILayout.Space(10);
		}

		private void SelectItemWithPath(string autoSelectPath)
		{
			treePanel.SelectItemWithPath(autoSelectPath);
		}

		private void StartProjectReferencesScan()
		{
			window.RemoveNotification();
			ReferencesFinder.FindAllAssetsReferences();
			window.Focus();
		}
	}
}
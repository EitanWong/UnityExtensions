#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using References;
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEditor.Experimental.SceneManagement;

	internal class HierarchyReferencesTab : ReferencesChildTab
	{
		public static ReferencingEntryData AutoSelectHierarchyReference { get; set; }

		protected override string CaptionName
		{
			get { return "Hierarchy Objects"; }
		}

		protected override Texture CaptionIcon
		{
			get { return CSEditorIcons.HierarchyViewIcon; }
		}

		private readonly HierarchyReferencesTreePanel treePanel;

		public HierarchyReferencesTab(MaintainerWindow window):base(window)
		{
			treePanel = new HierarchyReferencesTreePanel();
		}

		public void DrawLeftColumnHeader()
		{
			var assetPath = SceneManager.GetActiveScene().path;
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
#if UNITY_2020_1_OR_NEWER
				assetPath = prefabStage.assetPath;
#else
				assetPath = prefabStage.prefabAssetPath;
#endif
			}
		}

		public void DrawSettings()
		{
			GUILayout.Space(10);

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				using (new GUILayout.VerticalScope())
				{
					EditorGUILayout.HelpBox("Hold 'alt' key while dropping Game Objects to skip their components", MessageType.Info);
					GUILayout.Space(10);

					GUILayout.Label("<size=16><b>Settings</b></size>", UIHelpers.richLabel);
					UIHelpers.Separator();
					GUILayout.Space(10);

					UserSettings.References.clearHierarchyResults = GUILayout.Toggle(
						UserSettings.References.clearHierarchyResults,
						new GUIContent(@"Clear previous results",
							"Check to automatically clear last results on any new search.\n" +
							"Uncheck to add new results to the last results."));

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
				if (AutoSelectHierarchyReference != null)
				{
					EditorApplication.delayCall += () =>
					{
						SelectRow(AutoSelectHierarchyReference);
						AutoSelectHierarchyReference = null;
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

				if (SearchResultsStorage.HierarchyReferencesLastSearched.Length == 0)
				{
					GUI.enabled = false;
				}

				if (UIHelpers.ImageButton("Refresh", "Restarts references search for the previous results.",
					CSIcons.Repeat))
				{
					if (Event.current.control && Event.current.shift)
					{
						ReferencesFinder.debugMode = true;
						Event.current.Use();
					}
					else
					{
						ReferencesFinder.debugMode = false;
					}

					EditorApplication.delayCall += () =>
					{
						var sceneObjects =
							CSObjectTools.GetObjectsFromInstanceIds(SearchResultsStorage.HierarchyReferencesLastSearched);
						HierarchyScopeReferencesFinder.FindHierarchyObjectsReferences(sceneObjects, null);
					};
				}

				GUI.enabled = true;

				if (SearchResultsStorage.HierarchyReferencesSearchResults.Length == 0)
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
					ClearResults();
				}

				GUI.enabled = true;

				GUILayout.Space(10);
			}

			GUILayout.Space(10);
		}

		private void ClearResults()
		{
			SearchResultsStorage.HierarchyReferencesSearchResults = null;
			SearchResultsStorage.HierarchyReferencesLastSearched = null;
			Refresh(true);
		}

		private void SelectRow(ReferencingEntryData reference)
		{
			treePanel.SelectRow(reference.objectId, reference.componentId);
		}
	}
}
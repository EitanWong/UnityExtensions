#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using Core;
	using References;
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	internal class ProjectReferencesTreePanel
	{
		private ProjectReferenceItem[] treeElements;
		private TreeModel<ProjectReferenceItem> treeModel;
		private ProjectReferencesTreeView<ProjectReferenceItem> treeView;
		private SearchField searchField;

		private readonly ProjectExactReferencesListPanel exactReferencesPanel;

		private object splitterState;

		public ProjectReferencesTreePanel(MaintainerWindow window)
		{
			exactReferencesPanel = new ProjectExactReferencesListPanel(window);
		}

		public void Refresh(bool newData)
		{
			if (newData)
			{
				UserSettings.References.projectReferencesTreeViewState = new TreeViewState();
				treeModel = null;
			}

			if (treeModel == null)
			{
				UpdateTreeModel();
			}

			exactReferencesPanel.Refresh(newData);
		}

		public void SelectItemWithPath(string path)
		{
			treeView.SelectRowWithPath(path);
		}

		private void UpdateTreeModel()
		{
			var firstInit = UserSettings.References.projectReferencesTreeHeaderState == null || UserSettings.References.projectReferencesTreeHeaderState.columns == null || UserSettings.References.projectReferencesTreeHeaderState.columns.Length == 0;
			var headerState = ProjectReferencesTreeView<ProjectReferenceItem>.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(UserSettings.References.projectReferencesTreeHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(UserSettings.References.projectReferencesTreeHeaderState, headerState);
			UserSettings.References.projectReferencesTreeHeaderState = headerState;

			var multiColumnHeader = new MaintainerMultiColumnHeader(headerState);

			if (firstInit)
			{
				UserSettings.References.projectReferencesTreeViewState = new TreeViewState();
			}

			treeElements = LoadLastTreeElements();
			treeModel = new TreeModel<ProjectReferenceItem>(treeElements);
			treeView = new ProjectReferencesTreeView<ProjectReferenceItem>(UserSettings.References.projectReferencesTreeViewState, multiColumnHeader, treeModel);
			treeView.SetSearchString(UserSettings.References.projectTabSearchString);
			treeView.Reload();

			searchField = new SearchField();
			searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

			if (firstInit)
			{
				multiColumnHeader.ResizeToFit();
			}
		}

		public void Draw()
		{
			GUILayout.Space(3);
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(5);
				using (new GUILayout.VerticalScope())
				{
					EditorGUI.BeginChangeCheck();
					var searchString =
						searchField.OnGUI(
							GUILayoutUtility.GetRect(0, 0, 20, 20, GUILayout.ExpandWidth(true),
								GUILayout.ExpandHeight(false)), UserSettings.References.projectTabSearchString);
					if (EditorGUI.EndChangeCheck())
					{
						UserSettings.References.projectTabSearchString = searchString;
						treeView.SetSearchString(searchString);
						treeView.Reload();
					}

					GUILayout.Space(3);

					GetSplitterState();

					CSReflectionTools.BeginVerticalSplit(splitterState, new GUILayoutOption[0]);

					using (new GUILayout.VerticalScope())
					{
						treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true),
							GUILayout.ExpandHeight(true)));
						GUILayout.Space(2f);
					}

					using (new GUILayout.VerticalScope())
					{
						GUILayout.Space(2f);

						using (new GUILayout.VerticalScope(UIHelpers.panelWithoutBackground))
						{
							GUILayout.Label("Exact references", UIHelpers.centeredLabel);
							GUILayout.Space(1f);
						}

						GUILayout.Space(-1f);

						var selected = treeView.GetSelection();
						if (selected != null && selected.Count > 0)
						{
							var selectedRow = treeView.GetRow(selected[0]);
							exactReferencesPanel.Draw(selectedRow);
						}
						else
						{
							exactReferencesPanel.Draw(null);
						}
					}

					CSReflectionTools.EndVerticalSplit();

					SaveSplitterState();
				}

				GUILayout.Space(5);
			}
		}

		private void GetSplitterState()
		{
			if (splitterState != null)
			{
				return;
			}

			var savedState = UserSettings.References.splitterState;
			object result;

			try
			{
				if (!string.IsNullOrEmpty(savedState))
				{
					result = JsonUtility.FromJson(savedState, CSReflectionTools.splitterStateType);
				}
				else
				{
					result = Activator.CreateInstance(CSReflectionTools.splitterStateType,
						new [] {100f, 50f},
						new [] {90, 47},
						null);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.ConstructError("Couldn't create instance of the SplitterState class!\n" + e, ReferencesFinder.ModuleName));
				throw e;
			}

			splitterState = result;
		}

		private void SaveSplitterState()
		{
			UserSettings.References.splitterState = EditorJsonUtility.ToJson(splitterState, false);
		}

		public void CollapseAll()
		{
			treeView.CollapseAll();
		}

		public void ExpandAll()
		{
			treeView.ExpandAll();
		}

		private ProjectReferenceItem[] LoadLastTreeElements()
		{
			var loaded = SearchResultsStorage.ProjectReferencesSearchResults;
			if (loaded == null || loaded.Length == 0)
			{
				loaded = new ProjectReferenceItem[1];
				loaded[0] = new ProjectReferenceItem { id = 0, depth = -1, name = "root" };
			}
			return loaded;
		}
	}
}
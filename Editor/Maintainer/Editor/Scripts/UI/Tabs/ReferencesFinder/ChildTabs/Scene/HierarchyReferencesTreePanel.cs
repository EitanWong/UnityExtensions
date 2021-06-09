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
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	internal class HierarchyReferencesTreePanel
	{
		private TreeModel<HierarchyReferenceItem> treeModel;
		private HierarchyReferencesTreeView<HierarchyReferenceItem> treeView;
		private SearchField searchField;

		private HierarchyReferenceItem[] treeElements;

		public void Refresh(bool newData)
		{
			if (newData)
			{
				UserSettings.References.hierarchyReferencesTreeViewState = new TreeViewState();
				treeModel = null;
			}

			if (treeModel == null)
			{
				UpdateTreeModel();
			}
		}

		public void SelectRow(long objectId, long componentId)
		{
			treeView.SelectRow(objectId, componentId);
		}

		private void UpdateTreeModel()
		{
			var savedHeaderState = UserSettings.References.sceneReferencesTreeHeaderState;
			var firstInit = savedHeaderState == null || savedHeaderState.columns == null || savedHeaderState.columns.Length == 0;
			var headerState = HierarchyReferencesTreeView<HierarchyReferenceItem>.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(savedHeaderState, headerState))
			{
				MultiColumnHeaderState.OverwriteSerializedFields(savedHeaderState, headerState);
			}
			UserSettings.References.sceneReferencesTreeHeaderState = headerState;

			var multiColumnHeader = new MaintainerMultiColumnHeader(headerState);

			if (firstInit)
			{
				UserSettings.References.hierarchyReferencesTreeViewState = new TreeViewState();
			}

			treeElements = LoadLastTreeElements();
			treeModel = new TreeModel<HierarchyReferenceItem>(treeElements);
			treeView = new HierarchyReferencesTreeView<HierarchyReferenceItem>(UserSettings.References.hierarchyReferencesTreeViewState, multiColumnHeader, treeModel);
			treeView.SetSearchString(UserSettings.References.sceneTabSearchString);
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
								GUILayout.ExpandHeight(false)), UserSettings.References.sceneTabSearchString);
					if (EditorGUI.EndChangeCheck())
					{
						UserSettings.References.sceneTabSearchString = searchString;
						treeView.SetSearchString(searchString);
						treeView.Reload();
					}

					GUILayout.Space(3);

					using (new GUILayout.VerticalScope())
					{
						treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true),
							GUILayout.ExpandHeight(true)));
						GUILayout.Space(2f);
					}
				}

				GUILayout.Space(5);
			}
		}

		public void CollapseAll()
		{
			treeView.CollapseAll();
		}

		public void ExpandAll()
		{
			treeView.ExpandAll();
		}

		private HierarchyReferenceItem[] LoadLastTreeElements()
		{
			var loaded = SearchResultsStorage.HierarchyReferencesSearchResults;
			if (loaded == null || loaded.Length == 0)
			{
				loaded = new HierarchyReferenceItem[1];
				loaded[0] = new HierarchyReferenceItem { id = 0, depth = -1, name = "root" };
			}
			return loaded;
		}
	}
}
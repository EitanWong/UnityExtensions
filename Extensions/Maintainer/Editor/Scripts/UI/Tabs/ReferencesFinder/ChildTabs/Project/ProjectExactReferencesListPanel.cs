#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using References;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	internal class ProjectExactReferencesListPanel
	{
		private HierarchyReferenceItem[] listElements;
		private TreeModel<HierarchyReferenceItem> listModel;
		private ExactReferencesList<HierarchyReferenceItem> list;

		private MaintainerTreeViewItem<ProjectReferenceItem> lastSelectedRow;

		internal ProjectExactReferencesListPanel(MaintainerWindow window)
		{
		}

		internal void Refresh(bool newData)
		{
			if (newData)
			{
				listModel = null;
			}

			if (listModel == null && lastSelectedRow != null)
			{
				UpdateTreeModel();
			}
		}

		internal virtual void Draw(MaintainerTreeViewItem<ProjectReferenceItem> selectedRow)
		{
				if (selectedRow == null)
				{
					DrawRow("Please select any child item above to look for exact references location.");
					return;
				}

				if (selectedRow.data == null)
				{
					DrawRow("Selected item has no exact references support.");
					return;
				}

				var entries = selectedRow.data.referencingEntries;

				if (entries == null || entries.Length == 0)
				{
					if (selectedRow.data.depth == 0)
					{
						DrawRow("Please select any child item above to look for exact references location.");
						return;
					}

					DrawRow("Selected item has no exact references support.");
					return;
				}

				if (lastSelectedRow != selectedRow)
				{
					lastSelectedRow = selectedRow;
					UpdateTreeModel();
				}

				DrawReferencesPanel();
		}

		private void DrawRow(string label)
		{
			lastSelectedRow = new ListTreeViewItem<ProjectReferenceItem>(0, 0, label, null)
			{
				depth = 0,
				id = 1
			};
			UpdateTreeModel();
			DrawReferencesPanel();
		}

		private void DrawReferencesPanel()
		{
			list.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
		}

		private void UpdateTreeModel()
		{
			listElements = GetTreeElementsFromRow(lastSelectedRow);
			listModel = new TreeModel<HierarchyReferenceItem>(listElements);
			list = new ExactReferencesList<HierarchyReferenceItem>(new TreeViewState(), listModel);
			list.Reload();
		}

		private HierarchyReferenceItem[] GetTreeElementsFromRow(MaintainerTreeViewItem<ProjectReferenceItem> item)
		{
			var data = item.data;
			var entries = data != null ? data.referencingEntries : null;

			int count;
			if (entries != null && entries.Length > 0)
			{
				count = entries.Length + 1;
			}
			else
			{
				count = 2;
			}

			var result = new HierarchyReferenceItem[count];
			result[0] = new HierarchyReferenceItem
			{
				id = 0,
				name = "root",
				depth = -1
			};

			if (entries == null || entries.Length == 0)
			{
				result[1] = new HierarchyReferenceItem
				{
					id = 1,
					reference = null,
					name = item.displayName
				};

				return result;
			}

			for (var i = 0; i < entries.Length; i++)
			{
				var entry = entries[i];
				var newItem = new HierarchyReferenceItem
				{
					id = i + 1,
					reference = entry
				};
				newItem.SetAssetPath(item.data.assetPath);
				result[i + 1] = newItem;
			}

			return result;
		}
	}
}
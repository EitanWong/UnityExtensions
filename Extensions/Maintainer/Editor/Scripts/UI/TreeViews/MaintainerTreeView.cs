#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Core;

	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEditor.VersionControl;
	using UnityEngine;

	internal abstract class MaintainerTreeViewItem<T> : TreeViewItem where T : TreeItem
	{
		public readonly T data;

		internal MaintainerTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
		{
			this.data = data;
			Init();
		}

		private void Init()
		{
			Initialize();
		}

		protected virtual void Initialize()
		{
		}
	}

	internal abstract class MaintainerTreeView<T> : TreeView where T : TreeItem
	{
		public event Action TreeChanged;

		protected const float RowHeight = 25f;
#if UNITY_2019_3_OR_NEWER
		protected const int IconWidth = 20;
#else
		protected const int IconWidth = 16;
#endif
		protected const int IconPadding = 7;

		protected readonly List<TreeViewItem> rows = new List<TreeViewItem>(100);

		private TreeModel<T> TreeModel { get; set; }
		private string newSearchString;

		protected MaintainerTreeView(TreeViewState state, TreeModel<T> model) : base(state)
		{
			Init(model);
		}

		protected MaintainerTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader)
		{
			multiColumnHeader.sortingChanged += OnSortingChanged;
			Init(model);
		}

		public void SetSearchString(string newSearch)
		{
			newSearchString = newSearch;
		}

		public MaintainerTreeViewItem<T> GetRow(int id)
		{
			foreach (var row in rows)
			{
				if (row.id == id)
				{
					return row as MaintainerTreeViewItem<T>;
				}
			}

			return null;
		}

		private void Init(TreeModel<T> model)
		{
			rowHeight = RowHeight;
			
#if !UNITY_2019_3_OR_NEWER
			customFoldoutYOffset = (RowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
#endif

			showBorder = true;
			showAlternatingRowBackgrounds = true;

			TreeModel = model;
			TreeModel.ModelChanged += ModelChanged;

			PostInit();
		}

		protected override TreeViewItem BuildRoot()
		{
			return GetNewTreeViewItemInstance(TreeModel.root.id, -1, TreeModel.root.name, TreeModel.root);
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			if (TreeModel.root == null)
			{
				Debug.LogError(Maintainer.ConstructError("tree model root is null. did you call SetData()?"));
				return rows;
			}

			rows.Clear();

			if (TreeModel.root.HasChildren)
			{
				AddChildrenRecursive(TreeModel.root, 0, rows);
			}
			SetupParentsAndChildrenFromDepths(root, rows);

			SortIfNeeded(root, rows);

			return rows;
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (MaintainerTreeViewItem<T>)args.item;

			for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				var rect = args.GetCellRect(i);
				DrawCell(ref rect, item, args.GetColumn(i), args);
			}
		}

		protected override void DoubleClickedItem(int id)
		{
			base.DoubleClickedItem(id);
			var item = FindItem(id, rootItem);
			ShowItem(item);
		}

		protected override IList<int> GetAncestors(int id)
		{
			return TreeModel.GetAncestors(id);
		}

		protected override void ContextClickedItem(int id)
		{
			if (!CanChangeExpandedState(FindItem(id, rootItem))) return;

			SetExpanded(id, !IsExpanded(id));
			Event.current.Use();
		}

		protected override IList<int> GetDescendantsThatHaveChildren(int id)
		{
			return TreeModel.GetDescendantsThatHaveChildren(id);
		}

		private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> result)
		{
			foreach (var treeElement in parent.Children)
			{
				var child = (T)treeElement;

				if (!string.IsNullOrEmpty(newSearchString) && !child.CanBeFoundWith(newSearchString) && depth == 0) continue;

				var item = GetNewTreeViewItemInstance(child.id, depth, child.name, child);
				result.Add(item);

				if (child.HasChildren)
				{
					if (IsExpanded(child.id))
					{
						AddChildrenRecursive(child, depth + 1, result);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rowsToSort)
		{
			if (rowsToSort.Count <= 1)
				return;

			if (multiColumnHeader == null || multiColumnHeader.sortedColumnIndex == -1)
				return;

			SortByMultipleColumns();
			TreeToList(root, rowsToSort);
			Repaint();
		}

		private void ModelChanged()
		{
			if (TreeChanged != null) TreeChanged();
			Reload();
		}

		protected virtual void OnSortingChanged(MultiColumnHeader header)
		{
			SortIfNeeded(rootItem, GetRows());
		}

		private static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();

			if (root.children == null)
				return;

			var stack = new Stack<TreeViewItem>();
			for (var i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (var i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		protected virtual void DrawCell(ref Rect cellRect, MaintainerTreeViewItem<T> genericItem, int columnValue, RowGUIArgs args)
		{
			baseIndent = genericItem.depth * GetDepthIndentation();
			CenterRectUsingSingleLineHeight(ref cellRect);
		}

		protected void SelectRowInternal(MaintainerTreeViewItem<T> rowLocal)
		{
			EditorApplication.delayCall += () =>
			{
				var id = rowLocal.id;
				SetExpanded(id, true);

				var childId = -1;
				if (rowLocal.data.HasChildren && rowLocal.data.Children.Count > 0)
				{
					var child = rowLocal.data.Children[0];
					childId = child.id;
				}

				FrameItem(childId > -1 ? childId : id);

				SetSelection(new List<int> {id});
				SetFocusAndEnsureSelectedItem();

				MaintainerWindow.RepaintInstance();
			};
		}

		protected Rect DrawIconAndGetEntryRect(Rect cellRect, TreeViewItem item, bool indentWithNoIcon = false)
		{
			var iconPadding = !Provider.isActive ? 0 : IconPadding;
			var entryRect = cellRect;

			var num = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
			entryRect.xMin += num;

			if (item.icon != null)
			{
				var iconRect = entryRect;
				iconRect.width = IconWidth;
				iconRect.x += iconPadding;
				iconRect.height = EditorGUIUtility.singleLineHeight;

				GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
			}

			if (item.icon != null || indentWithNoIcon)
			{
				// BASED ON DECOMPILED CODE
				// AssetsTreeViewGUI:
				// float num = (!Provider.isActive) ? 0f : 7f;
				// iconRightPadding = num;
				// iconLeftPadding = num;

				// TreeViewGUI:
				// iconTotalPadding = iconLeftPadding + iconRightPadding

				entryRect.xMin +=

					// TreeViewGUI: public float k_IconWidth = 16f;
					IconWidth +

					// TreeViewGUI: iconTotalPadding
					iconPadding * 2 +

					// TreeViewGUI: public float k_SpaceBetweenIconAndText = 2f;
					2f;
			}

			return entryRect;
		}

		protected abstract TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data);
		protected abstract void SortByMultipleColumns();

		protected virtual void PostInit()
		{
			columnIndexForTreeFoldouts = 0;
		}

		protected virtual int GetDepthIndentation()
		{
			return 10;
		}

		protected virtual void ShowItem(TreeViewItem item) {}
	}

	internal static class EnumerableExtensionMethods
	{
		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}

			return source.OrderByDescending(selector);
		}

		public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.ThenBy(selector);
			}

			return source.ThenByDescending(selector);
		}
	}
}
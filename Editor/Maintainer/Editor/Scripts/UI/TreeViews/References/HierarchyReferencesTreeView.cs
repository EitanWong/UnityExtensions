#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Core;
	using References;
	using Tools;
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using Object = UnityEngine.Object;
	using UnityEditor.Experimental.SceneManagement;

	internal class HierarchyReferencesTreeView<T> : MaintainerTreeView<T> where T : HierarchyReferenceItem
	{
		private enum SortOption
		{
			Icon,
			GameObjectName,
			ComponentName,
			PropertyPath,
			ReferencesCount,
		}

		private enum Columns
		{
			Icon,
			GameObject,
			Component,
			Property,
			ReferencesCount
		}

		// count should be equal to columns count
		private readonly SortOption[] sortOptions =
		{
			SortOption.Icon,
			SortOption.GameObjectName,
			SortOption.ComponentName,
			SortOption.PropertyPath,
			SortOption.ReferencesCount,
		};

		public HierarchyReferencesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader, model) {}

		public void SelectRow(long objectId, long componentId)
		{
			foreach (var row in rows)
			{
				var rowLocal = (MaintainerTreeViewItem<T>)row;
				if (rowLocal.data.reference.objectId == objectId && rowLocal.data.reference.componentId == componentId)
				{
					SelectRowInternal(rowLocal);
					break;
				}
			}
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new HierarchyReferencesTreeViewItem<T>(id, depth, name, data);
		}

		protected override void PostInit()
		{
			columnIndexForTreeFoldouts = 1;
		}

		protected override int GetDepthIndentation()
		{
			return UIHelpers.EyeButtonSize;
		}

		protected override void SortByMultipleColumns()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<HierarchyReferencesTreeViewItem<T>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);
			for (var i = 1; i < sortedColumns.Length; i++)
			{
				var sortOption = sortOptions[sortedColumns[i]];
				var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.GameObjectName:
						orderedQuery = orderedQuery.ThenBy(l => l.data.name, ascending);
						break;
					case SortOption.ComponentName:
						orderedQuery = orderedQuery.ThenBy(l => l.data.reference.componentName, ascending);
						break;
					case SortOption.PropertyPath:
						orderedQuery = orderedQuery.ThenBy(l => l.data.reference.propertyPath, ascending);
						break;
					case SortOption.ReferencesCount:
						orderedQuery = orderedQuery.ThenBy(l => l.data.ChildrenCount, ascending);
						break;
					case SortOption.Icon:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
		}

		private IOrderedEnumerable<HierarchyReferencesTreeViewItem<T>> InitialOrder(IEnumerable<HierarchyReferencesTreeViewItem<T>> myTypes, IList<int> history)
		{
			var sortOption = sortOptions[history[0]];
			var ascending = multiColumnHeader.IsSortedAscending(history[0]);

			switch (sortOption)
			{
				case SortOption.GameObjectName:
					return myTypes.Order(l => l.data.name, ascending);
				case SortOption.ComponentName:
					return myTypes.Order(l => l.data.reference.componentName, ascending);
				case SortOption.PropertyPath:
					return myTypes.Order(l => l.data.reference.propertyPath, ascending);
				case SortOption.ReferencesCount:
					return myTypes.Order(l => l.data.ChildrenCount, ascending);
				default:
					return myTypes.Order(l => l.data.name, ascending);
			}
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			var objectReferences = DragAndDrop.objectReferences;

			if (objectReferences == null || objectReferences.Length == 0)
			{
				return DragAndDropVisualMode.Rejected;
			}

			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			var validItems = new List<Object>(objectReferences.Length);

			foreach (var reference in objectReferences)
			{
				// reject any objects from assets
				if (AssetDatabase.Contains(reference))
				{
					continue;
				}

				var validObject = false;
				var component = reference as Component;
				var gameObject = reference as GameObject;
				if (component != null)
				{
					gameObject = component.gameObject;
				}
				else if (gameObject == null)
				{
					continue;
				}

				if (gameObject != null)
				{
					if (prefabStage != null && PrefabStageUtility.GetPrefabStage(gameObject) == prefabStage)
					{
						validObject = true;
					}
					else if (gameObject.scene.IsValid())
					{
						validObject = true;
					}
				}

				if (validObject)
				{
					validItems.Add(reference);
				}
			}

			if (validItems.Count == 0)
			{
				return DragAndDropVisualMode.Rejected;
			}

			if (Event.current.type == EventType.DragPerform)
			{
				var alt = Event.current.alt;

				EditorApplication.delayCall += () =>
				{
					ReferencesFinder.FindObjectsReferencesInHierarchy(validItems.ToArray(), !alt);
				};
				DragAndDrop.AcceptDrag();
			}

			return DragAndDropVisualMode.Generic;
		}

		protected override void DrawCell(ref Rect cellRect, MaintainerTreeViewItem<T> genericItem, int columnValue, RowGUIArgs args)
		{
			base.DrawCell(ref cellRect, genericItem, columnValue, args);

			var column = (Columns)columnValue;
			var item = (HierarchyReferencesTreeViewItem<T>)genericItem;

			switch (column)
			{
				case Columns.Icon:

					if (item.depth == 0)
					{
						if (item.icon != null)
						{
							var iconRect = cellRect;
							iconRect.width = IconWidth;
							iconRect.height = EditorGUIUtility.singleLineHeight;

							GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
						}
					}

					break;
				case Columns.GameObject:

					var entryRect = cellRect;
					entryRect.xMin += baseIndent + UIHelpers.EyeButtonPadding;
					
					if (item.depth == 1)
					{
						if (item.icon != null)
						{
							var iconRect = entryRect;
							iconRect.xMin -= UIHelpers.EyeButtonSize - UIHelpers.EyeButtonPadding;
							iconRect.width = IconWidth;
							iconRect.x += IconPadding;
							iconRect.height = EditorGUIUtility.singleLineHeight;

							GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
						}
					}
					else
					{
						/*entryRect.xMin += baseIndent + UIHelpers.EyeButtonPadding;*/
					}
					
					Rect lastRect;
					var eyeButtonRect = entryRect;
					eyeButtonRect.xMin += IconPadding;
					eyeButtonRect.width = UIHelpers.EyeButtonSize;
					eyeButtonRect.height = UIHelpers.EyeButtonSize;
					eyeButtonRect.x += UIHelpers.EyeButtonPadding;

					lastRect = eyeButtonRect;

					if (UIHelpers.IconButton(eyeButtonRect, CSIcons.Show))
					{
						ShowItem(item);
					}

					var labelRect = entryRect;
					labelRect.xMin = lastRect.xMax + UIHelpers.EyeButtonPadding;

					if (item.data.depth == 0 && !item.data.HasChildren)
					{
						GUI.contentColor = CSColors.labelDimmedColor;
					}
					DefaultGUI.Label(labelRect, args.label, args.selected, args.focused);

					GUI.contentColor = Color.white;
					break;
				case Columns.Component:

					var componentName = item.data.reference.componentName;
					if (!string.IsNullOrEmpty(componentName))
					{
						DefaultGUI.Label(cellRect, componentName, args.selected, args.focused);
					}

					break;
				case Columns.Property:

					var propertyPath = item.data.reference.propertyPath;
					if (!string.IsNullOrEmpty(propertyPath))
					{
						DefaultGUI.Label(cellRect, propertyPath, args.selected, args.focused);
					}

					break;
				case Columns.ReferencesCount:

					if (item.depth == 0)
					{
						DefaultGUI.Label(cellRect, item.data.ChildrenCount.ToString(), args.selected, args.focused);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException("column", column, null);
			}
		}

		protected override void ShowItem(TreeViewItem clickedItem)
		{
			var item = (HierarchyReferencesTreeViewItem<T>)clickedItem;
			var target = item.data.Reference;
			var assetPath = item.data.AssetPath;
			
			CSSelectionTools.RevealAndSelectReferencingEntry(assetPath, target);
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(string.Empty, "Referenced Object kind: Game Object or Component"),
					headerTextAlignment = TextAlignment.Center,
					canSort = false,
					width = 22,
					minWidth = 22,
					maxWidth = 22,
					autoResize = false,
					allowToggleVisibility = false,
					contextMenuText = "Icon"
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(CSEditorTools.NicifyName(Columns.GameObject.ToString()), "Game Object name including full hierarchy path"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 200,
					minWidth = 200,
					autoResize = true,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(CSEditorTools.NicifyName(Columns.Component.ToString()), "Component name"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 90,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(CSEditorTools.NicifyName(Columns.Property.ToString()), "Full property path to the referenced item"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 90,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Refs", "Shows how much times object was referenced in the scene"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 33,
					minWidth = 33,
					maxWidth = 50,
					autoResize = false,
					allowToggleVisibility = true,
				}
			};

			var state = new MultiColumnHeaderState(columns)
			{
				sortedColumns = new[] {1, 4},
				sortedColumnIndex = 4
			};
			return state;
		}
	}
}
#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using References;
	using Tools;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	internal class ExactReferencesList<T> : ListTreeView<T> where T : HierarchyReferenceItem
	{
		public ExactReferencesList(TreeViewState state, TreeModel<T> model):base(state, model)
		{
		}

		protected override void PostInit()
		{
			showAlternatingRowBackgrounds = false;
			rowHeight = RowHeight - 4;
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new ExactReferencesListItem<T>(id, depth, name, data);
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref args.rowRect);

			var item = (ExactReferencesListItem<T>)args.item;
			var lastRect = args.rowRect;
			lastRect.xMin += 4;

			if (item.data == null || item.data.reference == null)
			{
				GUI.Label(lastRect, item.displayName);
				return;
			}

			var entry = item.data.reference;
			Rect iconRect;
			
			if (entry.location == Location.NotFound)
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.WarningIconSize;
				iconRect.height = UIHelpers.WarningIconSize;

				GUI.DrawTexture(iconRect, CSEditorIcons.WarnSmallIcon, ScaleMode.ScaleToFit);
				lastRect.xMin += UIHelpers.WarningIconSize + UIHelpers.EyeButtonPadding;
			}
			else if (entry.location == Location.Invisible)
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.WarningIconSize;
				iconRect.height = UIHelpers.WarningIconSize;

				GUI.DrawTexture(iconRect, CSEditorIcons.InfoSmallIcon, ScaleMode.ScaleToFit);
				lastRect.xMin += UIHelpers.WarningIconSize + UIHelpers.EyeButtonPadding;
			}
			else
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.EyeButtonSize;
				iconRect.height = UIHelpers.EyeButtonSize;
				if (UIHelpers.IconButton(iconRect, CSIcons.Show))
				{
					ShowItem(item);
				}
				lastRect.xMin += UIHelpers.EyeButtonSize + UIHelpers.EyeButtonPadding;
			}

			var boxRect = iconRect;
			boxRect.height = lastRect.height;
			boxRect.xMin = iconRect.xMax;
			boxRect.xMax = lastRect.xMax;

			var label = entry.GetLabel();
			DefaultGUI.Label(lastRect, label, args.selected, args.focused);
		}

		protected override void ShowItem(TreeViewItem clickedItem)
		{
			var item = (ExactReferencesListItem<T>)clickedItem;

			var assetPath = item.data.AssetPath;
			var referencingEntry = item.data.Reference;

			CSSelectionTools.RevealAndSelectReferencingEntry(assetPath, referencingEntry);
		}
	}
}
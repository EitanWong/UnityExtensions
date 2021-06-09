#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using System.Linq;
	using UnityEditor;
	using UnityEngine;
	using Core;
	using Tools;

	internal class PathFiltersTab : StringFiltersTab
	{
		private readonly bool showNotice;
		private readonly string headerExtra;

		internal PathFiltersTab(FilterType filterType, string headerExtra, FilterItem[] filtersList, bool showNotice, SaveFiltersCallback saveCallback, GetDefaultsCallback defaultsCallback = null) : base(filterType, filtersList, saveCallback, defaultsCallback)
		{
			caption = new GUIContent("Path <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + ">" + filterType + "</color>", CSEditorIcons.FolderIcon);
			this.headerExtra = headerExtra;
			this.showNotice = showNotice;
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			var paths = DragAndDrop.paths;
			var objectReferences = DragAndDrop.objectReferences;

			if (paths != null && objectReferences != null && paths.Length > 0 && paths.Length == objectReferences.Length)
			{
				var atLeastOneFileAsset = objectReferences.Any(AssetDatabase.Contains);
				if (!atLeastOneFileAsset)
				{
					return;
				}

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (currentEventType == EventType.DragPerform)
				{
					for (var i = 0; i < objectReferences.Length; i++)
					{
						paths[i] = CSPathTools.EnforceSlashes(AssetDatabase.GetAssetPath(objectReferences[i]));
					}

					var needToSave = false;
					var needToShowWarning = false;

					foreach (var path in paths)
					{
						var added = CSFilterTools.TryAddNewItemToFilters(ref filters, FilterItem.Create(path, FilterKind.Path));
						needToSave |= added;
						needToShowWarning |= !added;
					}

					if (needToSave)
					{
						SaveChanges();
					}

					if (needToShowWarning)
					{
						window.ShowNotification(new GUIContent("One or more of the dragged items already present in the list!"));
					}

					DragAndDrop.AcceptDrag();
				}
			}
			Event.current.Use();
		}

		protected override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify full or partial paths to <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + "><b>" +
										(filterType == FilterType.Includes ? "include" : "ignore") + "</b></color>.\n" +
										"You may drag & drop files and folders to this window directly from the Project Browser.",
										UIHelpers.richWordWrapLabel);

			EditorGUILayout.LabelField("Only Extension filter type matches whole word. Extension filter value should start with dot (.dll).", UIHelpers.richWordWrapLabel);

			if (!string.IsNullOrEmpty(headerExtra))
			{
				EditorGUILayout.LabelField(headerExtra, EditorStyles.wordWrappedLabel);
			}

			if (showNotice)
			{
				EditorGUILayout.Space();
				var oldRich = EditorStyles.helpBox.richText;
				var oldSize = EditorStyles.helpBox.fontSize;

				EditorStyles.helpBox.richText = true;
				EditorStyles.helpBox.fontSize = 12;
				EditorGUILayout.HelpBox("<b>Note:</b> If you have both Includes and Ignores added, first Includes are applied, then Ignores are applied to the included paths.", MessageType.Info, true);

				EditorStyles.helpBox.richText = oldRich;
				EditorStyles.helpBox.fontSize = oldSize;
			}
		}

		protected override bool CheckNewItem(ref string newItem)
		{
			newItem = CSPathTools.EnforceSlashes(newItem);
			return true;
		}

		protected override void DrawFilterKindLabel(FilterKind kind)
		{
			GUILayout.Label(kind.ToString(), GUILayout.Width(80));
		}

		protected override FilterKind DrawFilterKindDropdown(FilterKind kind)
		{
			var enumNames = new [] { FilterKind.Path.ToString(), FilterKind.Directory.ToString(), FilterKind.FileName.ToString(), FilterKind.Extension.ToString() };
			return (FilterKind) EditorGUILayout.Popup((int) kind, enumNames, GUILayout.Width(80));
		}

		protected override void DrawFilterIgnoreCaseLabel(bool ignore)
		{
			GUILayout.Label(!ignore ? "Match Case" : "<b>Ignore Case</b>", UIHelpers.richLabel, GUILayout.Width(90));
		}

		protected override bool DrawFilterIgnoreCaseToggle(bool ignore)
		{
			return GUILayout.Toggle(ignore, "Ignore Case", GUILayout.Width(90));
		}
	}
}

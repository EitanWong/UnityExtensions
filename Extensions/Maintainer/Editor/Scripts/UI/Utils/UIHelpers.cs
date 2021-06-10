#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using UnityEditor;
	using UnityEngine;

	internal static class UIHelpers
	{
		public const int EyeButtonSize = 22;
		public const int WarningIconSize = 16;
		public const int EyeButtonPadding = 4;

		// -----------------------------------------------------------------------------
		// static tooling
		// -----------------------------------------------------------------------------

		public static GUIStyle richLabel;
		public static GUIStyle iconLabel;
		public static GUIStyle richButton;
		public static GUIStyle richButtonMid;
		public static GUIStyle richButtonLeft;
		public static GUIStyle richButtonRight;
		public static GUIStyle compactButton;
		public static GUIStyle recordButton;
		public static GUIStyle iconButton;
		public static GUIStyle richWordWrapLabel;
		public static GUIStyle richFoldout;
		public static GUIStyle centeredLabel;
		public static GUIStyle line;
		public static GUIStyle panelWithBackground;
		public static GUIStyle panelWithoutBackground;
		public static GUIStyle panelWithButtonBackground;

		public static GUIStyle treeViewRichLabel;

		public static void SetupStyles()
		{
			if (richLabel != null) return;

			richLabel = new GUIStyle(GUI.skin.label) {richText = true};
			richButton = new GUIStyle(GUI.skin.button) {richText = true};
			richButtonLeft = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "left")) {richText = true};
			richButtonMid = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "mid")) {richText = true};
			richButtonRight = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "right")) {richText = true};

			var customStyles = GUI.skin.customStyles;
			var alreadyExists = false;

			foreach (var style in customStyles)
			{
				if (style.name == "Maintainer styles")
				{
					alreadyExists = true;
					break;
				}
			}

			if (!alreadyExists)
			{
				var markerStyle = new GUIStyle {name = "Maintainer styles"};
				ArrayUtility.AddRange(ref customStyles, new[] { richButtonLeft, richButtonMid, richButtonRight, markerStyle });
				GUI.skin.customStyles = customStyles;
			}

			iconLabel = new GUIStyle(GUI.skin.label)
			{
				overflow = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(0, 0, 0, 0),
				//margin = new RectOffset(0, 0, 0, 0),
				fixedHeight = 16,
				fixedWidth = 22
			};

			compactButton = new GUIStyle(GUI.skin.button)
			{
				overflow = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(6, 6, 1, 3),
				margin = new RectOffset(2, 2, 3, 2),
				richText = true,
				fixedHeight = 22,
				fontSize = 12
			};

			recordButton = new GUIStyle(compactButton) {fixedWidth = 80};

#if UNITY_2019_3_OR_NEWER
			var topIconPadding = EditorGUIUtility.isProSkin ? -3 : -2;
#else
			var topIconPadding = EditorGUIUtility.isProSkin ? -5 : -4;
#endif
			
			iconButton = new GUIStyle(compactButton)
			{
				padding = new RectOffset(0, 0, topIconPadding, -2),
				overflow = EditorGUIUtility.isProSkin ? new RectOffset(1, 1, 1, 1) : new RectOffset(0, 0, 2, 1),
				fixedHeight = 18,
				fixedWidth = 22
			};

			richWordWrapLabel = new GUIStyle(richLabel) {wordWrap = true};

			richFoldout = new GUIStyle(EditorStyles.foldout);
			richFoldout.active = richFoldout.focused = richFoldout.normal;
			richFoldout.onActive = richFoldout.onFocused = richFoldout.onNormal;
			richFoldout.richText = true;

			centeredLabel = new GUIStyle(richLabel) {alignment = TextAnchor.MiddleCenter};

#if UNITY_2019_3_OR_NEWER
			line = new GUIStyle(GUI.skin.button);
#else
			line = new GUIStyle(GUI.skin.box);
#endif

			line.border.top = line.border.bottom = 1;
			line.margin.top = line.margin.bottom = 1;
			line.padding.top = line.padding.bottom = 1;

			panelWithBackground = new GUIStyle(GUI.skin.box) {padding = new RectOffset()};
			panelWithButtonBackground = new GUIStyle(GUI.skin.button) {padding = new RectOffset()};

			panelWithoutBackground = new GUIStyle(EditorStyles.helpBox)
			{
				padding = new RectOffset(), margin = new RectOffset()
			};

			treeViewRichLabel = new GUIStyle("PR Label");
			treeViewRichLabel.padding.left = treeViewRichLabel.padding.right = 2;
			treeViewRichLabel.richText = true;
		}

		public static void Separator(float addSpace = 0)
		{
			if (addSpace > 0)
				GUILayout.Space(addSpace);

#if UNITY_2019_3_OR_NEWER
			GUILayout.Space(-2);
			GUILayout.Box(GUIContent.none, line, GUILayout.ExpandWidth(true), GUILayout.Height(2f));
			GUILayout.Space(-1);
#else
			GUILayout.Box(GUIContent.none, line, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
#endif
			
			if (addSpace > 0)
				GUILayout.Space(addSpace);
		}

		public static void VerticalSeparator()
		{
			GUILayout.Box(GUIContent.none, line, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
		}

		public static void Indent(int level = 5, int topPadding = 2)
		{
			GUILayout.Space(topPadding);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(level * 4);
			EditorGUILayout.BeginVertical();
		}

		public static void UnIndent(int bottomPadding = 5)
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(bottomPadding);
		}

		public static void IndentLevel()
		{
			EditorGUI.indentLevel++;
		}

		public static void UnIndentLevel()
		{
			EditorGUI.indentLevel--;
		}

		public static void Icon(Texture icon, string hint, params GUILayoutOption[] options)
		{
			GUILayout.Label(new GUIContent(icon, hint), iconLabel, options);
		}

		public static void Icon(Rect rect, Texture icon)
		{
			Icon(rect, icon, null);
		}

		public static void Icon(Rect rect, Texture icon, string hint)
		{
			GUI.Label(rect, new GUIContent(icon, hint), iconLabel);
		}

		public static bool ImageButton(string label, Texture image, params GUILayoutOption[] options)
		{
			return ImageButton(label, null, image, options);
		}

		public static bool ImageButton(string label, string hint, Texture image, params GUILayoutOption[] options)
		{
			return ImageButton(label, hint, image, compactButton, options);
		}

		public static bool ImageButton(string label, string hint, Texture image, GUIStyle style, params GUILayoutOption[] options)
		{
			var content = new GUIContent();

			if (!string.IsNullOrEmpty(label))
			{
				content.text = label;
			}

			if (!string.IsNullOrEmpty(hint))
			{
				content.tooltip = hint;
			}

			content.image = image;
			if (!string.IsNullOrEmpty(label))
			{
				content.text = " " + label;
			}

			return GUILayout.Button(content, style, options);
		}

		public static bool ImageButton(Rect rect, string label, Texture image)
		{
			return ImageButton(rect, label, null, image);
		}

		public static bool ImageButton(Rect rect, string label, string hint, Texture image)
		{
			return ImageButton(rect, label, hint, image, compactButton);
		}

		public static bool ImageButton(Rect rect, string label, string hint, Texture image, GUIStyle style)
		{
			var content = new GUIContent();

			if (!string.IsNullOrEmpty(label))
			{
				content.text = label;
			}

			if (!string.IsNullOrEmpty(hint))
			{
				content.tooltip = hint;
			}

			content.image = image;
			if (!string.IsNullOrEmpty(label))
			{
				content.text = " " + label;
			}

			return GUI.Button(rect, content, style);
		}

		public static bool IconButton(Texture icon, params GUILayoutOption[] options)
		{
			return IconButton(icon, null, options);
		}

		public static bool IconButton(Texture icon, string hint, params GUILayoutOption[] options)
		{
			return ImageButton(null, hint, icon, iconButton, options);
		}

		public static bool IconButton(Rect rect, Texture icon)
		{
			return IconButton(rect, icon, null);
		}

		public static bool IconButton(Rect rect, Texture icon, string hint)
		{
			return ImageButton(rect, null, hint, icon, iconButton);
		}

		public static bool RecordButton(RecordBase record, string hint, Texture image, params GUILayoutOption[] options)
		{
			return RecordButton(record, null, hint, image, options);
		}

		public static bool RecordButton(RecordBase record, string label, string hint, Texture image, params GUILayoutOption[] options)
		{
			return record.compactMode ? IconButton(image, hint) : ImageButton(label, hint, image, recordButton, options);
		}

		public static bool ToggleFoldout(ref bool toggle, ref bool foldout, GUIContent caption, params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal();
			toggle = EditorGUILayout.ToggleLeft("", toggle, GUILayout.Width(12));
			Foldout(ref foldout, caption, options);
			GUILayout.EndHorizontal();

			return toggle;
		}

		public static void Foldout(ref bool foldout, string caption, params GUILayoutOption[] options)
		{
			Foldout(ref foldout, new GUIContent(caption), options);
		}

		public static void Foldout(ref bool foldout, GUIContent caption, params GUILayoutOption[] options)
		{
			foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(options), foldout, caption, true, richFoldout);
		}

		public static void TreeViewRichLabel(Rect rect, string label, bool selected, bool focreferenced)
		{
			if (Event.current.type != EventType.Repaint)
				return;
			treeViewRichLabel.Draw(rect, label, false, false, selected, focreferenced);
		}

		public static string WrapTextWithColorTag(string input, Color color)
		{
			var colorString = "#" + ColorUtility.ToHtmlStringRGBA(color);
			return WrapTextWithColorTag(input, colorString);
		}

		// color argument should be in rrggbbaa format or match standard html color name
		public static string WrapTextWithColorTag(string input, string color)
		{
			return "<color=" + color + ">" + input + "</color>";
		}
	}
}
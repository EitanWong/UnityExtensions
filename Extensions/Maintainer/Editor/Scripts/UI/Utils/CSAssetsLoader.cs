#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEngine;

	internal static class CSAssetsLoader
	{
		private static readonly Dictionary<string, Texture> cachedTextures = new Dictionary<string, Texture>();

		public static Texture GetTexture(string fileName)
		{
			return GetTexture(fileName, false, false);
		}

		public static Texture GetIconTexture(string fileName, bool fromEditor = false)
		{
			return GetTexture(fileName, true, fromEditor);
		}

		private static Texture GetTexture(string fileName, bool icon, bool fromEditor)
		{
			Texture result;
			var isDark = EditorGUIUtility.isProSkin;

			var path = fileName;

			if (!fromEditor)
			{
				path = Path.Combine(Maintainer.Directory, "Textures/For" + (isDark ? "Dark/" : "Bright/") + (icon ? "Icons/" : "") + fileName);
			}

			if (cachedTextures.ContainsKey(path))
			{
				result = cachedTextures[path];
			}
			else
			{
				if (!fromEditor)
				{
					result = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				}
				else
				{
					result = EditorGUIUtility.FindTexture(path);
				}

				if (result == null)
				{
					Debug.LogError(Maintainer.LogPrefix + "Some error occurred while looking for image\n" + path);
				}
				else
				{
					cachedTextures[path] = result;
				}
			}
			return result;
		}

		public static Texture GetCachedTypeImage(Type type)
		{
			var key = type.ToString();
			if (cachedTextures.ContainsKey(key))
			{
				return cachedTextures[key];
			}

			var texture = EditorGUIUtility.ObjectContent(null, type).image;
			cachedTextures.Add(key, texture);

			return texture;
		}
	}

	internal static class CSIcons
	{
		public static Texture About { get { return CSAssetsLoader.GetIconTexture("About.png"); } }
		public static Texture HelpOutline { get { return CSAssetsLoader.GetIconTexture("HelpOutline.png"); } }
		public static Texture ArrowLeft { get { return CSAssetsLoader.GetIconTexture("ArrowLeft.png"); } }
		public static Texture ArrowRight { get { return CSAssetsLoader.GetIconTexture("ArrowRight.png"); } }
		public static Texture AssetStore { get { return CSAssetsLoader.GetIconTexture("UAS.png"); } }
		public static Texture AutoFix { get { return CSAssetsLoader.GetIconTexture("AutoFix.png"); } }
		public static Texture Clean { get { return CSAssetsLoader.GetIconTexture("Clean.png"); } }
		public static Texture Clear { get { return CSAssetsLoader.GetIconTexture("Clear.png"); } }
		public static Texture Collapse { get { return CSAssetsLoader.GetIconTexture("Collapse.png"); } }
		public static Texture Copy { get { return CSAssetsLoader.GetIconTexture("Copy.png"); } }
		public static Texture Delete { get { return CSAssetsLoader.GetIconTexture("Delete.png"); } }
		public static Texture DoubleArrowLeft { get { return CSAssetsLoader.GetIconTexture("DoubleArrowLeft.png"); } }
		public static Texture DoubleArrowRight { get { return CSAssetsLoader.GetIconTexture("DoubleArrowRight.png"); } }
		public static Texture Expand { get { return CSAssetsLoader.GetIconTexture("Expand.png"); } }
		public static Texture Export { get { return CSAssetsLoader.GetIconTexture("Export.png"); } }
		public static Texture Find { get { return CSAssetsLoader.GetIconTexture("Find.png"); } }
		public static Texture Filter { get { return CSAssetsLoader.GetIconTexture("Filter.png"); } }
		public static Texture Gear { get { return CSAssetsLoader.GetIconTexture("Gear.png"); } }
		public static Texture Hide { get { return CSAssetsLoader.GetIconTexture("Hide.png"); } }
		public static Texture Home { get { return CSAssetsLoader.GetIconTexture("Home.png"); } }
		public static Texture Issue { get { return CSAssetsLoader.GetIconTexture("Issue.png"); } }
		public static Texture Log { get { return CSAssetsLoader.GetIconTexture("Log.png"); } }
		public static Texture Maintainer { get { return CSAssetsLoader.GetIconTexture("Maintainer.png"); } }
		public static Texture Minus { get { return CSAssetsLoader.GetIconTexture("Minus.png"); } }
		public static Texture More { get { return CSAssetsLoader.GetIconTexture("More.png"); } }
		public static Texture Plus { get { return CSAssetsLoader.GetIconTexture("Plus.png"); } }
		public static Texture Publisher { get { return CSAssetsLoader.GetIconTexture("Publisher.png"); } }
		public static Texture Restore { get { return CSAssetsLoader.GetIconTexture("Restore.png"); } }
		public static Texture Reveal { get { return CSAssetsLoader.GetIconTexture("Reveal.png"); } }
		public static Texture RevealBig { get { return CSAssetsLoader.GetIconTexture("RevealBig.png"); } }
		public static Texture SelectAll { get { return CSAssetsLoader.GetIconTexture("SelectAll.png"); } }
		public static Texture SelectNone { get { return CSAssetsLoader.GetIconTexture("SelectNone.png"); } }
		public static Texture Show { get { return CSAssetsLoader.GetIconTexture("Show.png"); } }
		public static Texture Star { get { return CSAssetsLoader.GetIconTexture("Star.png"); } }
		public static Texture Support { get { return CSAssetsLoader.GetIconTexture("Support.png"); } }
		public static Texture Repeat { get { return CSAssetsLoader.GetIconTexture("Repeat.png"); } }
	}

	internal static class CSEditorIcons
	{
		public static Texture ErrorSmallIcon { get { return CSAssetsLoader.GetIconTexture("console.erroricon.sml", true); } }
		public static Texture ErrorIcon { get { return CSAssetsLoader.GetIconTexture("console.erroricon", true); } }
		public static Texture FolderIcon { get { return CSAssetsLoader.GetIconTexture("Folder Icon", true); } }
		public static Texture InfoSmallIcon { get { return CSAssetsLoader.GetIconTexture("console.infoicon.sml", true); } }
		public static Texture InfoIcon { get { return CSAssetsLoader.GetIconTexture("console.infoicon", true); } }
		public static Texture PrefabIcon { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.prefab"); } }
		public static Texture SceneIcon { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.unity"); } }
		public static Texture ScriptIcon { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.cs"); } }
		public static Texture WarnSmallIcon { get { return CSAssetsLoader.GetIconTexture("console.warnicon.sml", true); } }
		public static Texture WarnIcon { get { return CSAssetsLoader.GetIconTexture("console.warnicon", true); } }

		public static Texture InspectorIcon
		{
			get
			{
				return EditorGUIUtility.isProSkin
					? CSAssetsLoader.GetIconTexture("d_UnityEditor.InspectorWindow", true)
					: CSAssetsLoader.GetIconTexture("UnityEditor.InspectorWindow", true);
			}
		}

		public static Texture HierarchyViewIcon
		{
			get
			{
				return EditorGUIUtility.isProSkin
					? CSAssetsLoader.GetIconTexture("d_UnityEditor.SceneHierarchyWindow", true)
					: CSAssetsLoader.GetIconTexture("UnityEditor.SceneHierarchyWindow", true);
			}
		}

		public static Texture ProjectViewIcon { get { return CSAssetsLoader.GetIconTexture("Project", true); } }

		public static Texture FilterByType { get { return CSAssetsLoader.GetIconTexture("FilterByType", true); } }
		public static Texture GameObjectIcon { get { return CSAssetsLoader.GetCachedTypeImage(typeof(GameObject)); } }
	}

	internal static class CSImages
	{
		public static Texture Logo { get { return CSAssetsLoader.GetTexture("Logo.png"); } }
	}

	internal static class CSColors
	{
		public static Color labelDimmedColor = ChangeColorAlpha(GUI.skin.label.normal.textColor , 150);
		public static Color labelGreenColor = LerpColorToGreen(GUI.skin.label.normal.textColor, 0.3f);
		public static Color labelRedColor = LerpColorToRed(GUI.skin.label.normal.textColor, 0.3f);

		public static Color backgroundGreenTint = EditorGUIUtility.isProSkin ? new Color32(0, 255, 0, 150) : new Color32(0, 255, 0, 30);
		public static Color backgroundRedTint = EditorGUIUtility.isProSkin ? new Color32(255, 0, 0, 150) : new Color32(255, 0, 0, 30);

		private static Color32 LerpColorToRed(Color32 inValue, float greenAmountPercent)
		{
			return Color.Lerp(inValue, new Color32(255, 0, 0, 255), greenAmountPercent);
		}

		private static Color32 LerpColorToGreen(Color32 inValue, float greenAmountPercent)
		{
			return Color.Lerp(inValue, new Color32(0, 255, 0, 255), greenAmountPercent);
		}

		private static Color32 ChangeColorAlpha(Color32 inValue, byte alphaValue)
		{
			inValue.a = alphaValue;
			return inValue;
		}
	}
}
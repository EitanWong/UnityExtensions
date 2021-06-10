#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using References;
	using UnityEngine;
	using Settings;
	using System;
	using UnityEditor;

	public enum ReferenceFinderTab
	{
		Project = 0,
		Scene = 1,
	}

	internal class ReferencesTab : TwoColumnsTab
	{
		[NonSerialized]
		private ReferenceFinderTab currentTab;

		[NonSerialized]
		private readonly GUIContent[] tabsCaptions;

		[NonSerialized]
		private readonly ProjectReferencesTab projectTab;

		[NonSerialized]
		private readonly HierarchyReferencesTab hierarchyTab;

		protected override string CaptionName
		{
			get { return ReferencesFinder.ModuleName; }
		}

		protected override Texture CaptionIcon
		{
			get { return CSIcons.Find; }
		}

		public ReferencesTab(MaintainerWindow window) : base(window)
		{
			if (projectTab == null)
				projectTab = new ProjectReferencesTab(window);

			if (hierarchyTab == null)
				hierarchyTab = new HierarchyReferencesTab(window);

			if (tabsCaptions == null)
			{
				tabsCaptions = new[] { projectTab.Caption, hierarchyTab.Caption };
			}
		}

		public override void Refresh(bool newData)
		{
			base.Refresh(newData);

			currentTab = UserSettings.Instance.referencesFinder.selectedTab;

			switch (currentTab)
			{
				case ReferenceFinderTab.Project:
					projectTab.Refresh(newData);
					break;
				case ReferenceFinderTab.Scene:
					hierarchyTab.Refresh(newData);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void DrawLeftColumnHeader()
		{
			using (new GUILayout.VerticalScope())
			{
				GUILayout.Space(10);

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);
					GUILayout.Label("<size=16><b>Search scope</b></size>", UIHelpers.richLabel);
					GUILayout.FlexibleSpace();

					using (new GUILayout.VerticalScope())
					{
						/*GUILayout.Space(-3);*/
						//if (UIHelpers.ImageButton(null, CSIcons.HelpOutline, GUILayout.ExpandWidth(false)))
						if (UIHelpers.IconButton(CSIcons.HelpOutline, GUILayout.ExpandWidth(false)))
						{
							// TODO: update
							EditorUtility.DisplayDialog(ReferencesFinder.ModuleName + " scopes help",
								"Use " + projectTab.Caption.text + " scope to figure out where any specific asset is referenced in whole project.\n\n" +
								"Use " + hierarchyTab.Caption.text + " scope to figure out where any specific Game Object or component is referenced in active scene or opened prefab.",
								"OK");
						}
					}
					GUILayout.Space(10);
				}

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);
					UIHelpers.Separator();
					GUILayout.Space(10);
				}

				GUILayout.Space(10);

				EditorGUI.BeginChangeCheck();
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(10);
					currentTab = (ReferenceFinderTab)GUILayout.SelectionGrid((int)currentTab, tabsCaptions, 1,
						GUILayout.Height(56), GUILayout.ExpandWidth(true));
					GUILayout.Space(10);
				}

				if (EditorGUI.EndChangeCheck())
				{
					UserSettings.Instance.referencesFinder.selectedTab = currentTab;
					Refresh(false);
				}

				switch (currentTab)
				{
					case ReferenceFinderTab.Project:
						projectTab.DrawLeftColumnHeader();
						break;
					case ReferenceFinderTab.Scene:
						hierarchyTab.DrawLeftColumnHeader();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			base.DrawLeftColumnHeader();
		}

		protected override void DrawLeftColumnBody()
		{
			using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
			{
				switch (currentTab)
				{
					case ReferenceFinderTab.Project:
						projectTab.DrawSettings();
						break;
					case ReferenceFinderTab.Scene:
						hierarchyTab.DrawSettings();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		protected override bool DrawRightColumnCenter()
		{
			switch (currentTab)
			{
				case ReferenceFinderTab.Project:
					projectTab.DrawRightColumn();
					break;
				case ReferenceFinderTab.Scene:
					hierarchyTab.DrawRightColumn();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		protected override void DrawRightColumnBottom()
		{
			switch (currentTab)
			{
				case ReferenceFinderTab.Project:
					projectTab.DrawFooter();
					break;
				case ReferenceFinderTab.Scene:
					hierarchyTab.DrawFooter();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

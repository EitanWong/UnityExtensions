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
	using System.Linq;
	using Core;
	using Tools;

	using UnityEditor;
	using UnityEngine;

	[Serializable]
	internal class RecordsTabState
	{
		public List<bool> selection = new List<bool>();
		public List<bool> compaction = new List<bool>();
		public Vector2 scrollPosition = Vector2.zero;
	}

	internal abstract class RecordsTab<T> : TwoColumnsTab where T : RecordBase
	{
		private const int RecordsPerPage = 100;

		private int recordsCurrentPage;
		private int recordsTotalPages;
		private int[] recordsToDeleteIndexes;
		private T[] records;
		private IShowableRecord gotoRecord;

		protected T[] filteredRecords;

		/* virtual methods */

		protected RecordsTab(MaintainerWindow window) : base(window)
		{

		}

		public override void Refresh(bool newData)
		{
			base.Refresh(newData);

			records = null;
			filteredRecords = null;
			recordsCurrentPage = 0;

			if (newData)
				GetState().scrollPosition = Vector2.zero;
		}

		public override void Draw()
		{
			if (records == null)
			{
				records = LoadLastRecords();
				rightColumnScrollPosition = GetState().scrollPosition;
				ApplySorting();
				ApplyState();
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RecordsPerPage);
				PerformPostRefreshActions();
			}

			base.Draw();

			if (gotoRecord != null)
			{
				gotoRecord.Show();
				gotoRecord = null;
			}
		}

		protected virtual T[] GetRecords()
		{
			return records;
		}

		protected virtual void ClearRecords()
		{
			records = null;
			filteredRecords = null;
		}

		protected virtual void DeleteRecords(int[] indexes)
		{
			recordsToDeleteIndexes = indexes;
			EditorApplication.delayCall += DeleteRecords;
		}

		protected override bool DrawRightColumnCenter()
		{
			if (filteredRecords == null || filteredRecords.Length <= 0)
			{
				DrawEmptyPlaceholder();
				return false;
			}
			DrawCollectionPages();

			return true;
		}

		protected override void DrawRightColumnBottom()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				DrawSelectAllButton();
				DrawSelectNoneButton();
				DrawExpandAllButton();
				DrawCollapseAllButton();
				GUILayout.Space(10);
			}

			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(10);
				DrawCopyReportButton();
				DrawExportReportButton();
				DrawClearResultsButton();
				GUILayout.Space(10);
			}
			GUILayout.Space(10);
		}

		protected virtual void DrawCollectionPages()
		{
			var fromItem = recordsCurrentPage * RecordsPerPage;
			var toItem = fromItem + Math.Min(RecordsPerPage, filteredRecords.Length - fromItem);

			using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
			{
				GUILayout.Space(10);
				using (new GUILayout.VerticalScope())
				{
					GUILayout.FlexibleSpace();
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label(fromItem + 1 + " - " + toItem + " from " + filteredRecords.Length /* + " (" + records.Count + " total)"*/);
						GUILayout.FlexibleSpace();
						using (new GUILayout.HorizontalScope())
						{
							DrawPagesRightHeader();
						}
					}
					GUILayout.FlexibleSpace();
				}

				GUILayout.Space(10);
			}
			UIHelpers.Separator();

			DrawRecords(fromItem, toItem);

			UIHelpers.Separator();

			if (recordsTotalPages <= 1)
			{
				GUILayout.Space(5);
				return;
			}

			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = recordsCurrentPage > 0;
				if (UIHelpers.IconButton(CSIcons.DoubleArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage = 0;
					rightColumnScrollPosition = Vector2.zero;
					GetState().scrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.ArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage--;
					rightColumnScrollPosition = Vector2.zero;
					GetState().scrollPosition = Vector2.zero;
				}
				GUI.enabled = true;
				GUILayout.Label(recordsCurrentPage + 1 + " of " + recordsTotalPages, UIHelpers.centeredLabel);
				GUI.enabled = recordsCurrentPage < recordsTotalPages - 1;
				if (UIHelpers.IconButton(CSIcons.ArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage++;
					rightColumnScrollPosition = Vector2.zero;
					GetState().scrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.DoubleArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage = recordsTotalPages - 1;
					rightColumnScrollPosition = Vector2.zero;
					GetState().scrollPosition = Vector2.zero;
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
		}

		protected virtual void DrawRecords(int fromItem, int toItem)
		{
			rightColumnScrollPosition = GUILayout.BeginScrollView(rightColumnScrollPosition);

			GetState().scrollPosition = rightColumnScrollPosition;
			for (var i = fromItem; i < toItem; i++)
			{
				var record = filteredRecords[i];

				DrawRecord(record, i);

				if (Event.current != null && Event.current.type == EventType.MouseDown)
				{
					var guiRect = GUILayoutUtility.GetLastRect();
					guiRect.height += 2; // to compensate the separator's gap

					if (guiRect.Contains(Event.current.mousePosition))
					{
						Event.current.Use();

						record.compactMode = !record.compactMode;
						GetState().compaction[i] = record.compactMode;
					}
				}
			}

			GUILayout.EndScrollView();
		}

		protected virtual void ApplySorting()
		{
			filteredRecords = records.ToArray();
		}

		protected virtual void DrawRecordCheckbox(RecordBase record)
		{
			EditorGUI.BeginChangeCheck();
			record.selected = EditorGUILayout.ToggleLeft(new GUIContent(""), record.selected, GUILayout.Width(12));
			if (EditorGUI.EndChangeCheck())
			{
				var index = Array.IndexOf(filteredRecords, record);
				GetState().selection[index] = record.selected;

				OnSelectionChanged();
			}
		}

		/* empty virtual methods */

		protected virtual void PerformPostRefreshActions() { }

		protected virtual void DrawPagesRightHeader() { }

		protected virtual string GetReportHeader() { return null; }

		protected virtual string GetReportFooter() { return null; }

		protected virtual string GetReportFileNamePart() { return ""; }

		protected virtual void AfterClearRecords() { }

		protected virtual void OnSelectionChanged() { }

		protected virtual void DrawRecord(T record, int recordIndex) { }

		/* abstract methods */

		protected abstract T[] LoadLastRecords();
		protected abstract RecordsTabState GetState();

		protected abstract void SaveSearchResults();
		protected abstract string GetModuleName();

		/* protected methods */

		protected void DrawShowButtonIfPossible(T record)
		{
			var showableIssueRecord = record as IShowableRecord;
			if (showableIssueRecord == null) return;

			string hintText;
			switch (record.Location)
			{
				case RecordLocation.Unknown:
					hintText = "Oh, sorry, but looks like I have no clue about this record.";
					break;
				case RecordLocation.Scene:
					hintText = "Selects item in the scene. Opens scene with target item if necessary and highlights this scene in the Project Browser.";
					break;
				case RecordLocation.Asset:
					hintText = "Selects asset file in the Project Browser or opens settings asset.";
					break;
				case RecordLocation.Prefab:
					hintText = "Selects Prefab file with item in the Project Browser.";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (UIHelpers.RecordButton(record, "Show", hintText, CSIcons.Show))
			{
				gotoRecord = showableIssueRecord;
			}
		}

		protected void DrawCopyButton(T record)
		{
			if (UIHelpers.RecordButton(record, "Copy", "Copies record text to the clipboard.", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = record.ToString(true);
				MaintainerWindow.ShowNotification("Record copied to clipboard!");
			}
		}

		protected void DrawExpandCollapseButton(RecordBase record)
		{
			var r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(12));
			EditorGUI.BeginChangeCheck();
			record.compactMode = !EditorGUI.Foldout(r, !record.compactMode, GUIContent.none, UIHelpers.richFoldout);
			if (EditorGUI.EndChangeCheck())
			{
				var index = Array.IndexOf(filteredRecords, record);
				GetState().compaction[index] = record.compactMode;
			}
		}

		protected void DrawSelectAllButton()
		{
			if (UIHelpers.ImageButton("Select all", CSIcons.SelectAll))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = true;
				}

				OnSelectionChanged();
			}
		}

		protected void DrawSelectNoneButton()
		{
			if (UIHelpers.ImageButton("Select none", CSIcons.SelectNone))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = false;
				}

				OnSelectionChanged();
			}
		}

		protected void DrawExpandAllButton()
		{
			if (UIHelpers.ImageButton("Expand all", CSIcons.Expand))
			{
				for (var i = 0; i < filteredRecords.Length; i++)
				{
					var record = filteredRecords[i];
					record.compactMode = false;
					GetState().compaction[i] = false;
				}
			}
		}

		protected void DrawCollapseAllButton()
		{
			if (UIHelpers.ImageButton("Collapse all", CSIcons.Collapse))
			{
				for (var i = 0; i < filteredRecords.Length; i++)
				{
					var record = filteredRecords[i];
					record.compactMode = true;
					GetState().compaction[i] = true;
				}
			}
		}

		protected void DrawCopyReportButton()
		{
			if (UIHelpers.ImageButton("Copy report to clipboard", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter());
				MaintainerWindow.ShowNotification("Report copied to clipboard!");
			}
		}

		protected void DrawExportReportButton()
		{
			if (UIHelpers.ImageButton("Export report...", CSIcons.Export))
			{
				var filePath = EditorUtility.SaveFilePanel("Save " + GetModuleName() + " report", "", "Maintainer " + GetReportFileNamePart() + "Report.txt", "txt");
				if (!string.IsNullOrEmpty(filePath))
				{
					var sr = File.CreateText(filePath);
					sr.Write(ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter()));
					sr.Close();
					MaintainerWindow.ShowNotification("Report saved!");
				}
			}
		}

		protected void DrawClearResultsButton()
		{
			if (UIHelpers.ImageButton("Clear results", CSIcons.Clear))
			{
				ClearRecords();
				AfterClearRecords();
			}
		}

		protected void ApplyNewIgnoreFilter(FilterItem newFilter)
		{
			var indexes = new List<int>();
			for (int i = 0; i < filteredRecords.Length; i++)
			{
				if (filteredRecords[i].MatchesFilter(newFilter))
				{
					indexes.Add(i);
				}
			}

			if (indexes.Count > 0)
			{
				DeleteRecords(indexes.ToArray());
			}
		}

		private void DrawEmptyPlaceholder()
		{
			/* logo */

			using (new GUILayout.VerticalScope())
			{
				var rect = GUILayoutUtility.GetRect(0, int.MaxValue, 0, int.MaxValue);
				GUILayout.Space(rect.height);

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();

					var icon = CSIcons.RevealBig;
					if (icon != null)
					{
						icon.wrapMode = TextureWrapMode.Clamp;
						var iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(icon.width),
							GUILayout.Height(icon.height));
						GUI.DrawTexture(iconRect, icon);
					}

					GUILayout.FlexibleSpace();
				}
				GUILayout.Space(25);
				GUILayout.Label("<b><size=12>Please perform new scan</size></b>", UIHelpers.centeredLabel);

				rect = GUILayoutUtility.GetRect(0, int.MaxValue, 0, int.MaxValue);
				GUILayout.Space(rect.height);
			}
		}

		private void DeleteRecords()
		{
			for (var i = recordsToDeleteIndexes.Length - 1; i >= 0; i--)
			{
				var index = recordsToDeleteIndexes[i];
				var record = filteredRecords[index];
				records = CSArrayTools.RemoveAt(records, Array.IndexOf(records, record));

				GetState().selection.RemoveAt(index);
				GetState().compaction.RemoveAt(index);
			}

			recordsToDeleteIndexes = null;

			ApplySorting();

			if (filteredRecords.Length > 0)
			{
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RecordsPerPage);
			}
			else
			{
				recordsTotalPages = 1;
			}

			if (recordsCurrentPage + 1 > recordsTotalPages) recordsCurrentPage = recordsTotalPages - 1;

			SaveSearchResults();
			window.Repaint();
		}

		private void ApplyState()
		{
			if (GetState().selection.Count != filteredRecords.Length)
			{
				GetState().selection = new List<bool>(filteredRecords.Length);
				GetState().compaction = new List<bool>(filteredRecords.Length);

				for (var i = 0; i < filteredRecords.Length; i++)
				{
					GetState().selection.Add(true);
					GetState().compaction.Add(true);
				}
			}

			for (var i = 0; i < filteredRecords.Length; i++)
			{
				var record = filteredRecords[i];
				record.selected = GetState().selection[i];
				record.compactMode = GetState().compaction[i];
			}
		}
	}
}
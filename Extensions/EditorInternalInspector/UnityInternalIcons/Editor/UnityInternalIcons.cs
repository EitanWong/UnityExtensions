﻿/*
 *	Created by Philippe Groarke on 2016-08-28.
 *	Copyright (c) 2016 Tarfmagougou Games. All rights reserved.
 *
 *	Dedication : I dedicate this code to Gabriel, who makes kickass extensions. Now go out and use awesome icons!
 */

namespace EditorInternalInspector
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading;

	struct BuiltinIcon : System.IEquatable<BuiltinIcon>, System.IComparable<BuiltinIcon>
	{
		public GUIContent icon;
		public GUIContent name;

		public override bool Equals(object o) {
			return o is BuiltinIcon && this.Equals((BuiltinIcon)o);
		}

		public override int GetHashCode() {
			return name.GetHashCode();
		}

		public bool Equals(BuiltinIcon o) {
			return this.name.text == o.name.text;
		}

		public int CompareTo(BuiltinIcon o) {
			return this.name.text.CompareTo(o.name.text);
		}
	}

	public class UnityInternalIcons : EditorWindow
	{
		List<BuiltinIcon> _icons = new List<BuiltinIcon>();
		Vector2 _scroll_pos;
		GUIContent _refresh_button;

		[MenuItem("Window/编辑器扩展/Unity内置图标检查器")]
		public static void ShowWindow()
		{
			UnityInternalIcons w = EditorWindow.GetWindow<UnityInternalIcons>();
			UnityInternalIconHelperUII.SetWindowTitle(w, "Unity内置图标检查器");
		}

		void OnEnable()
		{
			_refresh_button = new GUIContent(EditorGUIUtility.IconContent("d_preAudioLoopOff").image,
				"Refresh : Icons are only loaded in memory when the appropriate window is opened.");

			FindIcons();
		}

		/* Find all textures and filter them to narrow the search. */
		void FindIcons()
		{
			_icons.Clear();

			Texture2D[] t = Resources.FindObjectsOfTypeAll<Texture2D>();
			foreach(Texture2D x in t) {
				if (x.name.Length == 0)
					continue;

				if (x.hideFlags != HideFlags.HideAndDontSave && x.hideFlags != (HideFlags.HideInInspector | HideFlags.HideAndDontSave))
					continue;

				if (!EditorUtility.IsPersistent(x))
					continue;

				/* This is the *only* way I have found to confirm the icons are indeed unity builtin. Unfortunately
				 * it uses LogError instead of LogWarning or throwing an Exception I can catch. So make it shut up. */
				UnityInternalIconHelperUII.DisableLogging();
				GUIContent gc = EditorGUIUtility.IconContent(x.name);
				UnityInternalIconHelperUII.EnableLogging();

				if (gc == null)
					continue;
				if (gc.image == null)
					continue;

				_icons.Add(new BuiltinIcon() {
					icon = gc,
					name = new GUIContent(x.name)
				});
			}

			_icons.Sort();
			Resources.UnloadUnusedAssets();
			System.GC.Collect();
			Repaint();
		}
			
		void OnGUI()
		{
			_scroll_pos = EditorGUILayout.BeginScrollView(_scroll_pos);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			if (GUILayout.Button(_refresh_button, EditorStyles.toolbarButton)) {
				FindIcons();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField("总共找到 " + _icons.Count + " 个图标");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("双击复制图标名称", UnityInternalIconHelperUII.GetMiniGreyLabelStyle());

			EditorGUILayout.Space();

			EditorGUIUtility.labelWidth = 100;
			for (int i = 0; i < _icons.Count; ++i) {
				EditorGUILayout.LabelField(_icons[i].icon, _icons[i].name);

				if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.clickCount > 1) {
					EditorGUIUtility.systemCopyBuffer = _icons[i].name.text;
					Debug.Log(_icons[i].name.text + " 已拷贝到剪贴板.");
				}
			}

			EditorGUILayout.EndScrollView();
		}
	}
}

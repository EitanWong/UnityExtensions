#region copyright

//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------

#endregion

namespace CodeStage.Maintainer.UI
{
    using System.IO;
    using Cleaner;
    using Issues;
    using Tools;
    using References;
    using UnityEditor;
    using UnityEngine;

    internal static class MaintainerMenu
    {
        private const string MenuSection = "⚙ Maintainer";

        private const string HierarchyMenu = "GameObject/";
        private const string ContextMenu = "CONTEXT/";
        private const string MenuMiddleName = "通用/";
        private const string Maintainer = "🔍 Maintainer";

        private const string ReferencesFinderMenuName = "🔍 Find References in Project";
        private const string ContextComponentMenu = ContextMenu + "Component/";
        private const string ScriptReferencesContextMenuName = "🔍 Maintainer: Script File References";
        private const string ComponentContextSceneReferencesMenuName = Maintainer + ": References In Scene";
        private const string HierarchyContextSceneReferencesMenuName = Maintainer + "/Game Object References In Scene";

        private const string HierarchyContextSceneReferencesWithComponentsMenuName =
            Maintainer + "/Game Object && Components References In Scene";

        private const string ScriptReferencesContextMenu = ContextComponentMenu + ScriptReferencesContextMenuName;

        private const string SceneReferencesContextMenu =
            ContextComponentMenu + ComponentContextSceneReferencesMenuName;

        private const string SceneReferencesHierarchyMenu = HierarchyMenu + HierarchyContextSceneReferencesMenuName;

        private const string SceneReferencesWithComponentsHierarchyMenu =
            HierarchyMenu + HierarchyContextSceneReferencesWithComponentsMenuName;

        private const string ProjectBrowserContextStart = "Assets/";
        private const string ProjectBrowserContextReferencesFinderName = MenuSection + "/" + ReferencesFinderMenuName;

        private const string ProjectBrowserContextReferencesFinderNoHotKey =
            ProjectBrowserContextStart + ProjectBrowserContextReferencesFinderName;

        private const string ProjectBrowserContextReferencesFinder =
            ProjectBrowserContextReferencesFinderNoHotKey + " %#&s";

        private const string MainMenu = "Window/" + MenuMiddleName + MenuSection + "/";

        private static float lastMenuCallTimestamp;

        [MenuItem(MainMenu + "显示 %#&`", false, 900)]
        private static void ShowWindow()
        {
            MaintainerWindow.Create();
        }

        // [MenuItem(MainMenu + "关于", false, 901)]
        // private static void ShowAbout()
        // {
        // 	MaintainerWindow.ShowAbout();
        // }
        //
        [MenuItem(MainMenu + "查找问题 %#&f", false, 1000)]
        private static void FindAllIssues()
        {
        	IssuesFinder.StartSearch(true);
        }

        [MenuItem(MainMenu + "查找垃圾 %#&g", false, 1001)]
        private static void FindAllGarbage()
        {
            ProjectCleaner.StartSearch(true);
        }

        [MenuItem(MainMenu + "查找所有资源引用 %#&r", false, 1002)]
        private static void FindAllReferences()
        {
            ReferencesFinder.FindAllAssetsReferences();
        }

        [MenuItem(ProjectBrowserContextReferencesFinder, true, 39)]
        public static bool ValidateFindReferences()
        {
            return ProjectScopeReferencesFinder.GetSelectedAssets().Length > 0;
        }

        [MenuItem(ProjectBrowserContextReferencesFinder, false, 39)]
        public static void FindReferences()
        {
            ReferencesFinder.FindSelectedAssetsReferences();
        }

        [MenuItem(ScriptReferencesContextMenu, true, 144445)]
        public static bool ValidateFindScriptReferences(MenuCommand command)
        {
            var scriptPath = CSObjectTools.GetScriptPathFromObject(command.context);
            return !string.IsNullOrEmpty(scriptPath) && Path.GetExtension(scriptPath).ToLower() != ".dll";
        }

        [MenuItem(ScriptReferencesContextMenu, false, 144445)]
        public static void FindScriptReferences(MenuCommand command)
        {
            var scriptPath = CSObjectTools.GetScriptPathFromObject(command.context);
            ReferencesFinder.FindAssetReferences(scriptPath);
        }

        [MenuItem(SceneReferencesContextMenu, true, 144444)]
        public static bool ValidateFindComponentReferences(MenuCommand command)
        {
            return command.context is Component && !AssetDatabase.Contains(command.context);
        }

        [MenuItem(SceneReferencesContextMenu, false, 144444)]
        public static void FindComponentReferences(MenuCommand command)
        {
            HierarchyScopeReferencesFinder.FindComponentReferencesInHierarchy(command.context as Component);
        }

        [MenuItem(SceneReferencesHierarchyMenu, false, -100)]
        public static void FindGameObjectReferences()
        {
            if (Time.unscaledTime.Equals(lastMenuCallTimestamp)) return;
            if (Selection.gameObjects.Length == 0) return;

            ReferencesFinder.FindObjectsReferencesInHierarchy(Selection.gameObjects);

            lastMenuCallTimestamp = Time.unscaledTime;
        }

        [MenuItem(SceneReferencesWithComponentsHierarchyMenu, false, -99)]
        public static void FindGameObjectWithComponentsReferences()
        {
            if (Time.unscaledTime.Equals(lastMenuCallTimestamp)) return;
            if (Selection.gameObjects.Length == 0) return;

            ReferencesFinder.FindObjectsReferencesInHierarchy(Selection.gameObjects, true);

            lastMenuCallTimestamp = Time.unscaledTime;
        }
    }
}
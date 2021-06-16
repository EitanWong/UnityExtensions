using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UnityExtensions.Memo
{
    internal static class MemoFileUtility
    {
        //public readonly static string RELATIVEPATH_DEFAULT = "Assets/Plugins/Memo/";

        private readonly static string SEARCH_KEYWORD = "MemoFileUtility";
        private readonly static string SEARCH_UNITYEDITORMEMO = "MemoSaveData";
        private readonly static string SEARCH_UNITYSCENEMEMO = "SceneMemoSaveData";

        private readonly static string RELATIVEPATH_GUI = "Editor/GUI/";
        private readonly static string RELATIVEPATH_SAVEDATA = "Editor/SaveData/";

        private readonly static string NAME_UNITYEDITORMEMO = "EditorMemoData.asset";
        private readonly static string NAME_UNITYSCENEMEMO = "SceneMemoData.asset";

        private readonly static string NAME_ASSEMBLY = "Memo.Editor";


        static MemoFileUtility()
        {
            if (UnityEditorMemoRootPath.StartsWith("Packages"))
                IsPackageManager = true;
        }

        //=======================================================
        // path
        //=======================================================

        public static string GUIDirectoryPath
        {
            get { return pathSlashFix(Path.Combine(UnityEditorMemoRootPath, RELATIVEPATH_GUI)); }
        }

        public static string SaveDataDirectoryPath
        {
            get { return pathSlashFix(Path.Combine(UnityEditorMemoRootPath, RELATIVEPATH_SAVEDATA)); }
        }

        public static string UnityEditorMemoRootPath
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly().GetName().Name;
                if (assembly.Contains("Assembly"))
                {
                    var guid = getAssetGUID(SEARCH_KEYWORD);

                    // if (string.IsNullOrEmpty(guid) && !IsPackageManager)
                    // {
                    //     Debug.LogError("致命 错误.");
                    //     return RELATIVEPATH_DEFAULT;
                    // }

                    var filePath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid));
                    var scriptPath = Path.GetDirectoryName(filePath);
                    var editorPath = Path.GetDirectoryName(scriptPath);
                    var rootPath = Path.GetDirectoryName(editorPath);

                    return pathSlashFix(rootPath);
                }
                else
                {
                    var guid = getAssetGUID(NAME_ASSEMBLY);

                    // if (string.IsNullOrEmpty(guid) && !IsPackageManager)
                    // {
                    //     Debug.LogError("致命 错误.");
                    //     return RELATIVEPATH_DEFAULT;
                    // }

                    var editorPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid));
                    var rootPath = Path.GetDirectoryName(editorPath);

                    return pathSlashFix(rootPath);
                }
            }
        }

        public static bool IsPackageManager { get; private set; }

        //=======================================================
        // file
        //=======================================================

        public static MemoSaveData LoadUnityEditorMemoData()
        {
            var saveData = FindAssetByType<MemoSaveData>(SEARCH_UNITYEDITORMEMO);
            if (saveData == null)
            {
                if (!Directory.Exists(SaveDataDirectoryPath))
                    Directory.CreateDirectory(SaveDataDirectoryPath);
                var savePath = Path.Combine(SaveDataDirectoryPath, NAME_UNITYEDITORMEMO);
                saveData = ScriptableObject.CreateInstance<MemoSaveData>();
                saveData.AddCategory(new MemoCategory("default"));
                AssetDatabase.CreateAsset(saveData, savePath);
                AssetDatabase.Refresh();
                Debug.Log("备忘录: EditorMemo-SaveData 创建成功");
            }

            return saveData;
        }

        public static SceneMemoSaveData LoadUnitySceneMemoData()
        {
            var saveData = FindAssetByType<SceneMemoSaveData>(SEARCH_UNITYSCENEMEMO);
            if (saveData == null)
            {
                if (!Directory.Exists(SaveDataDirectoryPath))
                    Directory.CreateDirectory(SaveDataDirectoryPath);
                var savePath = Path.Combine(SaveDataDirectoryPath, NAME_UNITYSCENEMEMO);
                saveData = ScriptableObject.CreateInstance<SceneMemoSaveData>();
                AssetDatabase.CreateAsset(saveData, savePath);
                AssetDatabase.Refresh();
                Debug.Log("备忘录: SceneMemo-SaveData 创建成功");
            }

            return saveData;
        }

        public static void ExportUnityEditorMemoData(string json)
        {
            var savePath = EditorUtility.OpenFolderPanel("选择导出路径", Application.dataPath, "");
            if (string.IsNullOrEmpty(savePath))
                return;
            savePath = Path.Combine(savePath, "backup_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".unitymemo");
            using (var sw = new StreamWriter(savePath))
            {
                sw.WriteLine(json);
            }
        }

        public static string ImportUnityEditorMemoData()
        {
            var filePath = EditorUtility.OpenFilePanel("选择 Memo Json", Application.dataPath, "");
            if (string.IsNullOrEmpty(filePath))
                return null;
            if (Path.GetExtension(filePath).Equals("unitymemo"))
            {
                Debug.LogError("UJnityEditorMemo：选中的数据不是首选项数据.");
                return null;
            }

            string json = "";
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("备忘录：数据加载错误. : " + ex);
            }

            return json;
        }

        //=======================================================
        // utility
        //=======================================================

        /// <summary>
        /// get path of dropped file
        /// </summary>
        public static Object GetDraggedObject(Event curEvent, Rect dropArea)
        {
            int ctrlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (curEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(curEvent.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = ctrlID;

                    if (curEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var draggedObj in DragAndDrop.objectReferences)
                        {
                            return draggedObj;
                        }
                    }

                    curEvent.Use();
                    break;
            }

            return null;
        }

        //=======================================================
        // process
        //=======================================================

        private static T FindAssetByType<T>(string type) where T : Object
        {
            var searchFilter = "t:" + type;
            var guid = getAssetGUID(searchFilter);
            if (string.IsNullOrEmpty(guid))
                return null;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static string getAssetGUID(string searchFilter)
        {
            var guids = AssetDatabase.FindAssets(searchFilter);
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning("找到了多个文件.");
            }

            return guids[0];
        }

        private const string forwardSlash = "/";
        private const string backSlash = "\\";

        private static string pathSlashFix(string path)
        {
            return path.Replace(backSlash, forwardSlash);
        }
    }
}
/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlayModeSave
{
    public class PlayModeSave : EditorWindow
    {
        #region CONTEXT

        private const string TOOLMenuItem_NAME = "PlayModeSave";

        [MenuItem("Edit/保存所有运行时更改 &^S", true, int.MaxValue)]
        private static bool CheckIsNotPlaying() => Application.isPlaying;

        [MenuItem("Edit/保存所有运行时更改 &^S", false, int.MaxValue)]
        private static void SaveAllComponentInEditMode()
        {
            if (!Application.isPlaying)
                return;

            foreach (var VARIABLE in FindObjectsOfType<Component>())
            {
                Add(VARIABLE, SaveCommand.SAVE_NOW);
            }

            mouseOverWindow.ShowNotification(new GUIContent("已保存场景中\n所有的修改"), 1f);
        }

        [MenuItem("CONTEXT/Component/立刻保存", true, 2000)]
        private static bool ValidateSaveNowMenu(MenuCommand command) =>
            PrefabUtility.GetPrefabAssetType(command.context) == PrefabAssetType.NotAPrefab &&
            Application.IsPlaying(command.context);

        [MenuItem("CONTEXT/Component/立刻保存", false, 2000)]
        private static void SaveNowMenu(MenuCommand command) => Add(command.context as Component, SaveCommand.SAVE_NOW);

        [MenuItem("CONTEXT/Component/退出 运行模式后保存", true, 2001)]
        private static bool ValidateSaveOnExtiMenu(MenuCommand command) => ValidateSaveNowMenu(command);

        [MenuItem("CONTEXT/Component/退出 运行模式后保存", false, 2001)]
        private static void SaveOnExitMenu(MenuCommand command) =>
            Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE);

        [MenuItem("CONTEXT/Component/应用运行模式下的修改", true, 2002)]
        private static bool ValidateApplyMenu(MenuCommand command) =>
            !Application.isPlaying && _compData.ContainsKey(GetKey(command.context));

        [MenuItem("CONTEXT/Component/应用运行模式下的修改", false, 2002)]
        private static void ApplyMenu(MenuCommand command) => Apply(GetKey(command.context));

        [MenuItem("CONTEXT/ScriptableObject/立刻保存", false, 2000)]
        private static void SaveScriptableObject(MenuCommand command)
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(command.context);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region WINDOW

        //
        // [MenuItem("Window/" + TOOL_NAME, false, int.MaxValue)]
        // private static void ShowWindow() => GetWindow<PlayModeSave>(TOOL_NAME);

        private const string AUTO_APPLY_PREF = "PLAY_MODE_SAVE_autoApply";
        private void OnEnable() => _autoApply = EditorPrefs.GetBool(AUTO_APPLY_PREF, true);
        private void OnDisable() => EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);

        private void OnGUI()
        {
            if (Application.isPlaying) return;

            using (new EditorGUILayout.VerticalScope())
            {
                _autoApply = EditorGUILayout.ToggleLeft("退出运行模式时 自动应用所有更改", _autoApply);
                if (_autoApply)
                {
                    maxSize = minSize = new Vector2(300, 24);
                    return;
                }

                maxSize = minSize = new Vector2(300, 44);
                if (_compData.Count == 0)
                {
                    EditorGUILayout.LabelField("Nothing to apply");
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply All Changes")) ApplyAll();
                    GUILayout.FlexibleSpace();
                }
            }
        }

        #endregion

        #region SAVE

        private static bool _autoApply = true;

        private enum SaveCommand
        {
            SAVE_NOW,
            SAVE_ON_EXITING_PLAY_MODE
        }

        private struct ComponentSaveDataKey
        {
            public int objId;
            public int compId;
            public ComponentSaveDataKey(int objId, int compId) => (this.objId, this.compId) = (objId, compId);
        }

        private struct SaveDataValue
        {
            public SerializedObject serializedObj;
            public SaveCommand saveCmd;

            public SaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd) =>
                (this.serializedObj, this.saveCmd) = (serializedObj, saveCmd);
        }

        private static Dictionary<ComponentSaveDataKey, SaveDataValue> _compData =
            new Dictionary<ComponentSaveDataKey, SaveDataValue>();

        private static ComponentSaveDataKey GetKey(Object comp) =>
            new ComponentSaveDataKey((comp as Component).gameObject.GetInstanceID(),
                (comp as Component).GetInstanceID());

        private static void Add(Component component, SaveCommand cmd)
        {
            var compId = component.GetInstanceID();
            var objId = component.gameObject.GetInstanceID();
            var key = new ComponentSaveDataKey(objId, compId);
            var data = new SerializedObject(component);
            if (_compData.ContainsKey(key)) _compData[key] = new SaveDataValue(data, cmd);
            else _compData.Add(key, new SaveDataValue(data, cmd));
            var prop = new SerializedObject(component).GetIterator();
            while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);
        }

        private static void RemoveNewObjects()
        {
            var compIds = _compData.Keys.ToArray();
            foreach (var id in compIds)
            {
                var obj = EditorUtility.InstanceIDToObject(id.objId) as GameObject;
                if (obj != null) continue;
                _compData.Remove(id);
            }
        }

        private static void Apply(ComponentSaveDataKey key)
        {
            var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
            if (obj == null) return;
            var data = _compData[key].serializedObj;
            var serializedObj = new SerializedObject(data.targetObject);
            var prop = data.GetIterator();
            while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
            serializedObj.ApplyModifiedProperties();
            _compData.Remove(key);
        }

        //[MenuItem("Edit/" + TOOLMenuItem_NAME, false, int.MaxValue)]
        private static void ApplyAll()
        {
            var comIds = _compData.Keys.ToArray();
            foreach (var id in comIds) Apply(id);
        }


        [InitializeOnLoadAttribute]
        private static class ApplicationEventHandler
        {
            private static GameObject AutoApplyFlag = null;
            private const string OBJECT_NAME = "PlayModeSave_AutoApply";
            private static Texture2D _icon = Resources.Load<Texture2D>("Save");

            static ApplicationEventHandler()
            {
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallback;
            }

            private static void OnStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode && _autoApply)
                {
                    AutoApplyFlag = new GameObject(OBJECT_NAME);
                    AutoApplyFlag.hideFlags = HideFlags.HideAndDontSave;
                    return;
                }

                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    foreach (var data in _compData.Values)
                    {
                        if (data.saveCmd == SaveCommand.SAVE_NOW) continue;
                        data.serializedObj.Update();
                    }

                    return;
                }

                if (state != PlayModeStateChange.EnteredEditMode) return;
                PlayModeSave.RemoveNewObjects();
                AutoApplyFlag = GameObject.Find(OBJECT_NAME);
                _autoApply = AutoApplyFlag != null;
                if (!_autoApply) return;
                DestroyImmediate(AutoApplyFlag);
                PlayModeSave.ApplyAll();
            }

            private static void HierarchyItemCallback(int instanceID, Rect selectionRect)
            {
                var keys = _compData.Keys.Where(k => k.objId == instanceID).ToArray();
                if (keys.Length == 0) return;
                if (_icon == null) _icon = Resources.Load<Texture2D>("Save");
                GUI.Box(new Rect(selectionRect.xMax - 10, selectionRect.y + 2, 11, 11), _icon, GUIStyle.none);
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        #endregion
    }
}
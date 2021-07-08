#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base
{
    public abstract class MEDR_Page : UnityEditor.Editor
    {
      //  public MeshEditorWindow ParentWindow;

        public List<MeshEditorItem> CheckFields
        {
            get
            {
                if (MeshEditorWindow.CheckItems==null) return null;
                return MeshEditorWindow.CheckItems;
            }
        }

        public bool Open = false;
         public Vector2 scrollViewPos;
        public string PageName = "NewMeshEditorPage";//页面名称
        public string PageToolTips;
        public Texture2D PageIcon;//页面图标
        public int editCount = 0;//可编辑数量

        protected bool Locked; //上锁
        protected string disableTitle, disableMessage, disableOK, disableCancel;

        #region Event

        private Action OnEditCountTooMuch;
        private Action OnNullEditField;
        private Action OnUnCheckField;

        #endregion

        protected abstract void OnGUI();
        
        /// <summary>
        /// 更新GUI（外部调用）
        /// </summary>
        internal void UpdateGUI()
        {
            if ( CheckFields == null || CheckFields.Count <= 0)
            {
                OnNullEditField?.Invoke();
                return;
            }

            if (editCount > 0 && CheckFields.Count != editCount)
            {
                OnEditCountTooMuch?.Invoke();
                return;
            }

            OnGUI();
        }

        #region VisualCallBack

        public void OnCloseWindowCallFromMainWindow()
        {
            OnWindowDestroy();
        }

        public bool OnUnCheckFromMainWindow()
        {
            return UnCheckField();
        }

        public void OnSceneGUIUpdateFromMainWindow()
        {
            OnSceneGUI();
        }

        /// <summary>
        /// 当SceneGUI更新
        /// </summary>
        protected virtual void OnSceneGUI()
        {
        }

        /// <summary>
        /// 当编辑项取消
        /// </summary>
        protected virtual void OnFieldUnCheck()
        {
        }

        /// <summary>
        /// 当窗户强制关闭
        /// </summary>
        protected virtual void OnWindowDestroy()
        {
        }

        /// <summary>
        /// 取消编辑项
        /// </summary>
        protected bool UnCheckField()
        {
            OnUnCheckField?.Invoke();
            if (Locked)
            {
                Locked = !ShowDisableDialog();
            }

            if (!Locked) //如果没有上锁
                OnFieldUnCheck();
            return !Locked;
        }

        #region ToolFunction

        /// <summary>
        /// 显示禁用对话框
        /// </summary>
        /// <returns></returns>
        private bool ShowDisableDialog()
        {
            return EditorUtility.DisplayDialog(disableTitle, disableMessage, disableOK, disableCancel);
        }
        


        #endregion

        #endregion
    }
}
#endif
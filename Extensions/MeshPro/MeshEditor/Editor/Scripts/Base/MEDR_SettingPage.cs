namespace MeshEditor.Editor.Scripts.Base
{
    public abstract class MEDR_SettingPage: UnityEditor.Editor
    {
        public string PageName = "NewMeshEditorSettingPage";//页面名称
        /// <summary>
        /// 更新GUI(外部调用)
        /// </summary>
        internal void UpdateGUI()
        {
            OnGUI();
        }
        protected abstract void OnGUI();
    }
}

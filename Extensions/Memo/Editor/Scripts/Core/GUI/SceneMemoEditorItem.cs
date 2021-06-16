using System;
using UnityEngine;
using UnityEditor;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class SceneMemoHierarchyMemoEditorItem : MemoEditorWindowItem<SceneMemo> {

        public bool IsEdit;
        public bool IsContextClick = false;

        internal SceneMemoHierarchyMemoEditorItem( SceneMemo data ) : base( data ) {

        }

        protected override void Draw() {
            DrawProcess();
        }

        //======================================================================
        // drawer
        //======================================================================

        private Rect rect = Rect.zero;
        private Vector2 scrollView = Vector2.zero;
        private void DrawProcess() {
            rect = EditorGUILayout.BeginVertical();
            {
                // header
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( Label );
                EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
                {
                    var edit = GUILayout.Toggle( IsEdit, "≡", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width( 18 ) } );
                    if( edit != IsEdit ) {
                        GUIUtility.keyboardControl = 0;
                        IsEdit = edit;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        drawComponents( data.Components );
                        GUILayout.Label( Name );
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;

                // memo
                scrollView = EditorGUILayout.BeginScrollView( scrollView );
                if( IsEdit ) {
                    Undo.IncrementCurrentGroup();
                    MemoUndoHelper.SceneMemoUndo( MemoUndoHelper.UNDO_SCENEMEMO_EDIT );
                    Memo = EditorGUILayout.TextArea( Memo, MemoGUIHelper.Styles.TextAreaWordWrap, new GUILayoutOption[] { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) } );
                } else {
                    GUILayout.Label( Memo, MemoGUIHelper.Styles.MemoLabel );
                }
                EditorGUILayout.EndScrollView();

                // footer
                if( IsEdit ) {
                    EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
                    {
                        GUILayout.FlexibleSpace();
                        var showAtScene = GUILayout.Toggle( ShowAtScene, "场景中显示", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width( 80 ) } );
                        if( showAtScene != ShowAtScene ) {
                            SceneView.RepaintAll();
                            ShowAtScene = showAtScene;
                        }
                        GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( Label );
                        Label = ( UnityEditorMemoLabel )EditorGUILayout.Popup( ( int )Label, MemoGUIHelper.LabelMenu, EditorStyles.toolbarDropDown, GUILayout.Width( 70 ) );
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();
                    if( ShowAtScene ) {
                        GUILayout.Space( 3 );

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label( "宽度" );
                            SceneMemoWidth = EditorGUILayout.Slider( SceneMemoWidth, 200, 500 );
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label( "高度" );
                            SceneMemoHeight = EditorGUILayout.Slider( SceneMemoHeight, 100, 500 );
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label( "文本颜色" );
                            TextCol = ( UnitySceneMemoTextColor )EditorGUILayout.Popup( ( int )TextCol, MemoGUIHelper.TextColorMenu, GUILayout.Width( 60 ) );
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    GUILayout.Space( 5 );
                }
            }
            EditorGUILayout.EndVertical();

            IsContextClick = eventProcess( Event.current );
        }

        private static void drawComponents( Texture2D[] components ) {
            if( components != null ) {
                GUILayout.Space( 3 );
                for( int i = 0; i < components.Length; i++ )
                    GUILayout.Box( components[ i ], GUIStyle.none, new GUILayoutOption[] { GUILayout.MaxWidth( 16 ), GUILayout.MaxHeight( 16 ) } );
            }
        }

        private bool eventProcess( Event e ) {
            switch( e.type ) {
                case EventType.ContextClick:
                    if( rect.Contains( e.mousePosition ) )
                        return true;
                    else
                        return false;
            }
            return false;
        }

        #region property

        //======================================================================
        // property
        //======================================================================

        public string Date {
            get {
                return data.Date;
            }
            set {
                data.Date = value;
            }
        }

        public string Memo {
            get {
                return data.Memo;
            }
            set {
                data.Memo = value;
            }
        }

        public UnityEditorMemoLabel Label {
            get {
                return data.Label;
            }
            set {
                data.Label = value;
            }
        }

        public UnityEditorMemoTexture Tex {
            get {
                return data.Tex;
            }
            set {
                data.Tex = value;
            }
        }

        public UnitySceneMemoTextColor TextCol {
            get {
                return data.TextCol;
            }
            set {
                data.TextCol = value;
            }
        }

        public bool ShowAtScene {
            get {
                return data.ShowAtScene;
            }
            set {
                data.ShowAtScene = value;
            }
        }

        public int LocalIdentifierInFile {
            get {
                return data.LocalIdentifierInFile;
            }
            set {
                data.LocalIdentifierInFile = value;
            }
        }

        public string SceneGuid {
            get {
                return data.SceneGuid;
            }
            set {
                data.SceneGuid = value;
            }
        }

        public string Name {
            get {
                return data.ObjectName;
            }
            set {
                data.ObjectName = value;
            }
        }

        public float SceneMemoWidth {
            get {
                return data.SceneMemoWidth;
            }
            set {
                data.SceneMemoWidth = value;
            }
        }

        public float SceneMemoHeight {
            get {
                return data.SceneMemoHeight;
            }
            set {
                data.SceneMemoHeight = value;
            }
        }


        public int InstanceId {
            get {
                return data.InstanceId;
            }
            set {
                data.InstanceId = value;
            }
        }

        #endregion

    }

}
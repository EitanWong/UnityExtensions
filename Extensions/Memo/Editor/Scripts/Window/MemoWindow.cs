using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#if !UNITY_2018_3_OR_NEWER
using System.Reflection;
#endif

namespace UnityExtensions.Memo {

    public class MemoWindow : EditorWindow {

        public static MemoWindow win;
        private bool IsInitialized = false;

        [SerializeField]
        private TreeViewState memoTreeViewState;
        private MemoTreeView memoTreeView;
        [SerializeField]
        private TreeViewState categoryTreeViewState;
        private MemoCategoryWindowTreeView categoryTreeView;

        private static SplitterState verticalState;
        private static SplitterState horizontalState;

        [ MenuItem( "Window/通用/备忘录" )]
        public static void OpenWindow() {
            win                   = GetWindow<MemoWindow>();
            win.minSize           = MemoWindowHelper.WINDOW_SIZE;
            win.titleContent.text = MemoWindowHelper.WINDOW_TITLE;
            win.titleContent = new GUIContent(MemoWindowHelper.WINDOW_TITLE,EditorGUIUtility.FindTexture("d_winbtn_win_max@2x"),MemoWindowHelper.WINDOW_TITLE);
            win.Show();
            win.IsInitialized = false;
        }

        private void OnEnable() {
            Undo.undoRedoPerformed -= Initialize;
            Undo.undoRedoPerformed += Initialize;
        }

        private void Initialize() {
            MemoWindowHelper.LoadData();
            var category = MemoWindowHelper.GetCategory( selectCategoryId );
            if( category == null ) {
                selectCategoryId = 0;
                return;
            }
            category.Initialize();

            verticalState = new SplitterState( new float[] { position.height * 0.9f, position.height * 0.1f },
                                                          new int[] { 200, 180 }, new int[] { 1500, 300 } );
            SetHorizontalState();

            CategoryTreeViewInitialize();
            MemoTreeViewInitialize();
            IsInitialized = true;
            EditorGUIUtility.keyboardControl = 0;
            Repaint();
        }

        private void SetHorizontalState() {
            horizontalState = isCategoryVisible ? new SplitterState( new float[] { position.width * 0.3f, position.width * 0.7f },
                                                          new int[] { 50, 100 }, new int[] { 350, 1500 } )
                                                : new SplitterState( new float[] { position.width * 0.0f, position.width },
                                                          new int[] { 2, 300 }, new int[] { 300, 1500 } );
        }

        private void CategoryTreeViewInitialize() {
            if( categoryTreeViewState == null )
                categoryTreeViewState = new TreeViewState();

            if( selectCategoryId >= MemoWindowHelper.Data.Category.Count )
                selectCategoryId = MemoWindowHelper.Data.Category.Count - 1;

            categoryTreeViewState.lastClickedID = selectCategoryId;
            categoryTreeViewState.selectedIDs = new List<int>() { selectCategoryId };
            categoryTreeView = new MemoCategoryWindowTreeView( categoryTreeViewState, MemoWindowHelper.Data.Category );
            categoryTreeView.OnContextClick += OnCategoryContextClicked;
            categoryTreeView.OnCategoryOrderChanged += OnCategoryOrderChanged;
            categoryTreeView.SetSelection( categoryTreeViewState.selectedIDs, TreeViewSelectionOptions.FireSelectionChanged );
            categoryTreeView.Reload();
        }

        private void MemoTreeViewInitialize() {
            MemoWindowHelper.CheckMemoHasRootElement( selectCategoryId );

            if( memoTreeViewState == null )
                memoTreeViewState = new TreeViewState();

            var memo = MemoWindowHelper.GetCategory( selectCategoryId ).Memo;
            var rowRectSize = isCategoryVisible ? ( preMemoWidth == 0 ? position.width * 0.7f : preMemoWidth ) : position.width;
            var treeModel = new MemoTreeModel<EditorMemo>( MemoWindowHelper.GetCategory( selectCategoryId ).Memo );
            memoTreeView = new MemoTreeView( memoTreeViewState, treeModel, memo, rowRectSize );
            memoTreeView.OnContextClicked += OnMemoContextClicked;
            memoTreeView.OnMemoOrderChanged += OnMemoOrderChanged;
            memoTreeView.SelectLabel = ( UnityEditorMemoLabel )selectLabel;
            memoTreeView.Reload();
        }

        void OnGUI() {
            if ( win == null )
                OpenWindow();
            if( !IsInitialized )
                Initialize();

            EditorGUI.BeginChangeCheck();
            MemoWindowHelper.OnGUIFirst( position.width );

            DrawContents();

            MemoWindowHelper.OnGUIEnd();

            if ( MemoWindowHelper.Data != null && EditorGUI.EndChangeCheck() )
                EditorUtility.SetDirty( MemoWindowHelper.Data );
        }

#region gui contents

        //======================================================================
        // gui contents
        //======================================================================

        [SerializeField]
        private int selectCategoryId;
        [SerializeField]
        private int selectLabel;
        [SerializeField]
        private string searchText;
        [SerializeField]
        private bool isCategoryVisible = true;
        private readonly string[] categoryMenu = new string[] { "菜单", "", "创建新类别", "", "打开首选项", "", "导出备忘录", "导入备忘录 (覆盖)", "导入备忘录 (添加)" };
        void DrawContents() {
            if ( MemoWindowHelper.Data == null || MemoWindowHelper.GetCategory( selectCategoryId ) == null ) {
                EditorGUILayout.HelpBox( "致命错误.", MessageType.Error );
                selectCategoryId = 0;
                return;
            }
            HeaderGUI();
            MemoSplitterGUI.BeginVerticalSplit( verticalState );
            {
                MemoGUI();
                PostGUI();
            }
            MemoSplitterGUI.EndVerticalSplit();
        }

#region header

        void HeaderGUI() {
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
            {
                var visible = GUILayout.Toggle( isCategoryVisible, "≡", EditorStyles.toolbarButton, GUILayout.Width( 25 ) );
                if( visible != isCategoryVisible ) {
                    isCategoryVisible = visible;
                    SetHorizontalState();
                    memoTreeView.UpdateRowHeight( isCategoryVisible ? position.width * 0.7f : position.width );
                }
                var selectMenu = EditorGUILayout.Popup( 0, categoryMenu, EditorStyles.toolbarPopup, GUILayout.Width( 60 ) );
                switch( selectMenu ) {
                    case 2:
                        OnCategoryCreate();
                        break;
                    case 4:
#if UNITY_2018_3_OR_NEWER
                        SettingsService.OpenUserPreferences( "Preferences/UnityExtensions/Memo" );
#else
                        var assembly = Assembly.Load( "UnityEditor" );
                        var type = assembly.GetType( "UnityEditor.PreferencesWindow" );
                        var method = type.GetMethod( "ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static );
                        method.Invoke( null, null );
#endif
                        break;
                    case 6:
                        OnUnityEditorMemoExport();
                        break;
                    case 7:
                        OnUnityEditorMemoImport( true );
                        break;
                    case 8:
                        OnUnityEditorMemoImport( false );
                        break;
                }

                searchText = EditorGUILayout.TextField( searchText, MemoGUIHelper.Styles.SearchField );
                if( GUILayout.Button( "", MemoGUIHelper.Styles.SearchFieldCancel ) ) {
                    searchText = "";
                    EditorGUIUtility.keyboardControl = 0;
                }
                memoTreeView.searchString = searchText;
            }
            EditorGUILayout.EndHorizontal();
        }

#endregion

#region memo contents

        private float preMemoWidth = 0f;
        void MemoGUI() {
            var memoCount = memoTreeView.GetRows().Count;
            EditorGUILayout.BeginVertical();
            {
                MemoSplitterGUI.BeginHorizontalSplit( horizontalState );
                {
                    // category area
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space( 1 );
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();

                    categoryTreeView.OnGUI( GUILayoutUtility.GetLastRect() );
                    var selected = categoryTreeView.state.lastClickedID;
                    if( selected != selectCategoryId ) {
                        selectCategoryId = selected;
                        OnCategoryChange();
                    }

                    // memo area
                    EditorGUILayout.BeginVertical();
                    {
                        if( memoCount == 0 ) {
                            EditorGUILayout.HelpBox( MemoWindowHelper.TEXT_NO_MEMO, MessageType.Info );
                            GUILayout.FlexibleSpace();
                        } else {
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space( 2 );
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndVertical();

                            var memoRect = GUILayoutUtility.GetLastRect();
                            if( Event.current.type == EventType.Repaint ) {
                                if( preMemoWidth != memoRect.width ) {
                                    memoTreeView.UpdateRowHeight( memoRect.width );
                                    preMemoWidth = memoRect.width;
                                }
                            }
                            memoTreeView.OnGUI( memoRect );
                        }
                        LabelGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                MemoSplitterGUI.EndHorizontalSplit();

            }
            EditorGUILayout.EndVertical();
        }

        private bool[] footerToggle = { true, false, false, false, false, false };
        void LabelGUI() {
            EditorGUILayout.BeginHorizontal();
            {
                var curToggles = new bool[6];
                footerToggle.CopyTo( curToggles, 0 );

                curToggles[0]       = GUILayout.Toggle( curToggles[0], "全部", EditorStyles.toolbarButton );
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 1 );
                curToggles[1]       = GUILayout.Toggle( curToggles[1], EditorMemoPrefs.Label1, EditorStyles.toolbarButton );
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 2 );
                curToggles[2]       = GUILayout.Toggle( curToggles[2], EditorMemoPrefs.Label2, EditorStyles.toolbarButton );
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 3 );
                curToggles[3]       = GUILayout.Toggle( curToggles[3], EditorMemoPrefs.Label3, EditorStyles.toolbarButton );
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 4 );
                curToggles[4]       = GUILayout.Toggle( curToggles[4], EditorMemoPrefs.Label4, EditorStyles.toolbarButton );
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 5 );
                curToggles[5]       = GUILayout.Toggle( curToggles[5], EditorMemoPrefs.Label5, EditorStyles.toolbarButton );
                GUI.backgroundColor = Color.white;
                var label = MemoWindowHelper.ChangeFooterStatus( selectLabel, ref curToggles );
                if ( label != selectLabel ) {
                    MemoUndoHelper.WindowUndo( MemoUndoHelper.UNDO_CHANGE_LABEL ); // avoid error. why? :(
                    postMemoLabel = label;
                    selectLabel = label;
                    memoTreeView.SelectLabel = ( UnityEditorMemoLabel )selectLabel;
                    memoTreeView.Reload();
                    GUIUtility.keyboardControl = 0;
                }
                footerToggle = curToggles;
            }
            EditorGUILayout.EndHorizontal();
        }

#endregion

#region post contents

        [SerializeField]
        private string memoText        = "";
        [SerializeField]
        private string postMemoUrl     = "";
        [SerializeField]
        private int postMemoLabel      = 0;
        [SerializeField]
        private int postMemoTex        = 0;
        [SerializeField]
        private bool postToSlack       = false;
        private Vector2 postScrollView = Vector2.zero;
        /// <summary>
        /// display posting area
        /// </summary>
        void PostGUI() {
            var category = MemoWindowHelper.GetCategory( selectCategoryId );
            EditorGUILayout.BeginVertical( new GUILayoutOption[] { GUILayout.ExpandHeight( true ), GUILayout.ExpandWidth( true ) } );
            {
                GUILayout.Box( "", MemoGUIHelper.Styles.NoSpaceBox, new GUILayoutOption[] { GUILayout.Height( 2 ), GUILayout.ExpandWidth( true ) } );

                EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
                {
                    GUILayout.Label( DateTime.Now.RenderDate() );
                    GUILayout.Space( 5 );
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( postMemoLabel );
                    postMemoLabel = EditorGUILayout.Popup( postMemoLabel, MemoGUIHelper.LabelMenu, EditorStyles.toolbarPopup, GUILayout.Width( 80 ) );
                    GUI.backgroundColor = Color.white;

                    GUILayout.FlexibleSpace();

                    GUILayout.Label( "URL", GUILayout.Width( 30 ) );
                    postMemoUrl = EditorGUILayout.TextField( postMemoUrl, EditorStyles.toolbarTextField );
                }
                EditorGUILayout.EndHorizontal();

                if( EditorMemoPrefs.UnityEditorMemoUseSlack ) {
                    EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
                    {
                        EditorMemoPrefs.UnityEditorMemoSlackChannel = EditorGUILayout.TextField( EditorMemoPrefs.UnityEditorMemoSlackChannel );
                        postToSlack = GUILayout.Toggle( postToSlack, "添加到 Slack", EditorStyles.toolbarButton, GUILayout.Width( 100 ) );
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space( 5 );

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label( ( MemoWindowHelper.TEXT_CREATEMEMO_TITLE + category.Name ).ToMiddleBold() );
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space( 5 );

                    postScrollView = EditorGUILayout.BeginScrollView( postScrollView );
                    {
                        // draft
                        var tmp = EditorGUILayout.TextArea( memoText, MemoGUIHelper.Styles.TextAreaWordWrap, new GUILayoutOption[] { GUILayout.MaxHeight( 300 ) } );
                        if( tmp != memoText ) {
                            Undo.IncrementCurrentGroup();
                            MemoUndoHelper.WindowUndo( MemoUndoHelper.UNDO_DRAFT );
                            memoText = tmp;
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal();
                    {

                        GUILayout.FlexibleSpace();

                        postMemoTex = GUILayout.Toolbar( postMemoTex, MemoGUIHelper.Textures.Emotions, new GUILayoutOption[] { GUILayout.Height( 30 ), GUILayout.MaxWidth( 150 ) } );

                        //if ( GUILayout.Button( "test", new GUILayoutOption[] { GUILayout.Height( 30 ), GUILayout.Width( 50 ) } ) ) {
                        //    for( int i = 0; i < 110; i++ ) {
                        //        category.AddMemo( new UnityEditorMemo( i.ToString(), postMemoLabel, postMemoTex ) );
                        //    }
                        //}

                        // post button
                        GUI.backgroundColor = Color.cyan;
                        if ( GUILayout.Button( "添加", new GUILayoutOption[] { GUILayout.Height( 30 ), GUILayout.MaxWidth( 120 ) } ) ) {
                            MemoUndoHelper.WindowUndo( MemoUndoHelper.UNDO_POST );
                            if( !string.IsNullOrEmpty( memoText ) ) {
                                var memo = new EditorMemo( memoText, postMemoLabel, postMemoTex, postMemoUrl );
                                memo.id = category.Memo.Count;
                                if( EditorMemoPrefs.UnityEditorMemoUseSlack && postToSlack ) {
                                    if( MemoSlackHelper.Post( memo, category.Name ) )
                                        OnMemoPost( category, memo );
                                } else {
                                    OnMemoPost( category, memo );
                                }
                            } else {
                                Debug.LogWarning( MemoWindowHelper.WARNING_MEMO_EMPTY );
                            }
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space( 5 );
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

#endregion

#endregion

#region callback

        private void OnCategoryChange() {
            Undo.IncrementCurrentGroup();
            MemoUndoHelper.WindowUndo( MemoUndoHelper.UNDO_CHANGE_CATEGORY );

            selectLabel = 0;
            footerToggle = new bool[] { true, false, false, false, false, false };
            MemoWindowHelper.GetCategory( selectCategoryId ).Initialize();
            MemoTreeViewInitialize();
            EditorGUIUtility.keyboardControl = 0;
        }

        private void OnCategoryContextClicked( MemoCategory caterogy ) {
            var menu = new GenericMenu();

            if( caterogy == null ) {
                menu.AddItem( new GUIContent( "创建新类别" ), false, () => {
                    OnCategoryCreate();
                } );
            } else {
                if( caterogy.Name != "default" ) {
                    menu.AddItem( new GUIContent( "重命名类别" ), false, () => {
                        categoryTreeView.BeginRename( caterogy );
                    } );
                    menu.AddSeparator( "" );
                    menu.AddItem( new GUIContent( "删除类别" ), false, () => {
                        OnCategoryDelete( caterogy );
                    } );
                }
            }
            menu.ShowAsContext();
        }

        private void OnCategoryOrderChanged( List<MemoCategory> newCategory ) {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_EDIT_CATEGORY );

            MemoWindowHelper.Data.Category = newCategory;
        }

        private void OnCategoryDelete( MemoCategory category ) {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_DELETE_CATEGORY );

            MemoWindowHelper.Data.Category.Remove( category );
            CategoryTreeViewInitialize();
            MemoTreeViewInitialize();
        }

        private void OnCategoryCreate() {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_CREATE_CATEGORY );

            var newCategory = new MemoCategory( "new Category" );
            newCategory.Memo.Add( EditorMemo.Root );
            MemoWindowHelper.Data.AddCategory( newCategory );
            CategoryTreeViewInitialize();
            categoryTreeView.BeginRename( newCategory );
        }

        private void OnMemoPost( MemoCategory category, EditorMemo memo ) {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_POST );

            category.AddMemo( memo );
            category.Initialize();
            memoText = "";
            postMemoTex = 0;
            postMemoUrl = "";
            if( selectLabel == 0 )
                postMemoLabel = 0;
            GUIUtility.keyboardControl = 0;
            MemoTreeViewInitialize();
        }

        private void OnMemoDelete( EditorMemo memo ) {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_DELETE_MEMO );

            MemoWindowHelper.GetCategory( selectCategoryId ).Memo.Remove( memo );
            EditorUtility.SetDirty( MemoWindowHelper.Data );
            MemoTreeViewInitialize();
        }

        private void OnMemoContextClicked( EditorMemo memo ) {
            var menu = new GenericMenu();
            menu.AddItem( new GUIContent( !memo.IsEdit ? "编辑" : "完成" ), false, () => {
                MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                memo.IsEdit = !memo.IsEdit;
                memoTreeView.UpdateRowHeight();
            } );

            menu.AddItem( new GUIContent( "重新添加" ), false, () => {
                OnMemoDelete( memo );
                memo.Date = DateTime.Now.RenderDate();
                OnMemoPost( MemoWindowHelper.GetCategory( selectCategoryId ), memo );
            } );

            if( !string.IsNullOrEmpty( EditorMemoPrefs.Label1 ) ) {
                menu.AddItem( new GUIContent( "Label/" + EditorMemoPrefs.Label1 ), ( int )memo.Label == 1, () => {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    memo.Label = ( UnityEditorMemoLabel )1;
                } );
            }

            if( !string.IsNullOrEmpty( EditorMemoPrefs.Label2 ) ) {
                menu.AddItem( new GUIContent( "Label/" + EditorMemoPrefs.Label2 ), ( int )memo.Label == 2, () => {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    memo.Label = ( UnityEditorMemoLabel )2;
                } );
            }

            if( !string.IsNullOrEmpty( EditorMemoPrefs.Label3 ) ) {
                menu.AddItem( new GUIContent( "Label/" + EditorMemoPrefs.Label3 ), ( int )memo.Label == 3, () => {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    memo.Label = ( UnityEditorMemoLabel )3;
                } );
            }

            if( !string.IsNullOrEmpty( EditorMemoPrefs.Label4 ) ) {
                menu.AddItem( new GUIContent( "Label/" + EditorMemoPrefs.Label4 ), ( int )memo.Label == 4, () => {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    memo.Label = ( UnityEditorMemoLabel )4;
                } );
            }

            if( !string.IsNullOrEmpty( EditorMemoPrefs.Label5 ) ) {
                menu.AddItem( new GUIContent( "Label/" + EditorMemoPrefs.Label5 ), ( int )memo.Label == 5, () => {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    memo.Label = ( UnityEditorMemoLabel )5;
                } );
            }

            menu.AddSeparator( "" );

            if( !string.IsNullOrEmpty( memo.URL ) ) {
                menu.AddItem( new GUIContent( "打开 URL" ), false, () => {
                    Application.OpenURL( memo.URL );
                } );
                menu.AddSeparator( "" );
            }

            menu.AddItem( new GUIContent( "删除" ), false, () => {
                OnMemoDelete( memo );
            } );

            menu.ShowAsContext();
        }

        private void OnMemoOrderChanged( List<EditorMemo> newMemos )
        {
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );

            var currentCategory = MemoWindowHelper.GetCategory( selectCategoryId );
            currentCategory.Memo = newMemos;
            Initialize();
        }

        private void OnUnityEditorMemoExport() {
            var exportData = new MemoExport( MemoWindowHelper.Data.Category );
            var json = JsonUtility.ToJson( exportData );
            MemoFileUtility.ExportUnityEditorMemoData( json );
        }

        private void OnUnityEditorMemoImport( bool isOverride ) {
            var text = MemoFileUtility.ImportUnityEditorMemoData();
            var data = JsonUtility.FromJson<MemoExport>( text );
            if( data == null )
                return;
            MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_IMPORT_MEMO );
            if( isOverride ) {
                MemoWindowHelper.Data.Category = data.category.ToList();
            } else {
                data.category.ToList().RemoveAll( c => c.Name == "default" );
                MemoWindowHelper.Data.Category.AddRange( data.category.ToList() );
            }
            Initialize();
        }

#endregion

    }
}
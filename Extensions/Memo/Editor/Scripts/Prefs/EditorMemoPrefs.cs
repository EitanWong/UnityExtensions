using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityExtensions.Memo {

    internal static class EditorMemoPrefs {

        //======================================================================
        // Preference
        //======================================================================

        private static readonly string[] SCENEMEMO_POSITION = new string[] { "TopLeft", "BottomLeft", "BottomRight" };

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider PreferenceView()
        {
            var provider = new SettingsProvider( "Preferences/UnityExtensions/Memo", SettingsScope.User )
            {
                label = "备忘录",
                guiHandler = ( searchText ) => {
                    GUI.skin.label.richText = true;
                    EditorGUIUtility.labelWidth = 150f;

                    EditorGUILayout.HelpBox( "Unity 编辑器编译或播放后设置将更改.", MessageType.Warning );
                    BasicSettingGUI();
                    GUILayout.Space( 20 );
                    SceneMenuSettingGUI();
                    GUILayout.Space( 20 );
                    SlackSettingGUI();

                    GUILayout.FlexibleSpace();
                    GUILayout.Label( "版本 " + EditorMemoInfo.Version, EditorStyles.miniBoldLabel );

                    GUI.skin.label.richText = false;
                    EditorGUIUtility.labelWidth = 0f;
                },
                keywords = new HashSet<string>( new[] { "UnityEditormemo" } )
            };
            return provider;
        }
#else        
        [PreferenceItem( "UnityEditorMemo" )]
        public static void PreferenceView() {
            GUI.skin.label.richText = true;
            EditorGUIUtility.labelWidth = 150f;

            EditorGUILayout.HelpBox( "Setting will changed after unity editor compiled or played.", MessageType.Warning );
            BasicSettingGUI();
            GUILayout.Space( 20 );
            SceneMenuSettingGUI();
            GUILayout.Space( 20 );
            SlackSettingGUI();

            GUILayout.FlexibleSpace();
            GUILayout.Label( "Version " + UnityEditorMemoInfo.Version, EditorStyles.miniBoldLabel );

            GUI.skin.label.richText = false;
            EditorGUIUtility.labelWidth = 0f;
        }
#endif

        private static void BasicSettingGUI() {
            EditorGUILayout.BeginVertical();
            {
                UnityEditorMemoFontSize = EditorGUILayout.IntSlider( "字体大小", UnityEditorMemoFontSize, 9, 20 );
                GUILayout.Space( 3 );
                GUILayout.Label( "标签设置" );
                GUILayout.Space( 3 );
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 1 );
                    GUILayout.Space( 5 );
                    GUILayout.Toggle( false, "", EditorStyles.toolbarButton, GUILayout.Width( 50 ) );
                    Label1 = EditorGUILayout.TextField( Label1 );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 2 );
                    GUILayout.Space( 5 );
                    GUILayout.Toggle( false, "", EditorStyles.toolbarButton, GUILayout.Width( 50 ) );
                    Label2 = EditorGUILayout.TextField( Label2 );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 3 );
                    GUILayout.Space( 5 );
                    GUILayout.Toggle( false, "", EditorStyles.toolbarButton, GUILayout.Width( 50 ) );
                    Label3 = EditorGUILayout.TextField( Label3 );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 4 );
                    GUILayout.Space( 5 );
                    GUILayout.Toggle( false, "", EditorStyles.toolbarButton, GUILayout.Width( 50 ) );
                    Label4 = EditorGUILayout.TextField( Label4 );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 5 );
                    GUILayout.Space( 5 );
                    GUILayout.Toggle( false, "", EditorStyles.toolbarButton, GUILayout.Width( 50 ) );
                    Label5 = EditorGUILayout.TextField( Label5 );
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndVertical();
        }

        private static void SceneMenuSettingGUI() {
            GUILayout.Label( "场景备忘录 设置", EditorStyles.boldLabel );
            GUILayout.Space( 3 );

            UnitySceneMemoActive = EditorGUILayout.Toggle( "启用 场景备忘录", UnitySceneMemoActive );
            if( UnitySceneMemoActive ) {
                GUILayout.Space( 5 );
                GUILayout.Label( "SceneView 备注位置" );
                UnitySceneMemoPosition = GUILayout.Toolbar( UnitySceneMemoPosition, SCENEMEMO_POSITION );
            }
        }

        private static void SlackSettingGUI() {
            GUILayout.Label( "带有Slack 的备忘录", EditorStyles.boldLabel );
            GUILayout.Space( 3 );

            UnityEditorMemoUseSlack = EditorGUILayout.Toggle( "使用 Slack 集成", UnityEditorMemoUseSlack );
            if( UnityEditorMemoUseSlack ) {
                EditorGUILayout.BeginVertical( EditorStyles.helpBox );
                {
                    GUILayout.Label( "访问令牌" );
                    UnityEditorMemoSlackAccessToken = EditorGUILayout.TextField( UnityEditorMemoSlackAccessToken );
                    GUILayout.Space( 3 );
                    GUILayout.Label( "渠道" );
                    UnityEditorMemoSlackChannel = EditorGUILayout.TextField( UnityEditorMemoSlackChannel );
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static void LabelGUI() {
        }

#region save data

        //======================================================================
        // Save Data
        //======================================================================

        private const string KEY_EDITORMEMO_FONTSIZE = "UnityExtensions.UnityEditorMemo.FontSize";

        private const string KEY_EDITORMEMO_LABEL1 = ".UnityExtensions.UnityEditorMemo.Label1";
        private const string KEY_EDITORMEMO_LABEL2 = ".UnityExtensions.UnityEditorMemo.Label2";
        private const string KEY_EDITORMEMO_LABEL3 = ".UnityExtensions.UnityEditorMemo.Label3";
        private const string KEY_EDITORMEMO_LABEL4 = ".UnityExtensions.UnityEditorMemo.Label4";
        private const string KEY_EDITORMEMO_LABEL5 = ".UnityExtensions.UnityEditorMemo.Label5";

        private const string KEY_SCENEMEMO_AVAILABLE     = ".UnityExtensions.UnityEditorMemo.SceneMemoAvailable";
        private const string KEY_SCENEMEMO_POSITION      = ".UnityExtensions.UnityEditorMemo.SceneMemoPosition";

        private const string KEY_EDITORMEMO_USESLACK     = ".UnityExtensions.UnityEditorMemo.UseSlack";
        private const string KEY_EDITORMEMO_SLACKTOKEN   = "UnityExtensions.UnityEditorMemo.SlackToken";
        private const string KEY_EDITORMEMO_SLACKCHANNEL = "UnityExtensions.UnityEditorMemo.SlackChannel";

        private const string KEY_EDITORMEMO_ISPACKAGE = ".UnityExtensions.UnityEditorMemo.IsLoadPackage";

        public static int UnityEditorMemoFontSize {
            get {
                return EditorPrefs.GetInt( ProjectName + KEY_EDITORMEMO_FONTSIZE, 11 );
            }
            set {
                EditorPrefs.SetInt( ProjectName + KEY_EDITORMEMO_FONTSIZE, value );
            }
        }

        public static bool UnitySceneMemoActive {
            get {
                return EditorPrefs.GetBool( KEY_SCENEMEMO_AVAILABLE, false );
            }
            set {
                EditorPrefs.SetBool( KEY_SCENEMEMO_AVAILABLE, value );
            }
        }

        public static int UnitySceneMemoPosition {
            get {
                return EditorPrefs.GetInt( KEY_SCENEMEMO_POSITION, 0 );
            }
            set {
                EditorPrefs.SetInt( KEY_SCENEMEMO_POSITION, value );
            }
        }

        public static bool UnityEditorMemoUseSlack {
            get {
                return EditorPrefs.GetBool( ProjectName + KEY_EDITORMEMO_USESLACK, false );
            }
            set {
                EditorPrefs.SetBool( ProjectName + KEY_EDITORMEMO_USESLACK, value );
            }
        }

        public static string UnityEditorMemoSlackAccessToken {
            get {
                return EditorPrefs.GetString( KEY_EDITORMEMO_SLACKTOKEN, "" );
            }
            set {
                EditorPrefs.SetString( KEY_EDITORMEMO_SLACKTOKEN, value );
            }
        }

        public static string UnityEditorMemoSlackChannel {
            get {
                return EditorPrefs.GetString( KEY_EDITORMEMO_SLACKCHANNEL, "general" );
            }
            set {
                EditorPrefs.SetString( KEY_EDITORMEMO_SLACKCHANNEL, value );
            }
        }

        public static string Label1 {
            get {
                return EditorPrefs.GetString( ProjectName + KEY_EDITORMEMO_LABEL1, "" );
            }
            set {
                EditorPrefs.SetString( ProjectName + KEY_EDITORMEMO_LABEL1, value );
            }
        }

        public static string Label2 {
            get {
                return EditorPrefs.GetString( ProjectName + KEY_EDITORMEMO_LABEL2, "" );
            }
            set {
                EditorPrefs.SetString( ProjectName + KEY_EDITORMEMO_LABEL2, value );
            }
        }

        public static string Label3 {
            get {
                return EditorPrefs.GetString( ProjectName + KEY_EDITORMEMO_LABEL3, "" );
            }
            set {
                EditorPrefs.SetString( ProjectName + KEY_EDITORMEMO_LABEL3, value );
            }
        }

        public static string Label4 {
            get {
                return EditorPrefs.GetString( ProjectName + KEY_EDITORMEMO_LABEL4, "" );
            }
            set {
                EditorPrefs.SetString( ProjectName + KEY_EDITORMEMO_LABEL4, value );
            }
        }

        public static string Label5 {
            get {
                return EditorPrefs.GetString( ProjectName + KEY_EDITORMEMO_LABEL5, "" );
            }
            set {
                EditorPrefs.SetString( ProjectName + KEY_EDITORMEMO_LABEL5, value );
            }
        }

        public static bool IsLoadPackage {
            get {
                return EditorPrefs.GetBool( ProjectName + KEY_EDITORMEMO_ISPACKAGE );
            }
            set {
                EditorPrefs.SetBool( ProjectName + KEY_EDITORMEMO_ISPACKAGE, value );
            }
        }

#endregion

        private static string projectName;
        private static string ProjectName {
            get {
                if( string.IsNullOrEmpty( projectName ) ) {
                    var split = Application.dataPath.Split( '/' );
                    projectName = split[ split.Length - 2 ];
                }
                return projectName;
            }
        }

    }

}
using UnityEngine;
using UnityEditor;

namespace UnityExtensions.Memo {

    internal enum SceneViewPos {
        TOPLEFT,
        BOTTOMLEFT,
        BOTTOMRIGHT,
    };

    internal static class SceneMemoSceneView {

        public static void OnGUI( SceneMemo memo ) {
            if ( memo == null || !memo.ShowAtScene )
                return;

            Handles.BeginGUI();
            GUILayout.BeginArea( memoRect( memo ) );
            {
                Draw( memo );
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static Vector2 scrollView = Vector2.zero;
        private static bool InVisible;
        private static void Draw( SceneMemo memo ) {
            EditorGUILayout.BeginVertical();
            {
                // header
                GUI.backgroundColor = MemoGUIHelper.Colors.SceneMemoLabelColor( memo.Label );
                EditorGUILayout.BeginHorizontal( MemoGUIHelper.Styles.MemoHeader );
                {
                    if( GUILayout.Button( InVisible ? "●" : "x", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width( 18 ) } ) ) {
                        Undo.IncrementCurrentGroup();
                        MemoUndoHelper.SceneMemoUndo( MemoUndoHelper.UNDO_SCENEMEMO_EDIT );
                        InVisible = !InVisible;
                    }
                    EditorGUILayout.BeginHorizontal();
                    {
                        drawComponents( memo.Components );
                        GUILayout.Label( memo.ObjectName );
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;

                if( !InVisible ) {
                    // memo
                    GUI.backgroundColor = MemoGUIHelper.Colors.TransparentColor;
                    scrollView = EditorGUILayout.BeginScrollView( scrollView, MemoGUIHelper.Styles.NoSpaceBox );
                    {
                        MemoGUIHelper.Styles.MemoLabel.normal.textColor = MemoGUIHelper.Colors.SceneMemoTextColor( memo.TextCol );
                        GUILayout.Label( memo.Memo, MemoGUIHelper.Styles.MemoLabel );
                        MemoGUIHelper.Styles.MemoLabel.normal.textColor = MemoGUIHelper.Colors.DefaultTextColor;
                    }
                    EditorGUILayout.EndScrollView();
                    GUI.backgroundColor = Color.white;
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void drawComponents( Texture2D[] components ) {
            if( components != null ) {
                GUILayout.Space( 3 );
                for( int i = 0; i < components.Length; i++ )
                    GUILayout.Box( components[ i ], GUIStyle.none, new GUILayoutOption[] { GUILayout.MaxWidth( 16 ), GUILayout.MaxHeight( 16 ) } );
            }
        }

        private static Rect memoRect( SceneMemo memo ) {
            var width       = memo.SceneMemoWidth;
            var height      = memo.SceneMemoHeight;
            var sceneWidth  = SceneView.lastActiveSceneView.position.width;
            var sceneHeight = SceneView.lastActiveSceneView.position.height;

            // clamp
            if( sceneWidth - 15f < width )
                width = sceneWidth - 15f;
            if( sceneHeight - 15f < height )
                height = sceneHeight - 15f;

            var pos = ( SceneViewPos )EditorMemoPrefs.UnitySceneMemoPosition;
            switch( pos ) {
                case SceneViewPos.TOPLEFT:
                    return new Rect( 5f, 5f, width, height );
                case SceneViewPos.BOTTOMLEFT:
                    return new Rect( 5f, ( sceneHeight - height ) - 25f, width, height );
                case SceneViewPos.BOTTOMRIGHT:
                    return new Rect( ( sceneWidth - width ) - 5f, ( sceneHeight - height ) - 25f, width, height );
            }
            return new Rect( ( sceneWidth - width ) - 5f, ( sceneHeight - height ) - 25f, width, height );
        }

    }
}
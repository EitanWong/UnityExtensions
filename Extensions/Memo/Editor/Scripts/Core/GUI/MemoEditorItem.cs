using System;
using UnityEngine;
using UnityEditor;

using Object       = UnityEngine.Object;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class MemoEditorItem : MemoEditorWindowItem<EditorMemo> {

        private const float SIZE_EMOJI = 32f;
        private const float SIZE_TEX = 23f;

        private float rowRectWidth = 0f;
        private Action RefreshCustomRowHeights;

        internal MemoEditorItem( EditorMemo data, Action RefreshCustomRowHeights ) : base( data ) {
            var label = ( int )data.Label;
            footerToggle = new bool[] {
                label == 0,
                label == 1,
                label == 2,
                label == 3,
                label == 4,
                label == 5,
            };
            this.RefreshCustomRowHeights = RefreshCustomRowHeights;
        }

        protected override void Draw( Rect rect ) {
            rowRectWidth = rect.width;

            if( Event.current.type == EventType.Repaint ) {
                GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( Label );
                MemoGUIHelper.Styles.MemoHeader.Draw( headerRect( rect ), false, false, false, false );
                GUI.backgroundColor = Color.white;
                MemoGUIHelper.Styles.MemoBack.Draw( backgroundRect( rect ), false, false, false, false );
            }

            HeaderGUI( headerRect( rect ) );
            MemoGUI( backgroundRect( rect ) );
            GUI.backgroundColor = Color.white;
        }

        private void HeaderGUI( Rect headerRect ) {
            headerRect.y += 1f;
            headerRect.xMin += 3f;
            GUI.Label( headerRect, Date );
            if( !string.IsNullOrEmpty( URL ) ) {
                var urlRect = headerRect;
                urlRect.xMin += GUI.skin.label.CalcSize( new GUIContent( Date ) ).x;
                urlRect.width = EditorGUIUtility.singleLineHeight;
                urlRect.height = EditorGUIUtility.singleLineHeight;
                GUI.DrawTexture( urlRect, MemoGUIHelper.Textures.OpenLink );
            }
        }

        private float preAraHeight = 0f;
        private void MemoGUI( Rect rect ) {
            var emojiRect = this.emojiRect( rect );
            if( MemoGUIHelper.Textures.Emotions[ ( int )Tex ] != null )
                GUI.DrawTexture( emojiRect, MemoGUIHelper.Textures.Emotions[ ( int )Tex ] );

            rect.y = emojiRect.y;
            rect.xMin += emojiRect.xMax + 3f;
            rect.xMax = rect.xMax - 2f;

            if( !IsEdit ) {
                rect.height = MemoGUIHelper.Styles.MemoLabel.CalcHeight( new GUIContent( Memo ), rowRectWidth - SIZE_EMOJI - 17f );
                EditorGUI.LabelField( rect, Memo, MemoGUIHelper.Styles.MemoLabel );
                ObjectReferenceGUI( rect );
            } else {
                rect.height = MemoGUIHelper.Styles.TextAreaWordWrap.CalcHeight( new GUIContent( Memo ), rowRectWidth - SIZE_EMOJI - 12f );
                if( preAraHeight != rect.height ) {
                    RefreshCustomRowHeights();
                    preAraHeight = rect.height;
                }
                rect.xMax -= 4f;
                var memo = GUI.TextArea( rect, Memo, MemoGUIHelper.Styles.TextAreaWordWrap );
                if( memo != Memo ) {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    Memo = memo;
                }

                if( ObjectReferenceGUI( rect ) )
                    rect.y += EditorGUIUtility.singleLineHeight;

                rect.y += EditorGUIUtility.singleLineHeight + rect.height;
                rect.height = EditorGUIUtility.singleLineHeight;
                var url = EditorGUI.TextField( rect, URL );
                if( url != URL ) {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    URL = url;
                }
                var urlLabelRect = rect;
                urlLabelRect.x -= 32f;
                EditorGUI.LabelField( urlLabelRect, "URL:" );

                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = SIZE_TEX;
                var tex = ( UnityEditorMemoTexture )GUI.Toolbar( rect, ( int )Tex, MemoGUIHelper.Textures.Emotions );
                if( tex != Tex ) {
                    MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                    Tex = tex;
                }

                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                labelSelectionGUI( rect );
            }
        }

        private bool[] footerToggle = { true, false, false, false, false, false };
        private void labelSelectionGUI( Rect rect ) {
            var curToggles = new bool[ 6 ];
            footerToggle.CopyTo( curToggles, 0 );

            rect.width = rect.width / 6;
            rect.height = 15f;

            GUI.backgroundColor = Color.white;
            curToggles[ 0 ] = GUI.Toggle( rect, curToggles[ 0 ], "", MemoGUIHelper.Styles.LargeButtonLeft );
            rect.x += rect.width;
            GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 1 );
            curToggles[ 1 ] = GUI.Toggle( rect, curToggles[ 1 ], "", MemoGUIHelper.Styles.LargeButtonMid );
            rect.x += rect.width;
            GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 2 );
            curToggles[ 2 ] = GUI.Toggle( rect, curToggles[ 2 ], "", MemoGUIHelper.Styles.LargeButtonMid );
            rect.x += rect.width;
            GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 3 );
            curToggles[ 3 ] = GUI.Toggle( rect, curToggles[ 3 ], "", MemoGUIHelper.Styles.LargeButtonMid );
            rect.x += rect.width;
            GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 4 );
            curToggles[ 4 ] = GUI.Toggle( rect, curToggles[ 4 ], "", MemoGUIHelper.Styles.LargeButtonMid );
            rect.x += rect.width;
            GUI.backgroundColor = MemoGUIHelper.Colors.LabelColor( 5 );
            curToggles[ 5 ] = GUI.Toggle( rect, curToggles[ 5 ], "", MemoGUIHelper.Styles.LargeButtonRight );
            rect.x += rect.width;
            GUI.backgroundColor = Color.white;
            var label = ( UnityEditorMemoLabel )MemoWindowHelper.ChangeFooterStatus( (int)Label, ref curToggles );
            if( label != Label ) {
                MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_CHANGE_LABEL );
                Label = label;
            }
            footerToggle = curToggles;
        }

        private bool ObjectReferenceGUI( Rect rect ) {
            var obj = ObjectRef.Obj;

            if( ObjectRef.HasReferenceObject() ) {
                var objectRect = rect;
                objectRect.width = IsEdit ? objectRect.width - 70f : objectRect.width;
                objectRect.y += EditorGUIUtility.standardVerticalSpacing + rect.height;
                objectRect.height = EditorGUIUtility.singleLineHeight;

                if( obj != null ) {
                    //Undo.IncrementCurrentGroup();
                    //UndoHelper.EditorMemoUndo( UndoHelper.UNDO_MEMO_EDIT );
                    ObjectRef.Obj = EditorGUI.ObjectField( objectRect, ObjectRef.Obj, typeof( Object ), true );
                } else {
                    var buttonWidth = EditorStyles.objectField.CalcSize( new GUIContent( ObjectRef.SceneMemo.ObjectName ) );
                    objectRect.width = buttonWidth.x;
                    if( GUI.Button( objectRect, ObjectRef.SceneMemo.ObjectName, EditorStyles.objectField ) ) {
                        SceneMemoHelper.PopupWindowContent.Initialize( ObjectRef.SceneMemo );
                        PopupWindow.Show( rect, SceneMemoHelper.PopupWindowContent );
                    }
                    var textureRect = objectRect;
                    textureRect.xMin += buttonWidth.x;
                    drawComponent( textureRect, ObjectRef.SceneMemo.Components );
                }

                if( IsEdit ) {
                    var buttonRect = objectRect;
                    buttonRect.x = objectRect.xMax;
                    buttonRect.width = 70f;
                    if( GUI.Button( buttonRect, "clear" ) ) {
                        MemoUndoHelper.EditorMemoUndo( MemoUndoHelper.UNDO_MEMO_EDIT );
                        ObjectRef.Obj = null;
                        ObjectRef.ScenePath = null;
                        ObjectRef.SceneMemo = null;
                        ObjectRef.LocalIdentifierInFile = 0;
                        GUIUtility.keyboardControl = 0;
                    }
                }
                return true;
            } else {
                return false;
            }
        }

        private void drawComponent( Rect rect, Texture2D[] texture2d ) {
            rect.width = EditorGUIUtility.singleLineHeight;
            if( texture2d != null ) {
                for( int i = 0; i < texture2d.Length; i++ ) {
                    GUI.DrawTexture( rect, texture2d[ i ] );
                    rect.x += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        #region property

        //=======================================================
        // gui property
        //=======================================================

        private Rect headerRect( Rect bgRect ) {
            var rect = bgRect;
            rect.height = MemoGUIHelper.Styles.MemoHeader.fixedHeight;
            return rect;
        }

        private Rect backgroundRect( Rect bgRect ) {
            var rect = headerRect( bgRect );

            rect.y += rect.height;
            rect.height = bgRect.height - rect.height;
            return rect;
        }

        private Rect emojiRect( Rect bgRect ) {
            var rect = bgRect;
            rect.y += 4f;
            rect.xMin += 5f;
            rect.width = SIZE_EMOJI;
            rect.height = SIZE_EMOJI;
            return rect;
        }

        //=======================================================
        // property
        //=======================================================

        private string Date {
            get {
                return data.Date;
            }
            set {
                data.Date = value;
            }
        }

        private string Memo {
            get {
                return data.Memo;
            }
            set {
                data.Memo = value;
            }
        }

        private string URL {
            get {
                return data.URL;
            }
            set {
                data.URL = value;
            }
        }

        private UnityEditorMemoLabel Label {
            get {
                return data.Label;
            }
            set {
                data.Label = value;
            }
        }

        private UnityEditorMemoTexture Tex {
            get {
                return data.Tex;
            }
            set {
                data.Tex = value;
            }
        }

        private MemoObject ObjectRef {
            get {
                return data.ObjectRef;
            }
            set {
                data.ObjectRef = value;
            }
        }

        private bool IsEdit {
            get {
                return data.IsEdit;
            }
            set {
                data.IsEdit = value;
            }
        }

        #endregion

    }
}
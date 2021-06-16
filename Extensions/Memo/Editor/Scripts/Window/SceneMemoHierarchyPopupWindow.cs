using UnityEngine;
using UnityEditor;

namespace UnityExtensions.Memo {

    internal class SceneMemoHierarchyWindow : PopupWindowContent {

        private SceneMemo memo;
        private SceneMemoHierarchyMemoEditorItem _memoMemoEditorItem;

        public void Initialize( SceneMemo memo ) {
            this.memo = memo;
            _memoMemoEditorItem = new SceneMemoHierarchyMemoEditorItem( memo );
        }

        public override void OnOpen() {
            base.OnOpen();

            if( memo.SceneMemoWidth == 0 ) {
                memo.SceneMemoWidth = 200f;
                memo.SceneMemoWidth = 100f;
            }

            editorWindow.minSize = new Vector2( 250, 150 );
            editorWindow.maxSize = new Vector2( 350, 200 );
            Undo.undoRedoPerformed += editorWindow.Repaint;
        }

        public override void OnClose() {
            Undo.undoRedoPerformed -= editorWindow.Repaint;
        }

        public override void OnGUI( Rect rect ) {
            if( _memoMemoEditorItem == null ) {
                editorWindow.Close();
                return;
            }

            EditorGUI.BeginChangeCheck();

            _memoMemoEditorItem.OnGUI();
            if( _memoMemoEditorItem.IsContextClick ) {
                var menu = new GenericMenu();
                menu.AddItem( new GUIContent( "编辑" ), false, () => {
                    _memoMemoEditorItem.IsEdit = true;
                } );
                menu.AddItem( new GUIContent( "删除" ), false, () => {
                    MemoUndoHelper.SceneMemoUndo( MemoUndoHelper.UNDO_SCENEMEMO_DELETE );
                    SceneMemoHelper.RemoveMemo( memo );
                    memo = null;
                    editorWindow.Close();
                } );
                menu.ShowAsContext();
            }

            if ( EditorGUI.EndChangeCheck() )
                SceneMemoHelper.SetDirty();
        }

        public override Vector2 GetWindowSize() {
            if( memo.ShowAtScene && _memoMemoEditorItem.IsEdit ) {
                return new Vector2( 270, 200 );
            } else {
                return new Vector2( 270, 150 );
            }
        }

    }

}
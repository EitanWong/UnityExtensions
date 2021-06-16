using UnityEditor;

namespace UnityExtensions.Memo {

    internal static class MemoUndoHelper {

        public static string UNDO_DRAFT             = "edit post memo";
        public static string UNDO_POST              = "post memo";
        public static string UNDO_MEMO_EDIT         = "edit memo";
        public static string UNDO_DELETE_MEMO       = "delete memo";
        public static string UNDO_EDIT_CATEGORY     = "edit category";
        public static string UNDO_DELETE_CATEGORY   = "delete category";
        public static string UNDO_CREATE_CATEGORY   = "create category";

        public static string UNDO_SCENEMEMO_ADD     = "add scene memo";
        public static string UNDO_SCENEMEMO_EDIT    = "edit scene memo";
        public static string UNDO_SCENEMEMO_DELETE  = "delete scene memo";

        public static string UNDO_CHANGE_CATEGORY   = "change category";
        public static string UNDO_CHANGE_LABEL      = "change label";

        public static string UNDO_IMPORT_MEMO       = "import memo";

        public static void EditorMemoUndo( string text ) {
            if ( MemoWindowHelper.Data != null )
                Undo.RecordObject( MemoWindowHelper.Data, text );
        }

        public static void SceneMemoUndo( string text ) {
            if ( SceneMemoHelper.Data != null )
                Undo.RecordObject( SceneMemoHelper.Data, text );
        }

        public static void WindowUndo( string text ) {
            if ( MemoWindow.win != null )
                Undo.RecordObject( MemoWindow.win, text );
        }

    }
}
using UnityEditor;

namespace UnityExtensions.Memo {

    [CustomEditor( typeof( MemoSaveData ) )]
    public class EditorMemoSaveDataEditor : Editor {

        public override void OnInspectorGUI() {
            EditorGUI.BeginDisabledGroup( true );
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }

    }

}
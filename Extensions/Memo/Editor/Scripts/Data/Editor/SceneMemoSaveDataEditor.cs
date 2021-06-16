using UnityEditor;

namespace UnityExtensions.Memo {

    [CustomEditor( typeof( SceneMemoSaveData ) )]
    public class SceneMemoSaveDataEditor : Editor {

        public override void OnInspectorGUI() {
            EditorGUI.BeginDisabledGroup( true );
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }

    }

}
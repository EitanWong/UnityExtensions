using UnityEditor;
using UnityEngine;

namespace UnityExtensions.Memo {

    internal abstract class MemoEditorWindowItem<T> {

        [SerializeField]
        protected T data;

        public MemoEditorWindowItem( T data ) {
            this.data = data;
        }

        public T Data {
            get {
                return data;
            }
        }

        public void OnGUI() {
            if( data == null ) {
                DrawIfDataIsNull();
                return;
            }
            EditorGUI.BeginChangeCheck();
            Draw();
            if( EditorGUI.EndChangeCheck() )
                GUI.changed = true;
        }

        public void OnGUI( Rect rect ) {
            if( data == null ) {
                DrawIfDataIsNull();
                return;
            }

            EditorGUI.BeginChangeCheck();
            Draw( rect );
            if( EditorGUI.EndChangeCheck() )
                GUI.changed = true;
        }

        protected virtual void Draw() { }
        protected virtual void Draw( Rect rect ) { }
        protected virtual void DrawIfDataIsNull() { }

    }

}
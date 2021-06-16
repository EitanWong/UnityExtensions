using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class SceneMemo {

        public string Date;
        public string Memo;
        public UnityEditorMemoLabel Label;
        public UnityEditorMemoTexture Tex;
        public UnitySceneMemoTextColor TextCol;
        public bool ShowAtScene = false;

        public int LocalIdentifierInFile;
        public string SceneGuid;
        public string ObjectName;
        public Texture2D[] Components;

        public float SceneMemoWidth  = 270f;
        public float SceneMemoHeight = 150f;

        [NonSerialized]
        public int InstanceId;

        public SceneMemo( int localIdentifierInFile, string sceneGuid ) {
            Date                  = DateTime.Now.RenderDate();
            SceneGuid             = sceneGuid;
            LocalIdentifierInFile = localIdentifierInFile;
            ShowAtScene           = false;
        }

        public void Initialize( int instanceId ) {
            var obj = EditorUtility.InstanceIDToObject( instanceId ) as GameObject;
            if ( obj == null )
                return;

            ObjectName     = obj.name;
            InstanceId     = instanceId;
            var components = obj.GetComponents<Component>().Where( c => c != null ).Where( c => !( c is Transform ) ).Reverse();
            Components     = components.Select( c => AssetPreview.GetMiniThumbnail( c ) ).Where( t => t != null ).ToArray();
        }

        public void SelectObject() {
            var obj = EditorUtility.InstanceIDToObject( InstanceId );
            if( obj != null ) {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject( obj );
            }
        }

        public static implicit operator int( SceneMemo memo ) {
            return memo.LocalIdentifierInFile;
        }

    }

}
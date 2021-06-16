using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class SceneMemoScene {

        public string Guid;
        [SerializeField]
        public List<SceneMemo> Memo = new List<SceneMemo>();

        private Dictionary<int, SceneMemo> CachedMemo = new Dictionary<int, SceneMemo>();

        public SceneMemoScene( string guid ) {
            Guid = guid;
            Memo = new List<SceneMemo>();
        }

        public void Initialize() {
            CachedMemo = new Dictionary<int, SceneMemo>();
        }

        public static implicit operator string( SceneMemoScene scene ) {
            return scene.Guid;
        }

        //=======================================================
        // public
        //=======================================================

        public void AddMemo( GameObject obj, int localIdentifierInFile ) {
            if ( GetMemo( obj ) != null ) {
                Debug.LogError( "dame dame" );
                return;
            }

            Memo.Add( new SceneMemo( localIdentifierInFile, Guid ) );
        }

        public SceneMemo GetMemo( GameObject obj ) {
            var instanceId = obj.GetInstanceID();
            SceneMemo memo = getMemoFromCache( instanceId );
            if( memo == null ) {
                var localIdentifierInFile = SceneMemoHelper.GetLocalIdentifierInFile( obj );
                memo = GetMemo( localIdentifierInFile );
                if ( memo != null ) {
                    memo.Initialize( instanceId );
                    CachedMemo.Add( instanceId, memo );
                }
            }
            return memo;
        }

        public SceneMemo GetMemo( int localIdentifierInFile ) {
            return Memo.FirstOrDefault( m => m == localIdentifierInFile );
        }

        public void DeleteMemo( SceneMemo memo ) {
            CachedMemo.Remove( memo.InstanceId );
            Memo.Remove( memo );
        }

        //=======================================================
        // private
        //=======================================================

        public SceneMemo getMemoFromCache( int instanceId ) {
            if ( CachedMemo == null ) {
                Initialize();
                return null;
            }

            SceneMemo memo = null;
            CachedMemo.TryGetValue( instanceId, out memo );
            return memo;
        }

    }

}
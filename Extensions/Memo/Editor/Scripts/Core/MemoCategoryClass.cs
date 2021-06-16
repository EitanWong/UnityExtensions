using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class MemoCategory {

        public string Name;
        public DateTime Date;
        public int MenuDepth;

        [SerializeField]
        public List<EditorMemo> Memo = new List<EditorMemo>();

        public MemoCategory( string name ) {
            Name = name;
            Date = DateTime.Now;
            Memo = new List<EditorMemo>();
            MenuDepth = 0;
        }

        public void Initialize() {
            for( int i = 0; i < Memo.Count; i++ )
                Memo[ i ].Initialize( i );
        }

        public void AddMemo( EditorMemo memo ) {
            Memo.Add( memo );
        }

    }

}
using System;
using System.Collections.Generic;
using UnityEngine;

// this code from unity technologies tree view sample
// http://files.unity3d.com/mads/TreeViewExamples.zip
namespace UnityExtensions.Memo {

    [Serializable]
    public class MemoTreeElement {
        [SerializeField] int m_ID;
        [SerializeField] string m_Name;
        [SerializeField] int m_Depth;
        [NonSerialized] MemoTreeElement m_Parent;
        [NonSerialized] List<MemoTreeElement> m_Children;

        public int depth {
            get { return m_Depth; }
            set { m_Depth = value; }
        }

        public MemoTreeElement parent {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public List<MemoTreeElement> children {
            get { return m_Children; }
            set { m_Children = value; }
        }

        public bool hasChildren {
            get { return children != null && children.Count > 0; }
        }

        public string name {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int id {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public MemoTreeElement() { }

        public MemoTreeElement( string name, int depth, int id ) {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
        }

    }

}
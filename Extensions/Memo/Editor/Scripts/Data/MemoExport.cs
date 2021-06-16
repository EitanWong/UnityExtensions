using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtensions.Memo {

    [Serializable]
    internal class MemoExport {

        public MemoCategory[] category;

        public MemoExport( List<MemoCategory> c ) {
            category = c.ToArray();
        }

    }

}
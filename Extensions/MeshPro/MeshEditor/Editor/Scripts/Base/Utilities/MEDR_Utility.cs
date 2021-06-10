using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base;

namespace MeshEditor.Editor.Scripts.Base.Utilities
{
    // ReSharper disable once InconsistentNaming
    public static class MEDR_Utility
    {
        public static Vector3 GetMeshItemMiddlePosition(List<MeshEditorItem> fItems)
        {
            Vector3 result=Vector3.zero;
            foreach (var item in fItems)
            {
                result += item.Filter.transform.position;
            }
            return result/fItems.Count;
        }
    }
}

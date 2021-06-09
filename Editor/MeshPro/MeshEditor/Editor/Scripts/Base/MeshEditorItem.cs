using UnityEngine;

namespace TransformPro.MeshPro.MeshEditor.Editor.Scripts.Base
{
    public class MeshEditorItem
    {
        public MeshFilter Filter;
        public MeshRenderer Renderer;

        public  bool Check;
        

        public static MeshEditorItem Create(MeshFilter filter,MeshRenderer renderer)
        {
            var field = new MeshEditorItem {Filter = filter, Renderer = renderer};
            field.Check = true;
            return field;
        }
    }
}

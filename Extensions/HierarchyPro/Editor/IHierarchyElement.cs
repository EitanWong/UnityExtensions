using UnityEngine.UIElements;

namespace HierarchyPro.Editor
{
    internal interface IHierarchyElement
    {
        void Canvas(HierarchyCanvas canvas);
        VisualElement CreateCanvasElement();
    }
}



using UnityEngine.UIElements;

namespace HierarchyPro
{
    internal interface IHierarchyElement
    {
        void Canvas(HierarchyCanvas canvas);
        VisualElement CreateCanvasElement();
    }
}



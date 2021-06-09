using UnityEngine.UIElements;

namespace HierarchyPro
{
    internal interface IHierarchyShelf
    {
        void Canvas(HierarchyCanvas canvas);
        int ShelfPriority();
        VisualElement CreateShelfElement();
    }
}
using UnityEngine.UIElements;

namespace HierarchyPro.Editor
{
    internal interface IHierarchyShelf
    {
        void Canvas(HierarchyCanvas canvas);
        int ShelfPriority();
        VisualElement CreateShelfElement();
    }
}
namespace PrefabBrush.PrefabBrushData
{
    public enum PB_ActiveTab { About, PrefabPaint, Settings, Saves, PrefabErase }
    public enum PB_Direction { Up, Down, Left, Right, Forward, Backward }
    public enum PB_EraseDetectionType { Collision, Distance }
    public enum PB_EraseTypes { PrefabsInBrush, PrefabsInBounds }
    public enum PB_PaintType { Surface, Physics, Single }
    public enum PB_ParentingStyle { None, Surface, SingleParent, ClosestFromList, RoundRobin }
    public enum PB_PrefabDisplayType { Icon, List }
    public enum PB_SaveApplicationType { Set, Multiply }
    public enum PB_SaveOptions { New, Open, Save, SaveAs, ComfirationOverwrite, ComfirmationDelete, ComfirmationOpen }
    public enum PB_ScaleType { None, SingleValue, MultiAxis }
    public enum PB_DragModType { Position, Rotation, Scale}
    public enum PB_PrefabDataType { Prefab, PrefabData}
}
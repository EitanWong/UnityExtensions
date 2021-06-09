namespace Hub.Editor.Scripts.Base
{
    using UnityEditor;
    public abstract class Hub_PageBase : Editor
    {
        public string pageName = nameof(Hub_PageBase);
        public int layer;
        public void DrawPage()
        {
            OnGUI();
        }

        public void PageOnFocus()
        {
            OnFocus();
        }

        protected abstract void OnGUI();

        protected virtual void OnFocus()
        {
        }

        // protected void DropAreaGUI(Rect drop_area, Action DrawCallBack,
        //     Action<Object> DropCallBack, Action<Event> OnContextClick)
        // {
        //     Event evt = Event.current;
        //     //Rect drop_area = GUILayoutUtility.GetRect(x, width, y, height, GUILayout.ExpandWidth(expandWidth));
        //     GUILayout.BeginArea(drop_area);
        //     DrawCallBack?.Invoke();
        //     switch (evt.type)
        //     {
        //         case EventType.DragUpdated:
        //         case EventType.DragPerform:
        //             if (!drop_area.Contains(evt.mousePosition))
        //                 return;
        //
        //             DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //
        //             if (evt.type == EventType.DragPerform)
        //             {
        //                 DragAndDrop.AcceptDrag();
        //
        //                 foreach (Object dragged_object in DragAndDrop.objectReferences)
        //                 {
        //                     // Do On Drag Stuff here
        //                     DropCallBack?.Invoke(dragged_object);
        //                 }
        //             }
        //
        //             break;
        //         case EventType.ContextClick:
        //             OnContextClick?.Invoke(evt);
        //             break;
        //     }
        //
        //     GUILayout.EndArea();
        // }
    }
}
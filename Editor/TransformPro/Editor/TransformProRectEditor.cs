using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(RectTransform))]
public class TransformProRectEditor : Editor
{
    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        bool dirty = false;
        RectTransform
            rectTransform = (RectTransform) this.target;
        //Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        //CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;
        Vector2 pivot = rectTransform.pivot;

        Rect rectAnchorX = EditorGUILayout.GetControlRect(false);
        Rect rectAnchorXLabel = new Rect(rectAnchorX) {width = 80};
        Rect rectAnchorXMin = new Rect(rectAnchorX) {x = rectAnchorXLabel.xMax, width = (rectAnchorX.width - 80) * 0.5f};
        Rect rectAnchorXMax = new Rect(rectAnchorX) {x = rectAnchorXMin.xMax, width = (rectAnchorX.width - 80) * 0.5f};

        Rect rectAnchorY = EditorGUILayout.GetControlRect(false);
        Rect rectAnchorYLabel = new Rect(rectAnchorY) {width = 80};
        Rect rectAnchorYMin = new Rect(rectAnchorY) {x = rectAnchorYLabel.xMax, width = (rectAnchorY.width - 80) * 0.5f};
        Rect rectAnchorYMax = new Rect(rectAnchorY) {x = rectAnchorYMin.xMax, width = (rectAnchorY.width - 80) * 0.5f};

        Rect rectPivot = EditorGUILayout.GetControlRect(false);
        Rect rectPivotLabel = new Rect(rectPivot) {width = 80};
        Rect rectPivotX = new Rect(rectPivot) {x = rectPivotLabel.xMax, width = (rectPivot.width - 80) * 0.5f};
        Rect rectPivotY = new Rect(rectPivot) {x = rectPivotX.xMax, width = (rectPivot.width - 80) * 0.5f};

        EditorGUI.BeginChangeCheck();
        GUI.Label(rectAnchorXLabel, "Anchor X");
        anchorMin.x = EditorGUI.FloatField(rectAnchorXMin, anchorMin.x);
        anchorMin.y = EditorGUI.FloatField(rectAnchorYMin, anchorMin.y);
        if (EditorGUI.EndChangeCheck())
        {
            rectTransform.anchorMin = anchorMin;
            dirty = true;
        }
        EditorGUI.BeginChangeCheck();
        GUI.Label(rectAnchorYLabel, "Anchor Y");
        anchorMax.x = EditorGUI.FloatField(rectAnchorXMax, anchorMax.x);
        anchorMax.y = EditorGUI.FloatField(rectAnchorYMax, anchorMax.y);
        if (EditorGUI.EndChangeCheck())
        {
            rectTransform.anchorMax = anchorMax;
            dirty = true;
        }
        EditorGUI.BeginChangeCheck();
        GUI.Label(rectPivotLabel, "Pivot");
        pivot.x = EditorGUI.FloatField(rectPivotX, pivot.x);
        pivot.y = EditorGUI.FloatField(rectPivotY, pivot.y);
        if (EditorGUI.EndChangeCheck())
        {
            rectTransform.pivot = pivot;
            dirty = true;
        }

        EditorGUILayout.EndVertical();

        Rect anchorRect = GUILayoutUtility.GetRect(56, 56, 56, 56);
        this.DrawGraph(anchorRect, rectTransform);

        EditorGUILayout.EndHorizontal();

        Rect rectPosition = EditorGUILayout.GetControlRect(false);
        Rect rectPositionLabel = new Rect(rectPosition) {width = 80};
        Rect rectPositionX = new Rect(rectPosition) {x = rectPositionLabel.xMax, width = (rectPosition.width - 80) * 0.333f};
        Rect rectPositionY = new Rect(rectPosition) {x = rectPositionX.xMax, width = (rectPosition.width - 80) * 0.333f};
        Rect rectPositionZ = new Rect(rectPosition) {x = rectPositionY.xMax, width = (rectPosition.width - 80) * 0.333f};

        Rect rectSizeDelta = EditorGUILayout.GetControlRect(false);
        Rect rectSizeDeltaLabel = new Rect(rectSizeDelta) {width = 80};
        Rect rectSizeDeltaX = new Rect(rectSizeDelta) {x = rectSizeDeltaLabel.xMax, width = (rectSizeDelta.width - 80) * 0.333f};
        Rect rectSizeDeltaY = new Rect(rectSizeDelta) {x = rectSizeDeltaX.xMax, width = (rectSizeDelta.width - 80) * 0.333f};

        Vector3 position = rectTransform.position;
        GUI.Label(rectPositionLabel, "Position");
        EditorGUI.BeginChangeCheck();
        position.x = EditorGUI.FloatField(rectPositionX, position.x);
        position.y = EditorGUI.FloatField(rectPositionY, position.y);
        position.z = EditorGUI.FloatField(rectPositionZ, position.z);
        if (EditorGUI.EndChangeCheck())
        {
            rectTransform.position = position;
        }

        Vector2 sizeDelta = rectTransform.sizeDelta;
        GUI.Label(rectSizeDeltaLabel, "Size Delta");
        EditorGUI.BeginChangeCheck();
        sizeDelta.x = EditorGUI.FloatField(rectSizeDeltaX, sizeDelta.x);
        sizeDelta.y = EditorGUI.FloatField(rectSizeDeltaY, sizeDelta.y);
        if (EditorGUI.EndChangeCheck())
        {
            rectTransform.sizeDelta = sizeDelta;
        }

        if (GUI.changed || dirty)
        {
            EditorUtility.SetDirty(rectTransform);
        }
    }

    private void DrawGraph(Rect rect, RectTransform rectTransform)
    {
        Color background = new Color(0.3f, 0.3f, 0.3f);
        Color colorAnchor = new Color(1, 0, 0, 0.2f);
        Rect rectBackground = new Rect(rect);
        rectBackground.width -= 2;
        rectBackground.height -= 2;
        rectBackground.x += 1;
        rectBackground.y += 1;

        EditorGUI.DrawRect(rect, Color.white);
        EditorGUI.DrawRect(rectBackground, background);

        Vector2 anchorMin = rectTransform.anchorMin;
        anchorMin.y = 1 - anchorMin.y;
        Vector2 anchorMax = rectTransform.anchorMax;
        anchorMax.y = 1 - anchorMax.y;
        bool xAnchorHasSize = !Mathf.Approximately(anchorMin.x, anchorMax.x);
        bool yAnchorHasSize = !Mathf.Approximately(anchorMin.y, anchorMax.y);

        Rect rectAnchor = this.GetRelativeMinMaxRect(rectBackground, anchorMin, anchorMax);
        rectAnchor.width = Mathf.Max(1, rectAnchor.width);
        rectAnchor.height = Mathf.Max(1, rectAnchor.height);
        if (xAnchorHasSize)
        {
            EditorGUI.DrawRect(new Rect(rectAnchor.x, rectBackground.y, rectAnchor.width, rectBackground.height), colorAnchor);
        }
        else
        {
            EditorGUI.DrawRect(new Rect(rectAnchor.x - 1, rectBackground.y, 3, rectBackground.height), colorAnchor);
            EditorGUI.DrawRect(new Rect(rectAnchor.x, rectBackground.y, 1, rectBackground.height), colorAnchor);
        }
        if (yAnchorHasSize)
        {
            EditorGUI.DrawRect(new Rect(rectBackground.x, rectAnchor.y, rectBackground.width, rectAnchor.height), colorAnchor);
        }
        else
        {
            EditorGUI.DrawRect(new Rect(rectBackground.x, rectAnchor.y - 1, rectBackground.width, 3), colorAnchor);
            EditorGUI.DrawRect(new Rect(rectBackground.x, rectAnchor.y, rectBackground.width, 1), colorAnchor);
        }
        EditorGUI.DrawRect(rectAnchor, Color.red);

        Vector2 pivot = rectTransform.pivot;
        pivot.y = 1 - pivot.y;
        Vector2 rectPoint = this.GetRelativePoint(rectBackground, pivot);
        Vector2 pivotRadius = new Vector2(3, 3);
        Vector2 position = rectPoint - pivotRadius;
        Vector2 size = pivotRadius * 2;
        Rect rectPivot = new Rect(position.x, position.y, size.x, size.y);
        EditorGUI.DrawRect(rectPivot, Color.cyan);
    }

    private Rect GetRelativeMinMaxRect(Rect rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        Vector2 min = this.GetRelativePoint(rect, anchorMin);
        Vector2 max = this.GetRelativePoint(rect, anchorMax);
        Vector2 size = max - min;
        Rect result = new Rect(min.x, min.y, size.x, size.y);
        if (result.height < 0)
        {
            result.y += result.height;
            result.height = Mathf.Abs(result.height);
        }
        return result;
    }

    private Vector2 GetRelativePoint(Rect rect, Vector2 point)
    {
        return rect.position + Vector2.Scale(rect.size, point);
    }
}

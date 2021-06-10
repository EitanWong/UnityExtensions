using HierarchyPro.Runtime;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace HierarchyPro.Editor
{
    public class ObjectCustomizationShelf : IHierarchyShelf
    {
        HierarchyCanvas canvas;

        public void Canvas(HierarchyCanvas canvas) => this.canvas = canvas;

        public VisualElement CreateShelfElement()
        {
            VisualElement shelfButton = new VisualElement();
            shelfButton.name = nameof(OpenSettings);
            shelfButton.tooltip = "";
            shelfButton.StyleHeight(23);
            shelfButton.StyleJustifyContent(Justify.Center);
            Color c = EditorGUIUtility.isProSkin ? new Color32(32, 32, 32, 255) : new Color32(128, 128, 128, 255);
            shelfButton.StyleBorderColor(c);
            shelfButton.StyleBorderWidth(0, 0, 1, 0);

            shelfButton.Add(new Label("自定义选中对象"));

            shelfButton.RegisterCallback<MouseDownEvent>((evt) =>
            {
                var isPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null ? true : false;
                if (isPrefabMode)
                {
                    Debug.LogWarning("无法在预制体中 自定义对象.");
                    evt.StopPropagation();
                    return;
                }

                if (Application.isPlaying)
                {
                    Debug.LogWarning("无法在播放模式下 自定义对象.");
                    evt.StopPropagation();
                    return;
                }
                
                if (Selection.gameObjects.Length == 1 && Selection.activeGameObject != null)
                {
                    ObjectCustomizationPopup.ShowPopup(Selection.activeGameObject);
                }
                else
                {
                    if (Selection.gameObjects.Length > 1)
                    {
                        Debug.LogWarning("一次只允许自定义一个选中对象.");
                    }
                    else
                        Debug.LogWarning("没有对象被选中.");
                }

                evt.StopPropagation();
            });

            shelfButton.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                shelfButton.StyleBackgroundColor(new Color(.5f, .5f, .5f, .5f));
            });

            shelfButton.RegisterCallback<MouseLeaveEvent>((evt) => { shelfButton.StyleBackgroundColor(Color.clear); });

            return shelfButton;
        }

        public int ShelfPriority()
        {
            return 98;
        }
    }

    public class ObjectCustomizationPopup : EditorWindow
    {
        static EditorWindow window;
        GameObject gameObject;
        HierarchyLocalData hierarchyLocalData;

        public static ObjectCustomizationPopup ShowPopup(GameObject gameObject)
        {
            if (Selection.gameObjects.Length > 1 || Selection.activeGameObject == null)
                return null;

            if (window == null)
                window = ObjectCustomizationPopup.GetWindow<ObjectCustomizationPopup>(gameObject.name);
            else
            {
                window.Close();
                window = ObjectCustomizationPopup.GetWindow<ObjectCustomizationPopup>(gameObject.name);
            }

            Vector2 v2 = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Vector2 size = new Vector2(256, 140);
            window.position = new Rect(v2.x, v2.y, size.x, size.y);
            window.maxSize = window.minSize = size;
            window.ShowPopup();
            window.Focus();

            ObjectCustomizationPopup objectCustomizationPopup = window as ObjectCustomizationPopup;
            return objectCustomizationPopup;
        }

        void OnEnable()
        {
            rootVisualElement.StyleMargin(4, 4, 2, 0);

            hierarchyLocalData = HierarchyEditor.Instance.GetHierarchyLocalData(Selection.activeGameObject.scene);
            gameObject = Selection.activeGameObject;
            Selection.activeGameObject = null;

            CustomRowItem customRowItem = null;
            if (hierarchyLocalData.TryGetCustomRowData(gameObject, out customRowItem) == false)
            {
                customRowItem = hierarchyLocalData.CreateCustomRowItemFor(gameObject);
            }


            IMGUIContainer iMGUIContainer = new IMGUIContainer(() =>
            {
                customRowItem.useBackground = EditorGUILayout.Toggle("修改 背景", customRowItem.useBackground);
                if (customRowItem.useBackground)
                {
                    customRowItem.backgroundStyle = (CustomRowItem.BackgroundStyle)EditorGUILayout.EnumPopup("背景风格", customRowItem.backgroundStyle);
                    customRowItem.backgroundMode = (CustomRowItem.BackgroundMode)EditorGUILayout.EnumPopup("背景模式", customRowItem.backgroundMode);
                    customRowItem.backgroundColor = EditorGUILayout.ColorField("背景颜色", customRowItem.backgroundColor);
                }


                
                customRowItem.overrideLabel = EditorGUILayout.Toggle("修改 标签", customRowItem.overrideLabel);

                if (customRowItem.overrideLabel)
                {
                    var wideMode = EditorGUIUtility.wideMode;
                    EditorGUIUtility.wideMode = true;
                    customRowItem.labelOffset = EditorGUILayout.Vector2Field("标签 偏移", customRowItem.labelOffset);
                    EditorGUIUtility.wideMode = wideMode;
                    customRowItem.labelColor = EditorGUILayout.ColorField("标签 颜色", customRowItem.labelColor);
                }
                
                if (GUI.changed)
                    EditorApplication.RepaintHierarchyWindow();
            });
            rootVisualElement.Add(iMGUIContainer);
        }
    }
}
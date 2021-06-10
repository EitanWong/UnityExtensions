using ToolbarExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extensions.ToolbarExtensions.Editor.Extenders
{
    [InitializeOnLoad]
    static class ToolbarExTimeScaleSlider
    {
        private static Slider timeScaleElement;

        static ToolbarExTimeScaleSlider()
        {
            timeScaleElement = new Slider(0f, 1f);
            timeScaleElement.style.width = 150f;
            timeScaleElement.value = Time.timeScale;
            timeScaleElement.RegisterCallback<ChangeEvent<float>>(onTimeScaleSliderValueChange);


            timeScaleElement.style.flexDirection = FlexDirection.RowReverse;
            timeScaleElement.label = Time.timeScale.ToString();
            timeScaleElement.labelElement.style.minWidth = 50;
            timeScaleElement.labelElement.style.paddingLeft = 10;

            VisualElement iconVE = new VisualElement();
            iconVE.AddToClassList("unity-editor-toolbar-element__icon");
            //"d_UnityEditor.AnimationWindow@2x"
            //"d_Profiler.Record"
            var icon = Background.FromTexture2D(EditorGUIUtility.FindTexture("d_UnityEditor.AnimationWindow@2x"));
            iconVE.style.backgroundImage = icon;
            iconVE.style.height = 16;
            iconVE.style.width = 16;
            timeScaleElement.style.marginLeft = 0;
            timeScaleElement.Add(iconVE);

            //timeScaleElement.Add(container);
            var e = ToolbarElement.Create(timeScaleElement, ExtenderType.Right);
            ToolbarExtender.ToolbarExtend(e);
        }

        private static void onTimeScaleSliderValueChange(ChangeEvent<float> evt)
        {
            Time.timeScale = evt.newValue;
            //timeScaleElement.label = string.Format("时间缩放:{0}", Time.timeScale);
            var views = SceneView.sceneViews;
            var sceneView = (SceneView) views[0];
            if (sceneView)
            {
                sceneView.Focus();
                sceneView.ShowNotification(new GUIContent(string.Format("时间缩放\nx{0}", Time.timeScale)), 1f);
            }

            timeScaleElement.label = Time.timeScale.ToString();
        }
    }
}
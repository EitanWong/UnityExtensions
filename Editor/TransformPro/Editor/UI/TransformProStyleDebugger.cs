namespace TransformPro.Scripts
{
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class TransformProStyleDebugger
    {
        public static string OutputStyle(GUIStyle style)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(style.name);
            stringBuilder.AppendLine("");

            stringBuilder.AppendLine("   Padding:  " + style.padding);
            stringBuilder.AppendLine("   Margin:   " + style.margin);
            stringBuilder.AppendLine("   Border:   " + style.border);
            stringBuilder.AppendLine("   Overflow: " + style.overflow);
            stringBuilder.AppendLine("");

            stringBuilder.AppendLine("   Font:       " + style.font);
            stringBuilder.AppendLine("   FontSize:   " + style.fontSize);
            stringBuilder.AppendLine("   FontStyle:  " + style.fontStyle);
            stringBuilder.AppendLine("   LineHeight: " + style.lineHeight);
            stringBuilder.AppendLine("   RichText:   " + style.richText);
            stringBuilder.AppendLine("   WordWrap:   " + style.wordWrap);
            stringBuilder.AppendLine("");

            stringBuilder.AppendLine("   Alignment:     " + style.alignment);
            stringBuilder.AppendLine("   Clipping:      " + style.clipping);
            stringBuilder.AppendLine("   ContentOffset: " + style.contentOffset);
            stringBuilder.AppendLine("   ImagePosition: " + style.imagePosition);
            stringBuilder.AppendLine("");

            stringBuilder.AppendLine("   FixedWidth:        " + style.fixedWidth);
            stringBuilder.AppendLine("   StretchWidth:      " + style.stretchWidth);
            stringBuilder.AppendLine("   FixedHeight:       " + style.fixedHeight);
            stringBuilder.AppendLine("   StretchHeight:     " + style.stretchHeight);
            stringBuilder.AppendLine("   heightDependWidth: " + style.isHeightDependantOnWidth);
            stringBuilder.AppendLine("");

            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.normal));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.onNormal));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.active));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.onActive));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.hover));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.onHover));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.focused));
            stringBuilder.AppendLine(TransformProStyleDebugger.OutputStyleState(style.onFocused));

            return stringBuilder.ToString();
        }

        public static string OutputStyleState(GUIStyleState state)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(state + ":");
            stringBuilder.AppendLine("   Background:  " + (state.background != null ? state.background.name : ""));
            stringBuilder.AppendLine("   TextColor:   " + state.textColor);
#if UNITY_5_4_OR_NEWER
            stringBuilder.AppendLine("   ScaledBacks: " + string.Join(", ", state.scaledBackgrounds.Select(x => x.name).ToArray()));
#endif

            return stringBuilder.ToString();
        }

        private static void Output()
        {
            Debug.Log(TransformProStyleDebugger.OutputStyle(EditorStyles.miniButton));
            Debug.Log(TransformProStyleDebugger.OutputStyle(EditorStyles.miniButtonLeft));
            Debug.Log(TransformProStyleDebugger.OutputStyle(EditorStyles.miniButtonMid));
            Debug.Log(TransformProStyleDebugger.OutputStyle(EditorStyles.miniButtonRight));
        }
    }
}

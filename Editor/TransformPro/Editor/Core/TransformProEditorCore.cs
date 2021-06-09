namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     GUI Functions for the core transform fields.
    ///     TransformPro no longer uses Vector3 fields, instead using seperate float fields.
    /// </summary>
    public static class TransformProEditorCore
    {
        /// <summary>
        ///     Caches the original field width so it can be reset.
        /// </summary>
        private static float fieldWidth;

        /// <summary>
        ///     Caches the original label width so it can be reset.
        /// </summary>
        private static float labelWidth;

        /// <summary>
        ///     Draws a single axis float field.
        ///     This method operates with no provided value, so the field is disabled.
        /// </summary>
        /// <param name="axis">The axis to draw. Value should be 'x', 'y' or 'z'.</param>
        public static void DrawAxis(char axis)
        {
            GUI.enabled = false;
            string label = axis.ToString().ToUpper();
            TransformProEditorCore.Setup(label);
            EditorGUILayout.TextField(label, string.Empty);
            TransformProEditorCore.Reset();
        }

        /// <summary>
        ///     Draws a single axis float field.
        ///     This method operates with a single fixed value, and returns a single value via boxing.
        /// </summary>
        /// <param name="axis">The axis to draw. Value should be 'x', 'y' or 'z'.</param>
        /// <param name="value">The value of the field. Any changes will be boxed and returned via the ref keyword.</param>
        /// <returns>A value indicating if the field was changed or not.</returns>
        public static bool DrawAxis(char axis, ref float value)
        {
            string label = axis.ToString().ToUpper();
            TransformProEditorCore.Setup(label);

            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(label, value);
            bool changed = EditorGUI.EndChangeCheck();

            TransformProEditorCore.Reset();

            return changed;
        }

        /// <summary>
        ///     Draws a single axis float field.
        ///     This method operates with multiple different values, and returns a single value via boxing.
        ///     If all the items in the collection are identical, that single value will be shown in the field.
        ///     If multiple different values are present, the mixed value flag will be set, and the field will show a dash.
        ///     On change, the single value entered will be boxed and returned via the out keyword.
        /// </summary>
        /// <param name="axis">The axis to draw. Value should be 'x', 'y' or 'z'.</param>
        /// <param name="valuesIn">A collection of multiple input values.</param>
        /// <param name="valueOut">The value of the field. Any changes will be boxed and returned via the out keyword.</param>
        /// <returns>A value indicating if the field was changed or not.</returns>
        public static bool DrawAxis(char axis, ICollection<float> valuesIn, out float valueOut)
        {
            if (valuesIn == null)
            {
                valuesIn = new float[0];
            }

            string label = axis.ToString().ToUpper();
            bool mixed = valuesIn.Mixed();
            float value = valuesIn.Count > 0 ? valuesIn.First() : 0;

            TransformProEditorCore.Setup(label);

            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            valueOut = EditorGUILayout.FloatField(label, value);
            EditorGUI.showMixedValue = false;
            bool changed = EditorGUI.EndChangeCheck();

            TransformProEditorCore.Reset();

            return changed;
        }

        /// <summary>
        ///     Draws a float field for a single scalar value, representing all three axes. Does not have a label.
        ///     This method operates with no provided value, so the field is disabled.
        /// </summary>
        public static void DrawAxisScalar()
        {
            GUI.enabled = false;
            TransformProEditorCore.Setup();
            EditorGUILayout.TextField(" ", string.Empty);
            TransformProEditorCore.Reset();
        }

        /// <summary>
        ///     Draws a float field for a single scalar value, representing all three axes. Does not have a label.
        ///     This method operates with a single fixed value, and returns a single value via boxing.
        /// </summary>
        /// <param name="value">The value of the field. Any changes will be boxed and returned via the ref keyword.</param>
        /// <returns>A value indicating if the field was changed or not.</returns>
        public static bool DrawAxisScalar(ref float value)
        {
            TransformProEditorCore.Setup();

            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(" ", value);
            bool changed = EditorGUI.EndChangeCheck();

            TransformProEditorCore.Reset();

            return changed;
        }

        /// <summary>
        ///     Draws a float field for a single scalar value, representing all three axes. Does not have a label.
        ///     This method operates with multiple different values, and returns a single value via boxing.
        ///     If all the items in the collection are identical, that single value will be shown in the field.
        ///     If multiple different values are present, the mixed value flag will be set, and the field will show a dash.
        ///     On change, the single value entered will be boxed and returned via the out keyword.
        /// </summary>
        /// <param name="valuesIn">A collection of multiple input values.</param>
        /// <param name="valueOut">The value of the field. Any changes will be boxed and returned via the out keyword.</param>
        /// <returns>A value indicating if the field was changed or not.</returns>
        public static bool DrawAxisScalar(ICollection<float> valuesIn, out float valueOut)
        {
            if (valuesIn == null)
            {
                valuesIn = new float[0];
            }

            bool mixed = valuesIn.Mixed();
            float value = valuesIn.Count > 0 ? valuesIn.First() : 0;

            TransformProEditorCore.Setup();

            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            valueOut = EditorGUILayout.FloatField(" ", value);
            EditorGUI.showMixedValue = false;
            bool changed = EditorGUI.EndChangeCheck();

            TransformProEditorCore.Reset();

            return changed;
        }

        /// <summary>
        ///     Draws a single set of 'nudge' buttons, spaced to match the layout of the three axis based fields.
        /// </summary>
        /// <returns>
        ///     An int value representing which button was pressed.
        ///     Starting from 1, these are X-, X+, Y-, Y+, Z-, Z+.
        ///     A value of zero means no button was pressed.
        /// </returns>
        public static int DrawNudges()
        {
            int result = 0;
            GUI.color = TransformProStyles.ColorAxisX;
            if (GUILayout.Button("-", TransformProStyles.Buttons.TextTiny.Left))
            {
                result = 1;
            }
            if (GUILayout.Button("+", TransformProStyles.Buttons.TextTiny.Right))
            {
                result = 2;
            }

            GUILayout.Label(" ", TransformProStyles.LabelTiny, GUILayout.Width(11));
            GUI.color = TransformProStyles.ColorAxisY;
            if (GUILayout.Button("-", TransformProStyles.Buttons.TextTiny.Left))
            {
                result = 3;
            }
            if (GUILayout.Button("+", TransformProStyles.Buttons.TextTiny.Right))
            {
                result = 4;
            }

            GUILayout.Label(" ", TransformProStyles.LabelTiny, GUILayout.Width(11));
            GUI.color = TransformProStyles.ColorAxisZ;
            if (GUILayout.Button("-", TransformProStyles.Buttons.TextTiny.Left))
            {
                result = 5;
            }
            if (GUILayout.Button("+", TransformProStyles.Buttons.TextTiny.Right))
            {
                result = 6;
            }

            GUI.color = Color.white;
            return result;
        }

        /// <summary>
        ///     Restes the modified global and skin settings to their defaults.
        /// </summary>
        private static void Reset()
        {
            EditorGUIUtility.labelWidth = TransformProEditorCore.labelWidth;
            EditorGUIUtility.fieldWidth = TransformProEditorCore.fieldWidth;
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        /// <summary>
        ///     Store some current global and skin settings, and reset them to make the fields take up as little unecassary space
        ///     as possible.
        /// </summary>
        private static void Setup()
        {
            TransformProEditorCore.labelWidth = EditorGUIUtility.labelWidth;
            TransformProEditorCore.fieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 14;
            EditorGUIUtility.fieldWidth = 28;
        }

        /// <summary>
        ///     Store some current global and skin settings, and reset them to make the fields take up as little unecassary space
        ///     as possible.
        ///     Also updates the field color based on the axis requested.
        /// </summary>
        /// <param name="label">The axis to draw. Value should be 'x', 'y' or 'z'.</param>
        private static void Setup(string label)
        {
            TransformProEditorCore.Setup();
            GUI.backgroundColor = label == "X" ? TransformProStyles.ColorAxisXLight : label == "Y" ? TransformProStyles.ColorAxisYLight : TransformProStyles.ColorAxisZLight;
        }
    }
}

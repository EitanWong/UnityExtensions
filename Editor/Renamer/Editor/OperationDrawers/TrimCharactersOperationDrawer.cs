﻿/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RenamerExtension.Renamer
{
    using UnityEngine;
    using UnityEditor;

    public class TrimCharactersOperationDrawer : RenameOperationDrawer<TrimCharactersOperation>
    {
        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return GetOperationPath("delete", "trimCharacters");
            }
        }

        /// <summary>
        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        public override string HeadingLabel
        {
            get
            {
                return LocalizationManager.Instance.GetTranslation("trimCharacters");
            }
        }

        /// <summary>
        /// Gets the color to use for highlighting the operation.
        /// </summary>
        /// <value>The color of the highlight.</value>
        public override Color32 HighlightColor
        {
            get
            {
                return this.DeleteColor;
            }
        }

        /// <summary>
        /// Gets the name of the control to focus when this operation is focused
        /// </summary>
        /// <value>The name of the control to focus.</value>
        public override string ControlToFocus
        {
            get
            {
                return LocalizationManager.Instance.GetTranslation("deleteFromFront");
            }
        }

        /// <summary>
        /// Gets the preferred height for the contents of the operation.
        /// This allows inherited operations to specify their height.
        /// </summary>
        /// <returns>The preferred height for contents.</returns>
        protected override float GetPreferredHeightForContents()
        {
            return this.CalculateGUIHeightForLines(2);
        }

        /// <summary>
        /// Draws the contents of the Rename Op.
        /// </summary>
        /// <param name="controlPrefix">The prefix of the control to assign to the control names</param>
        protected override void DrawContents(Rect operationRect, int controlPrefix)
        {
            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, LocalizationManager.Instance.GetTranslation("deleteFromFront")));
            this.RenameOperation.NumFrontDeleteChars = EditorGUI.IntField(
                operationRect.GetSplitVertical(1, 2, LineSpacing),
                LocalizationManager.Instance.GetTranslation("deleteFromFront"),
                this.RenameOperation.NumFrontDeleteChars);
            this.RenameOperation.NumFrontDeleteChars = Mathf.Max(0, this.RenameOperation.NumFrontDeleteChars);

            GUI.SetNextControlName(GUIControlNameUtility.CreatePrefixedName(controlPrefix, LocalizationManager.Instance.GetTranslation("deleteFromBack")));
            this.RenameOperation.NumBackDeleteChars = EditorGUI.IntField(
                operationRect.GetSplitVertical(2, 2, LineSpacing),
                LocalizationManager.Instance.GetTranslation("deleteFromBack"),
                this.RenameOperation.NumBackDeleteChars);
            this.RenameOperation.NumBackDeleteChars = Mathf.Max(0, this.RenameOperation.NumBackDeleteChars);
        }
    }
}
namespace TransformPro.Scripts
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class TransformProStylesButtons
    {
        private static readonly int fontSize = 8;
        private static readonly int height = 16;

        private Button icon;
        private Button iconTint;
        private Button iconPadded;
        private Button textTiny;
        private Button iconBold;
        private Button iconLarge;
        private Button standard;
        private Button tiny;

        public TransformProStylesButtons()
        {
            this.standard = new Button(delegate(GUIStyle style)
                                       {
                                           style.fontSize = TransformProStylesButtons.fontSize;
                                           style.fontStyle = EditorGUIUtility.isProSkin ? FontStyle.Bold : FontStyle.Normal;
                                           style.fixedHeight = TransformProStylesButtons.height;
                                           return style;
                                       });

            this.tiny = new Button(delegate(GUIStyle style)
                                   {
                                       style.fontSize = 8;
                                       style.fixedHeight = 12;
                                       return style;
                                   },
                                   TransformProStylesButtonTextures.GetTinySingle(),
                                   TransformProStylesButtonTextures.GetTinyLeft(),
                                   TransformProStylesButtonTextures.GetTinyMiddle(),
                                   TransformProStylesButtonTextures.GetTinyRight());

            this.icon = new Button(delegate(GUIStyle style)
                                   {
                                       style.fixedHeight = 16;
                                       style.padding = new RectOffset(0, 0, 0, 0);
                                       style.fontStyle = FontStyle.Normal;
                                       return style;
                                   },
                                   this.Standard);

            this.iconTint = new Button(delegate(GUIStyle style)
                                       {
                                           style.normal.textColor = Color.white;
                                           style.fontStyle = FontStyle.Bold;
                                           return style;
                                       },
                                       this.Icon);

            this.iconPadded = new Button(delegate(GUIStyle style)
                                         {
                                             style.fixedHeight = 20;
                                             style.padding = new RectOffset(2, 2, 2, 2);
                                             style.fontStyle = FontStyle.Normal;
                                             return style;
                                         },
                                         this.Standard);

            this.iconBold = new Button(delegate(GUIStyle style)
                                       {
                                           style.fixedHeight = 16;
                                           style.padding = new RectOffset(0, 0, 0, 0);
                                           style.fontStyle = FontStyle.Bold;
                                           return style;
                                       },
                                       this.Standard);

            this.iconLarge = new Button(delegate(GUIStyle style)
                                        {
                                            style.fixedHeight = 20;
                                            style.padding = new RectOffset(1, 1, 1, 1);
                                            style.fontStyle = FontStyle.Normal;
                                            return style;
                                        },
                                        this.Standard);

            this.textTiny = new Button(delegate(GUIStyle style)
                                       {
                                           style.fixedHeight = 12;
                                           style.padding = new RectOffset(0, 0, 0, 0);
                                           style.fontSize = 8;
                                           style.fontStyle = FontStyle.Normal;
                                           return style;
                                       },
                                       this.Standard);
        }

        public Button Icon { get { return this.icon; } }
        public Button IconBold { get { return this.iconBold; } }
        public Button IconLarge { get { return this.iconLarge; } }
        public Button IconPadded { get { return this.iconPadded; } }
        public Button IconTint { get { return this.iconTint; } }
        public Button Standard { get { return this.standard; } }
        public Button TextTiny { get { return this.textTiny; } }
        public Button Tiny { get { return this.tiny; } }

        public class Button
        {
            private readonly GUIStyle left;
            private readonly GUIStyle middle;
            private readonly GUIStyle right;
            private readonly GUIStyle single;

            public Button(Func<GUIStyle, GUIStyle> function)
                : this(
                       function,
                       TransformProStylesButtonTextures.GetStyleSingle(),
                       TransformProStylesButtonTextures.GetStyleLeft(),
                       TransformProStylesButtonTextures.GetStyleMiddle(),
                       TransformProStylesButtonTextures.GetStyleRight())
            {
            }

            public Button(Func<GUIStyle, GUIStyle> function, Button button)
                : this(function, button.Single, button.Left, button.Middle, button.Right)
            {
            }

            public Button(Func<GUIStyle, GUIStyle> function, GUIStyle single, GUIStyle left, GUIStyle middle, GUIStyle right)
            {
                this.single = function(new GUIStyle(single));
                this.left = function(new GUIStyle(left));
                this.middle = function(new GUIStyle(middle));
                this.right = function(new GUIStyle(right));
            }

            public GUIStyle Left { get { return this.left; } }

            public GUIStyle Middle { get { return this.middle; } }

            public GUIStyle Right { get { return this.right; } }

            public GUIStyle Single { get { return this.single; } }
        }
    }
}

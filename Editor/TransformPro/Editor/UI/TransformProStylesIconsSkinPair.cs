namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     A pair of textures that automaticaklly switches based on the current string.
    ///     Both are loaded for simplicity.
    /// </summary>
    public struct TransformProStylesIconsSkinPair
    {
        private Texture personal;
        private Texture professional;

        public TransformProStylesIconsSkinPair(Texture single)
        {
            this.personal = single;
            this.professional = single;
        }

        public TransformProStylesIconsSkinPair(Texture personal, Texture professional)
        {
            this.personal = personal;
            this.professional = professional;
        }

        /// <summary>
        ///     The <see cref="Texture" /> to display. The correct version based on the current skin will automatically be
        ///     provided.
        /// </summary>
        public Texture Texture { get { return EditorGUIUtility.isProSkin ? this.professional : this.personal; } }
    }
}

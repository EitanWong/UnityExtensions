namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Implement this interface to create a Gadget that provide a standard UI panel.
    ///     Can be used in conjuction with the other Gadget interfaces as required.
    /// </summary>
    public interface ITransformProGadgetPanel
        : ITransformProGadget
    {
        /// <summary>
        ///     What height should the panel render as?
        ///     A planned update will render this propery obsolete. Instead we will replicate the internet GUI system and measure
        ///     the UI when the Layout mesage is passed.
        /// </summary>
        float Height { get; }

        /// <summary>
        ///     The position in the list of panels to draw this gadget.
        ///     This will be replaced with a system the preferences can tie into so the gadgets can be reordered manually.
        /// </summary>
        int Sort { get; }

        /// <summary>
        ///     Draws the GUI for the current Gadget Panel.
        /// </summary>
        /// <param name="sceneView">The current SceneView object being drawn to.</param>
        /// <param name="gadgets">The Gadgets manager that is drawing this panel.</param>
        /// <param name="rect">
        ///     The screen rectangle that the GUI is being drawn to.
        ///     A planned update will instead allow you to use the layout system to make this process simpler.
        /// </param>
        void DrawPanelGUI(SceneView sceneView, TransformProEditorGadgets gadgets, Rect rect);
    }
}

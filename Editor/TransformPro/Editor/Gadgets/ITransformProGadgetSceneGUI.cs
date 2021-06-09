namespace TransformPro.Scripts
{
    using UnityEditor;

    /// <summary>
    ///     Implement this inteface on a Gadget to draw GUI elements to the scene view outside the structured panel system.
    ///     Can be used in conjuction with the other Gadget interfaces as required.
    /// </summary>
    public interface ITransformProGadgetSceneGUI
        : ITransformProGadget
    {
        /// <summary>
        ///     Use this method to draw to the scene.
        /// </summary>
        /// <param name="sceneView">The current SceneView object being drawn to.</param>
        /// <param name="gadgets">The Gadgets manager that is drawing this panel.</param>
        void DrawSceneGUI(SceneView sceneView, TransformProEditorGadgets gadgets);
    }
}

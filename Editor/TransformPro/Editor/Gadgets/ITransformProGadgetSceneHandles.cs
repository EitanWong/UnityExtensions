namespace TransformPro.Scripts
{
    using UnityEditor;

    /// <summary>
    ///     Implement this interface to draw to the scene using the Handles system.
    ///     Can be used in conjuction with the other Gadget interfaces as required.
    /// </summary>
    public interface ITransformProGadgetSceneHandles
        : ITransformProGadget
    {
        /// <summary>
        ///     Use this method to draw to the scene using the Handles system.
        /// </summary>
        /// <param name="sceneView">The current SceneView object being drawn to.</param>
        /// <param name="gadgets">The Gadgets manager that is drawing this panel.</param>
        void DrawSceneHandles(SceneView sceneView, TransformProEditorGadgets gadgets);
    }
}

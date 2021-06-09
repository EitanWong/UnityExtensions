namespace TransformPro.Scripts
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Implement this inteface on a Gadget to get events raised when the user clicks within the scene.
    /// </summary>
    public interface ITransformProGadgetSceneClick : ITransformProGadget
    {
        /// <summary>
        ///     This method is invoked when the user clicks within the scene.
        /// </summary>
        /// <param name="sceneView">The current SceneView object being drawn to.</param>
        /// <param name="gadgets">The Gadgets manager that is drawing this panel.</param>
        /// <param name="mousePosition">The screen position of the click event.</param>
        /// <param name="executeClick">
        ///     This value is only true once per click event.
        ///     You should use it to handle the event as a singular click event.
        /// </param>
        /// <returns>
        ///     A value indicating whether the method handled a click sucessfully. Used to prevent events cascading.
        /// </returns>
        bool SceneClick(SceneView sceneView, TransformProEditorGadgets gadgets, Vector2 mousePosition, bool executeClick);
    }
}

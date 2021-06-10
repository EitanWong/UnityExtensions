namespace TransformPro.Scripts
{
    /// <summary>
    ///     The English string provider.
    /// </summary>
    public class TransformProStringsEnglish : TransformProStringsBase
    {
        /// <inheritdoc />
        public override string BoundsDrawCallWarning { get { return "Too many root Transform objects are selected to draw all their bounds to the scene view.\n\nYou can adjust the limit in the TransformPro preferences."; } }

        /// <inheritdoc />
        public override string CreateBoxCollider { get { return "Creates a box collider to fit current local bounds."; } }

        /// <inheritdoc />
        public override string CreateCapsuleCollider { get { return "Creates a capsule collider to fit the current local bounds."; } }

        /// <inheritdoc />
        public override string TooltipCopyPastePosition { get { return "Copy the currently displayed value for the position only.\n\nTo paste this value shift click or right click this button."; } }

        /// <inheritdoc />
        public override string TooltipCopyPasteRotation { get { return "Copy the currently displayed value for the rotation only.\n\nTo paste this value shift click or right click this button."; } }

        /// <inheritdoc />
        public override string TooltipCopyPasteScale { get { return "Copy the currently displayed value for the scale only.\n\nTo paste this value shift click or right click this button."; } }

        /// <inheritdoc />
        public override string TooltipCopyPasteTransform { get { return "Copy all 3 currently displayed values for the entire transform.\n\nTo paste these values shift click or right click this button."; } }

        /// <inheritdoc />
        public override string TooltipDrop { get { return "Drops the currently selected object onto the floor beneath it. Does not affect the rotation of the object."; } }

        /// <inheritdoc />
        public override string TooltipGadgetToggle { get { return "Toggles gadget visibility."; } }

        /// <inheritdoc />
        public override string TooltipGround { get { return "Drops the currently selected object onto the floor beneath it, and also re-orients the object to sit flat against it."; } }

        /// <inheritdoc />
        public override string TooltipLookAt { get { return "Reorients this object to be looking at the current clipboard position."; } }

        /// <inheritdoc />
        public override string TooltipPreferences { get { return "Show the preferences window, containing settings such as the current grid size and offset.\n\nYou can also disable the automatic bounds calculation if it is not required."; } }

        /// <inheritdoc />
        public override string TooltipQuickPositionX { get { return "Moves the object in the current space along the X axis.\n\nThe object will move by {0} grid tiles, or {1} units."; } }

        /// <inheritdoc />
        public override string TooltipQuickPositionY { get { return "Moves the object in the current space along the Y axis.\n\nThe object will move by {0} grid tiles, or {1} units."; } }

        /// <inheritdoc />
        public override string TooltipQuickPositionZ { get { return "Moves the object in the current space along the Z axis.\n\nThe object will move by {0} grid tiles, or {1} units."; } }

        /// <inheritdoc />
        public override string TooltipQuickRotationX { get { return "Rotate the object in the current space around the X axis.\n\nThe object will rotate by {0} degrees."; } }

        /// <inheritdoc />
        public override string TooltipQuickRotationY { get { return "Rotate the object in the current space around the Y axis.\n\nThe object will rotate by {0} degrees."; } }

        /// <inheritdoc />
        public override string TooltipQuickRotationZ { get { return "Rotate the object in the current space around the Z axis.\n\nThe object will rotate by {0} degrees."; } }

        /// <inheritdoc />
        public override string TooltipResetPosition { get { return "Resets the position of the currently selected object in the selected space to (0, 0, 0)."; } }

        /// <inheritdoc />
        public override string TooltipResetPositionX { get { return "Resets the position X axis of the currently selected object in the selected space to 0."; } }

        /// <inheritdoc />
        public override string TooltipResetPositionY { get { return "Resets the position Y axis of the currently selected object in the selected space to 0."; } }

        /// <inheritdoc />
        public override string TooltipResetPositionZ { get { return "Resets the position Z axis of the currently selected object in the selected space to 0."; } }

        /// <inheritdoc />
        public override string TooltipResetRotation { get { return "Resets the rotation of the currently selected object in the selected space to (0, 0, 0) / Quaternion.identity."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationRndX { get { return "Randomise the euler rotation X axis of the currently selected object in the selected space."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationRndY { get { return "Randomise the euler rotation Y axis of the currently selected object in the selected space."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationRndZ { get { return "Randomise the euler rotation Z axis of the currently selected object in the selected space."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationX { get { return "Resets the euler rotation X axis of the currently selected object in the selected space to 0 degrees."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationY { get { return "Resets the euler rotation Y axis of the currently selected object in the selected space to 0 degrees."; } }

        /// <inheritdoc />
        public override string TooltipResetRotationZ { get { return "Resets the euler rotation Z axis of the currently selected object in the selected space to 0 degrees."; } }

        /// <inheritdoc />
        public override string TooltipResetScale { get { return "Resets the scale of the currently selected object in the selected space to (1, 1, 1)."; } }

        /// <inheritdoc />
        public override string TooltipResetTransform { get { return "Resets the entire transform of the currently selected object in the selected space.\n\nThe position value will be set to (0, 0, 0)\nThe rotation value will be set to (0, 0, 0) / Quaternion.identity\nThe scale value will be set to (1, 1, 1)"; } }

        /// <inheritdoc />
        public override string TooltipSnapPosition { get { return "Snap the position of the current transform to the grid."; } }

        /// <inheritdoc />
        public override string TooltipSnapRotation { get { return "Snap the rotation of the current transform to the grid."; } }

        /// <inheritdoc />
        public override string TooltipSnapTransform { get { return "Snap the position and rotation of the current transform to the grid."; } }

        /// <inheritdoc />
        public override string TooltipSpaceLocal { get { return "The transform will display and operate in local space, where the origin represents the transform of the parent."; } }

        /// <inheritdoc />
        public override string TooltipSpaceWorld { get { return "The transform will display and operate in world space, where the origin represents the global origin."; } }

        /// <inheritdoc />
        public override string TooltipVisualiseBoundsSpaceMode { get { return "Switches bounds visualisation between local and world space."; } }

        /// <inheritdoc />
        public override string TooltipVisualiseColliders { get { return "Toggles world space visualisation of overall Renderer bounds in the scene view."; } }

        /// <inheritdoc />
        public override string TooltipVisualiseRenderers { get { return "Toggles world space visualisation of overall Collider bounds in the scene view."; } }
    }
}

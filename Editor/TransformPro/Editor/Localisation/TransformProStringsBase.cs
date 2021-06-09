
#pragma warning disable 1574

namespace TransformPro.Scripts
{
    /// <summary>
    ///     The base string provider. Exposes the full list of strings the system uses.
    /// </summary>
    public abstract class TransformProStringsBase
    {
        /// <summary>Warning shown when too many root bounds objects are selected. Limit can be adjusted in the preferences.</summary>
        public abstract string BoundsDrawCallWarning { get; }

        /// <summary>Warning shown when too many root bounds objects are selected. Limit can be adjusted in the preferences.</summary>
        public abstract string CreateBoxCollider { get; }

        /// <summary>Warning shown when too many root bounds objects are selected. Limit can be adjusted in the preferences.</summary>
        public abstract string CreateCapsuleCollider { get; }

        /// <summary>The tooltip for the Copy/Paste Position button.</summary>
        public abstract string TooltipCopyPastePosition { get; }

        /// <summary>The tooltip for the Copy/Paste Rotation button.</summary>
        public abstract string TooltipCopyPasteRotation { get; }

        /// <summary>The tooltip for the Copy/Paste Scale button.</summary>
        public abstract string TooltipCopyPasteScale { get; }

        /// <summary>The tooltip for the Copy/Paste Transform button.</summary>
        public abstract string TooltipCopyPasteTransform { get; }

        /// <summary>The tooltip for the Grounding module Drop button.</summary>
        public abstract string TooltipDrop { get; }

        /// <summary>Toggles all gadgets visibility on and off.</summary>
        public abstract string TooltipGadgetToggle { get; }

        /// <summary>The tooltip for the Grounding module Ground button.</summary>
        public abstract string TooltipGround { get; }

        /// <summary> The tooltip for the look at button. </summary>
        public abstract string TooltipLookAt { get; }

        /// <summary>The tooltip for the Preferences button.</summary>
        public abstract string TooltipPreferences { get; }

        /// <summary>
        ///     The tooltip for updaing the X position. Used by <see cref="string.Format" />, with two input properties - grid
        ///     multiplier and unit distance.
        /// </summary>
        public abstract string TooltipQuickPositionX { get; }

        /// <summary>
        ///     The tooltip for updaing the Y position. Used by <see cref="string.Format" />, with two input properties - grid
        ///     multiplier and unit distance.
        /// </summary>
        public abstract string TooltipQuickPositionY { get; }

        /// <summary>
        ///     The tooltip for updaing the Z position. Used by <see cref="string.Format" />, with two input properties - grid
        ///     multiplier and unit distance.
        /// </summary>
        public abstract string TooltipQuickPositionZ { get; }

        /// <summary>
        ///     The tooltip for updaing the X euler rotation. Used by <see cref="string.Format" />, with single input property -
        ///     angle
        /// </summary>
        public abstract string TooltipQuickRotationX { get; }

        /// <summary>
        ///     The tooltip for updaing the Y euler rotation. Used by <see cref="string.Format" />, with single input property
        ///     - angle
        /// </summary>
        public abstract string TooltipQuickRotationY { get; }

        /// <summary>
        ///     The tooltip for updaing the Z euler rotation. Used by <see cref="string.Format" />, with single input property
        ///     - angle
        /// </summary>
        public abstract string TooltipQuickRotationZ { get; }

        /// <summary>The tooltip for the Reset Position button.</summary>
        public abstract string TooltipResetPosition { get; }

        /// <summary>The tooltip for the Reset Position X Axis button.</summary>
        public abstract string TooltipResetPositionX { get; }

        /// <summary>The tooltip for the Reset Position Y Axis button.</summary>
        public abstract string TooltipResetPositionY { get; }

        /// <summary>The tooltip for the Reset Position Z Axis button.</summary>
        public abstract string TooltipResetPositionZ { get; }

        /// <summary>The tooltip for the Reset Rotation button.</summary>
        public abstract string TooltipResetRotation { get; }

        /// <summary>The tooltip for the Rotation Randomise X Axis button.</summary>
        public abstract string TooltipResetRotationRndX { get; }

        /// <summary>The tooltip for the Rotation Randomise Y Axis button.</summary>
        public abstract string TooltipResetRotationRndY { get; }

        /// <summary>The tooltip for the Rotation Randomise Z Axis button.</summary>
        public abstract string TooltipResetRotationRndZ { get; }

        /// <summary>The tooltip for the Reset Rotation X Axis button.</summary>
        public abstract string TooltipResetRotationX { get; }

        /// <summary>The tooltip for the Reset Rotation Y Axis button.</summary>
        public abstract string TooltipResetRotationY { get; }

        /// <summary>The tooltip for the Reset Rotation Z Axis button.</summary>
        public abstract string TooltipResetRotationZ { get; }

        /// <summary>The tooltip for the Reset Scale button.</summary>
        public abstract string TooltipResetScale { get; }

        /// <summary>The tooltip for the Reset Transform button.</summary>
        public abstract string TooltipResetTransform { get; }

        /// <summary>The tooltip for the Snap Position button.</summary>
        public abstract string TooltipSnapPosition { get; }

        /// <summary>The tooltip for the Snap Rotation button.</summary>
        public abstract string TooltipSnapRotation { get; }

        /// <summary>The tooltip for the Snap Transform button.</summary>
        public abstract string TooltipSnapTransform { get; }

        /// <summary>The tooltip for the Local Space button.</summary>
        public abstract string TooltipSpaceLocal { get; }

        /// <summary>The tooltip for the World Space button.</summary>
        public abstract string TooltipSpaceWorld { get; }

        /// <summary>The tooltip for the Bounds Visualisation Space Mode toggle.</summary>
        public abstract string TooltipVisualiseBoundsSpaceMode { get; }

        /// <summary>The tooltip for the Visualise Colliders button.</summary>
        public abstract string TooltipVisualiseColliders { get; }

        /// <summary>The tooltip for the Visualise Renderers button.</summary>
        public abstract string TooltipVisualiseRenderers { get; }
    }
}

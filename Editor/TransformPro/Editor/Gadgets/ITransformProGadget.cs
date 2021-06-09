namespace TransformPro.Scripts
{
    /// <summary>
    ///     All of the gadgets used by the new GUI system extend from this base.
    ///     It's main purpose is to allow the system to easily find a list of all gadgets, before testing them for more
    ///     specific interfaces individually.
    /// </summary>
    public interface ITransformProGadget
    {
        /// <summary>
        ///     Is the gadget enabled or not. This will be replaced with a system the preferences can tie into so the user can turn
        ///     gadgets on/off as required.
        /// </summary>
        bool Enabled { get; }
    }
}

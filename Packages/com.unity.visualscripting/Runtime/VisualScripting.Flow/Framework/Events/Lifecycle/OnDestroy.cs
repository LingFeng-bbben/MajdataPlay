namespace Unity.VisualScripting
{
    /// <summary>
    /// Called before the machine is destroyed.
    /// </summary>
    [UnitCategory("Events/Lifecycle")]
    [UnitOrder(7)]
    public sealed class OnDestroy : MachineEventUnit<EmptyEventArgs>
    {
        protected override string hookName => EventHooks.OnDestroy;

        public override void StopListening(GraphStack stack)
        {
            // StopListening is typically triggered when the object is disabled or destroyed
            // OnDestroy is a special case event where we want it to continue listening even when the object
            // is disabled. It only unregisters itself on destruction and not before.
            // That's why this method is overriden to do nothing.
        }

        private protected override void InternalTrigger(GraphReference reference, EmptyEventArgs args)
        {
            base.InternalTrigger(reference, args);

            // Stop listening for events after we're triggered (i.e when we're destroyed)
            using var stack = reference.ToStackPooled();
            base.StopListening(stack);
        }
    }
}

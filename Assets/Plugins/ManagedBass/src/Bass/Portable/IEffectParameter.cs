namespace ManagedBass
{
    /// <summary>
    /// Parameters for an Effect.
    /// </summary>
    public interface IEffectParameter
    {
        /// <summary>
        /// Gets the <see cref="EffectType"/>.
        /// </summary>
        EffectType FXType { get; }
    }
}
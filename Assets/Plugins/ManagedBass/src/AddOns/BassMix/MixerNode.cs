using System.Runtime.InteropServices;

namespace ManagedBass.Mix
{
    /// <summary>
    /// Used with <see cref="BassMix.ChannelSetEnvelope(int,MixEnvelope,MixerNode[],int)" /> to set an envelope on a mixer source channel.
    /// </summary>
    /// <remarks>
    /// <para>Envelopes are applied on top of the channel's attributes, as set via <see cref="Bass.ChannelSetAttribute(int,ChannelAttribute,float)" />. 
    /// In the case of <see cref="MixEnvelope.Frequency"/> and <see cref="MixEnvelope.Volume"/>, 
    /// the final sample rate and volume is a product of the channel attribute and the envelope. 
    /// While in the <see cref="MixEnvelope.Pan"/> case, the final panning is a sum of the channel attribute and envelope.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MixerNode
    {
        /// <summary>
        /// The postion of the node in bytes. This is based on the mixer's sample format, not the source channel's format!
        /// </summary>
        /// <remarks>Note: Envelopes deal with mixer positions, not sources!</remarks>
        public long Position;

        /// <summary>
        /// The envelope value at the position.
        /// </summary>
        public float Value;
    }
}
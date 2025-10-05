using System;

namespace ManagedBass.Mix
{
	/// <summary>
	/// Mixer envelope attribute types, used with <see cref="BassMix.ChannelSetEnvelope(int,MixEnvelope,MixerNode[],int)" />, <see cref="BassMix.ChannelGetEnvelopePosition" /> and <see cref="BassMix.ChannelSetEnvelopePosition" /> to set/retrieve an envelope on a mixer source channel.
	/// </summary>
    [Flags]
    public enum MixEnvelope
    {
        /// <summary>
        /// Sample rate.
        /// <para>Envelopes are applied on top of the channel's attributes.
        /// The final sample rate is the product of the channel attribute and the envelope.</para>
        /// </summary>
        Frequency = 1,

        /// <summary>
        /// Volume.
        /// <para>Envelopes are applied on top of the channel's attributes. 
        /// The final volume is the product of the channel attribute and the envelope.</para>
        /// </summary>
        Volume = 2,

        /// <summary>
        /// Panning/Balance.
        /// <para>Envelopes are applied on top of the channel's attributes. 
        /// The final panning is a sum of the channel attribute and envelope.</para>
        /// </summary>
        Pan = 3,

        /// <summary>
        /// Loop the envelope (flag).
        /// </summary>
        Loop = 0x10000,

        /// <summary>
        /// Remove the source from the mixer at the end of the envelope. This is a flag and can be used in combination with any of the above.
        /// </summary>
        Remove = 0x20000
    }
}
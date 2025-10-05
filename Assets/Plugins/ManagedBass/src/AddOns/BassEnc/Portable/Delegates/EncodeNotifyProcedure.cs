using System;

namespace ManagedBass.Enc
{
    /// <summary>
    /// User defined callback function to receive notifications on an encoder's status.
    /// </summary>
    /// <param name="Handle">The encoder that the notification is from (returned by <see cref="BassEnc.EncodeSetNotify" />).</param>
    /// <param name="Status">The encoder's status.</param>
    /// <param name="User">The user instance data given when <see cref="BassEnc.EncodeSetNotify" /> was called.</param>
    /// <remarks>
    /// <para>
    /// When setting a notification callback on a channel, it only applies to the encoders that are currently set on the channel.
    /// Subsequent encoders will not automatically have the notification callback set on them, this function will have to be called again to set them up.
    /// </para>
    /// <para>
    /// An encoder can only have one notification callback set.
    /// Subsequent calls of this function can be used to change the callback function, or disable notifications.
    /// </para>
    /// <para>If the encoder is already dead when setting up a notification callback, the callback will be triggered immediately.</para>
    /// <para>It is safe to call <see cref="BassEnc.EncodeStop(int)" /> to free an encoder from within a notification callback.</para>
    /// </remarks>
    public delegate void EncodeNotifyProcedure(int Handle, EncodeNotifyStatus Status, IntPtr User);
}
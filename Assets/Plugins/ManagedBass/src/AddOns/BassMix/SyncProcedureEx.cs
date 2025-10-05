using System;

namespace ManagedBass.Mix
{
    /// <summary>
	/// User defined extended mixer synchronizer callback function (see <see cref="BassMix.ChannelSetSync(int,SyncFlags,long,SyncProcedureEx,IntPtr)" /> for details).
	/// </summary>
	/// <param name="Handle">The sync handle that has occured (as returned by <see cref="BassMix.ChannelSetSync(int,SyncFlags,long,SyncProcedureEx,IntPtr)" />).</param>
	/// <param name="Channel">The channel that the sync occured on (the mixer source channel).</param>
	/// <param name="Data">Additional data associated with the sync's occurance.</param>
	/// <param name="User">The user instance data given when <see cref="BassMix.ChannelSetSync(int,SyncFlags,long,SyncProcedureEx,IntPtr)" /> was called.</param>
	/// <param name="Offset">The offset in bytes containing the position of the sync occurrence within the update cycle converted to the mixer stream.</param>
	/// <remarks>
	/// <para>
    /// A sync callback function should be very quick as other syncs can't be processed until it has finished.
    /// Attribute slides (<see cref="Bass.ChannelSlideAttribute" />) are also performed by the sync thread, so are also affected if a sync callback takes a long time.
    /// </para>
	/// <para>
    /// If the sync is a <see cref="SyncFlags.Mixtime"/> sync, then depending on the sync type, the callback will be executed in the update thread.
	/// The <paramref name="Offset" /> specifies the position of the sync within the update buffer converted to the mixer stream position.
	/// Note that the <paramref name="Offset" /> is based on the mixer's sample format, so you'll need to convert that to the source's format if using the sync to trigger things on the source.
    /// </para>
	/// <para>
    /// The usual restrictions on which BASS functions can be called that apply to stream callbacks (<see cref="StreamProcedure" />) also apply here. 
	/// It is also unsafe to call <see cref="Bass.ChannelSetSync" /> on the same channel from a mixtime sync callback.
    /// <see cref="Bass.ChannelSetPosition" /> can be used in a mixtime sync to implement custom looping, eg. set a <see cref="SyncFlags.Position"/> sync at the loop end position and seek to the loop start position in the callback.
    /// </para>
	/// </remarks>
	public delegate void SyncProcedureEx(int Handle, int Channel, int Data, IntPtr User, long Offset);
}
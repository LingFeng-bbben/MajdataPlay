using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined synchronizer callback function (see <see cref="BassDShow.ChannelSetSync" /> for details).
	/// </summary>
	/// <param name="Channel">The channel that the sync applies to.</param>
	/// <param name="Sync">The sync occurred (one of <see cref="BassDShowSync" />).</param>
	/// <param name="Data">The data associated with the sync (see <see cref="BassDShowSync" /> for details).</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.ChannelSetSync" /> was called.</param>
	public delegate void VideoSyncProcedure(int Channel, BassDShowSync Sync, int Data, IntPtr User);
}
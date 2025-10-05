using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Channel Sync type to be used with <see cref="BassDShow.ChannelSetSync" />.
	/// </summary>
	[Flags]
	public enum BassDShowSync
	{
		/// <summary>
		/// Triggered when the stream ends.
		/// <para>data: alwqays 0.</para>
		/// </summary>
		End = 1,

		/// <summary>
		/// Triggered when a new DVD chapter starts.
		/// <para>data: the new chapter index.</para>
		/// </summary>
		DvdChapterStart = 2,
		
		/// <summary>
		/// Triggered when a title change occured in a DVD stream.
		/// <para>data: new title number.</para>
		/// </summary>
		DvdTitleChange = 3,
		
		/// <summary>
		/// Triggered when an error occured on a DVD stream.
		/// <para>data: alwqays 0.</para>
		/// </summary>
		DvdError = 4,
		
		/// <summary>
		/// Triggered by video renderers when it needs a repaint. A call to <see cref="M:Un4seen.Bass.AddOn.DShow.BassDShow.BASS_DSHOW_ChannelRepaint(System.Int32,System.IntPtr,System.IntPtr)" /> should be done, but not necessary.
		/// <para>data: alwqays 0.</para>
		/// </summary>
		Repaint = 5,
		
		/// <summary>
		/// Triggered by video renderers when the video size changed.
		/// <para>data: the new video width/height (the HiWord represents the new width; the LoWord represents the new height).</para>
		/// </summary>
		VideoSizeChanged = 6
	}
}
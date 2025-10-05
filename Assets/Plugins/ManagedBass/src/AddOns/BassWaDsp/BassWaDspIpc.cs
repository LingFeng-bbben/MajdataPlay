namespace ManagedBass.WaDsp
{
    /// <summary>
    /// Communication to Winamp is done via the classic Win32 Message API.
    /// Most DSP plugins use this to ask for certain values.
    /// These definitions are the most commonly used messages, which might be handled by the fake winamp window which we create.
    /// </summary>
    public enum BassWaDspIpc
    {
        /// <summary>
        /// Version will be 0x20yx for winamp 2.yx. 
        /// Versions previous to Winamp 2.0 typically use 0x1zyx for 1.zx versions.
        /// <para>int version = SendMessage(hwnd_winamp,WM_WA_IPC,0,BassWaDspIpc.GetVersion);</para>
        /// </summary>
        GetVersion = 0,

        /// <summary>
        /// Using IPC_STARTPLAY is like hitting 'Play' in Winamp, mostly.
        /// <para>SendMessage(hwnd_winamp,WM_WA_IPC,0,BassWaDspIpc.StartPlay);</para>
        /// </summary>
        StartPlay = 102,

        /// <summary>
        /// Returns the status of playback.
        /// If it returns 1, it is playing.
        /// if it returns 3, it is paused.
        /// if it returns 0, it is not playing.
        /// <para>int res = SendMessage(hwnd_winamp,WM_WA_IPC,0,BassWaDspIpc.IsPlaying);</para>
        /// </summary>
        IsPlaying = 104,

        /// <summary>
        /// Returns the position in milliseconds of the current song (mode = 0), 
        /// or the song length, in seconds (mode = 1).
        /// Returns -1 if not playing or error.
        /// <para>int res = SendMessage(hwnd_winamp,WM_WA_IPC,mode,BassWaDspIpc.GetOutputTime);</para>
        /// </summary>
        GetOutputTime = 105,

        /// <summary>
        /// Returns the length of the current playlist, in tracks.
        /// <para>int length = SendMessage(hwnd_winamp,WM_WA_IPC,0,BassWaDspIpc.GetListLength);</para>
        /// </summary>
        GetListLength = 124,

        /// <summary>
        /// Returns the playlist position [index].
        /// <para>int pos=SendMessage(hwnd_winamp,WM_WA_IPC,0,BassWaDspIpc.GetListPosition);</para>
        /// </summary>
        GetListPosition = 125,

        /// <summary>
        /// Returns info about the current playing song.
        /// The value it returns depends on the value of 'mode' (wParam):
        /// 0 - SampleRate (i.e. 44100), 1 - Bitrate  (i.e. 128), 2 - Channels (i.e. 2)
        /// <para>int inf=SendMessage(hwnd_winamp,WM_WA_IPC,mode,BassWaDspIpc.GetInfo);</para>
        /// </summary>
        GetInfo = 126,

        /// <summary>
        /// Gets the filename of the playlist entry [index].
        /// Returns a pointer to it.
        /// Returns <see langword="null" /> on error.
        /// <para>char *name=SendMessage(hwnd_winamp,WM_WA_IPC,index,BassWaDspIpc.GetPlaylistFile);</para>
        /// </summary>
        GetPlaylistFile = 211,

        /// <summary>
        /// Gets the title of the playlist entry [index].
        /// Returns a pointer to it.
        /// Returns <see langword="null" /> on error.
        /// <para>char *name=SendMessage(hwnd_winamp,WM_WA_IPC,index,BassWaDspIpc.GetPlaylistTitle);</para>
        /// </summary>
        GetPlaylistTitle = 212,

        /// <summary>
        /// Is defined as Win32's WM_USER an.
        /// When a message of this value arrives, we know, that we might handle it by ourself.
        /// </summary>
        Ipc = 1024
    }
}
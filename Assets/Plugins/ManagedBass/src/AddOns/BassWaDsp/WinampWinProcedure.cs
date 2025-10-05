using System;

namespace ManagedBass.WaDsp
{
    /// <summary>
    /// User defined Window Message Process Handler (to be used with <see cref="BassWaDsp.Load(string,int,int,int,int,WinampWinProcedure)" />).
    /// <para>
    /// Normally this is not needed, since BASS_WADSP implements a default handler which emulates most functions needed by Winamp DSPs.
    /// However, some very special Winamp DSPs might require something special.
    /// So you might implement your own windows message handler here.
    /// </para>
    /// </summary>
    /// <param name="hwnd">The Window handle we are dealing with - which is the hidden fake window which BASS_WADSP has created during <see cref="BassWaDsp.Load(string,int,int,int,int,WinampWinProcedure)" /> and which emulates a Winamp 1.x class.</param>
    /// <param name="msg">The window message send. You typically might only react on WM_USER messages (also defined as <see cref="BassWaDspIpc" />).</param>
    /// <param name="wParam">The wParam message parameter see the Winamp SDK for further details.</param>
    /// <param name="lParam">The lParam message parameter see the Winamp SDK for further details.</param>
    /// <returns>See the Winamp SDK documentation for information.</returns>
    /// <remarks>
    /// <para>The <see cref="BassWaDspIpc" /> enumeration defines all lParam values which you should typically handle. See the Winamp SDK for further details.</para>
    /// </remarks>
    public delegate int WinampWinProcedure(IntPtr hwnd, int msg, int wParam, int lParam);
}

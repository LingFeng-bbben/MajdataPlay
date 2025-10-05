using System;

namespace ManagedBass.Wma
{
	/// <summary>
	/// User defined client connection notification callback function.
	/// </summary>
	/// <param name="Handle">The encoder handle (as returned by <see cref="BassWma.EncodeSetNotify" />).</param>
	/// <param name="Connect">The client's IP address... "xxx.xxx.xxx.xxx:port".</param>
	/// <param name="IP">The client is connecting?</param>
	/// <param name="User">The user instance data given when <see cref="BassWma.EncodeSetNotify" /> was called.</param>
	/// <remarks>A client connection notification can be used to keep track of who's connected, where they're from, and for long they've been connected.</remarks>
    public delegate void ClientConnectProcedure(int Handle, bool Connect, string IP, IntPtr User);
}
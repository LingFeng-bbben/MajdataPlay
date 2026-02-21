using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.Main
{
	internal static class ManualResetEventExtensions
	{
		public static SafeWaitHandle GetSafeWaitHandle(this ManualResetEvent mre)
		{
			return mre.SafeWaitHandle;
		}
	}
}

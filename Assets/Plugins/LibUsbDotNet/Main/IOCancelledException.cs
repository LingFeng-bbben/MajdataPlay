using System.IO;

namespace LibUsbDotNet.Main
{
	public class IOCancelledException : IOException
	{
		public IOCancelledException(string message)
			: base(message)
		{
		}
	}
}

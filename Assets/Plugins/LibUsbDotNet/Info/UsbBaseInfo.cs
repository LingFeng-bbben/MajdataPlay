using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibUsbDotNet.Info
{
	public abstract class UsbBaseInfo
	{
		internal List<byte[]> mRawDescriptors = new List<byte[]>();

		public ReadOnlyCollection<byte[]> CustomDescriptors => mRawDescriptors.AsReadOnly();
	}
}

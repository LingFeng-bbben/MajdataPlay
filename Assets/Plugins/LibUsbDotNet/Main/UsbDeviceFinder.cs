using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LibUsbDotNet.Main
{
	public class UsbDeviceFinder : ISerializable
	{
		public const int NO_PID = int.MaxValue;

		public const int NO_REV = int.MaxValue;

		public const string NO_SERIAL = null;

		public const int NO_VID = int.MaxValue;

		public static readonly Guid NO_GUID = Guid.Empty;

		private Guid mDeviceInterfaceGuid = Guid.Empty;

		private int mPid = int.MaxValue;

		private int mRevision = int.MaxValue;

		private string mSerialNumber;

		private int mVid = int.MaxValue;

		public Guid DeviceInterfaceGuid
		{
			get
			{
				return mDeviceInterfaceGuid;
			}
			set
			{
				mDeviceInterfaceGuid = value;
			}
		}

		public string SerialNumber
		{
			get
			{
				return mSerialNumber;
			}
			set
			{
				mSerialNumber = value;
			}
		}

		public int Revision
		{
			get
			{
				return mRevision;
			}
			set
			{
				mRevision = value;
			}
		}

		public int Pid
		{
			get
			{
				return mPid;
			}
			set
			{
				mPid = value;
			}
		}

		public int Vid
		{
			get
			{
				return mVid;
			}
			set
			{
				mVid = value;
			}
		}

		public UsbDeviceFinder(int vid, int pid, int revision, string serialNumber, Guid deviceInterfaceGuid)
		{
			mVid = vid;
			mPid = pid;
			mRevision = revision;
			mSerialNumber = serialNumber;
			mDeviceInterfaceGuid = deviceInterfaceGuid;
		}

		public UsbDeviceFinder(int vid, int pid, string serialNumber)
			: this(vid, pid, int.MaxValue, serialNumber, Guid.Empty)
		{
		}

		public UsbDeviceFinder(int vid, int pid, int revision)
			: this(vid, pid, revision, null, Guid.Empty)
		{
		}

		public UsbDeviceFinder(int vid, int pid)
			: this(vid, pid, int.MaxValue, null, Guid.Empty)
		{
		}

		public UsbDeviceFinder(int vid)
			: this(vid, int.MaxValue, int.MaxValue, null, Guid.Empty)
		{
		}

		public UsbDeviceFinder(string serialNumber)
			: this(int.MaxValue, int.MaxValue, int.MaxValue, serialNumber, Guid.Empty)
		{
		}

		public UsbDeviceFinder(Guid deviceInterfaceGuid)
			: this(int.MaxValue, int.MaxValue, int.MaxValue, null, deviceInterfaceGuid)
		{
		}

		protected UsbDeviceFinder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			mVid = (int)info.GetValue("Vid", typeof(int));
			mPid = (int)info.GetValue("Pid", typeof(int));
			mRevision = (int)info.GetValue("Revision", typeof(int));
			mSerialNumber = (string)info.GetValue("SerialNumber", typeof(string));
			mDeviceInterfaceGuid = (Guid)info.GetValue("DeviceInterfaceGuid", typeof(Guid));
		}

		protected UsbDeviceFinder()
		{
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("Vid", mVid);
			info.AddValue("Pid", mPid);
			info.AddValue("Revision", mRevision);
			info.AddValue("SerialNumber", mSerialNumber);
			info.AddValue("DeviceInterfaceGuid", mDeviceInterfaceGuid);
		}

		public static UsbDeviceFinder Load(Stream deviceFinderStream)
		{
			return new BinaryFormatter().Deserialize(deviceFinderStream) as UsbDeviceFinder;
		}

		public static void Save(UsbDeviceFinder usbDeviceFinder, Stream outStream)
		{
			new BinaryFormatter().Serialize(outStream, usbDeviceFinder);
		}

		public virtual bool Check(UsbRegistry usbRegistry)
		{
			if (mVid != int.MaxValue && usbRegistry.Vid != mVid)
			{
				return false;
			}
			if (mPid != int.MaxValue && usbRegistry.Pid != mPid)
			{
				return false;
			}
			if (mRevision != int.MaxValue && usbRegistry.Rev != mRevision)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(mSerialNumber))
			{
				if (string.IsNullOrEmpty(usbRegistry.SymbolicName))
				{
					return false;
				}
				UsbSymbolicName usbSymbolicName = UsbSymbolicName.Parse(usbRegistry.SymbolicName);
				if (mSerialNumber != usbSymbolicName.SerialNumber)
				{
					return false;
				}
			}
			if (mDeviceInterfaceGuid != Guid.Empty && !new List<Guid>(usbRegistry.DeviceInterfaceGuids).Contains(mDeviceInterfaceGuid))
			{
				return false;
			}
			return true;
		}

		public virtual bool Check(UsbDevice usbDevice)
		{
			if (mVid != int.MaxValue && (ushort)usbDevice.Info.Descriptor.VendorID != mVid)
			{
				return false;
			}
			if (mPid != int.MaxValue && (ushort)usbDevice.Info.Descriptor.ProductID != mPid)
			{
				return false;
			}
			if (mRevision != int.MaxValue && (ushort)usbDevice.Info.Descriptor.BcdDevice != mRevision)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(mSerialNumber) && mSerialNumber != usbDevice.Info.SerialString)
			{
				return false;
			}
			return true;
		}
	}
}

namespace LibUsbDotNet.Main
{
	public class UsbLockStyle
	{
		private ControlEpLockType mControlEpLock;

		private DataEpLockType mDataEpLock;

		private DeviceLockType mDeviceLockType;

		private int mEndpointControlTimeout;

		private int mEndpointLockTimeout;

		public DeviceLockType DeviceLockType
		{
			get
			{
				return mDeviceLockType;
			}
			set
			{
				mDeviceLockType = value;
			}
		}

		public ControlEpLockType ControlEpLock
		{
			get
			{
				return mControlEpLock;
			}
			set
			{
				mControlEpLock = value;
			}
		}

		public DataEpLockType DataEpLock
		{
			get
			{
				return mDataEpLock;
			}
			set
			{
				mDataEpLock = value;
			}
		}

		public int EndpointControlTimeout
		{
			get
			{
				return mEndpointControlTimeout;
			}
			set
			{
				mEndpointControlTimeout = value;
			}
		}

		public int EndpointLockTimeout
		{
			get
			{
				return mEndpointLockTimeout;
			}
			set
			{
				mEndpointLockTimeout = value;
			}
		}

		public UsbLockStyle(DeviceLockType deviceLockType, ControlEpLockType controlEpLockType, DataEpLockType dataEpLockType)
			: this(deviceLockType, controlEpLockType, dataEpLockType, 1000, 1000)
		{
		}

		public UsbLockStyle(DeviceLockType deviceLockType, ControlEpLockType controlEpLockType, DataEpLockType dataEpLockType, int endpoint0Timeout, int endpointLockTimeout)
		{
			mDeviceLockType = deviceLockType;
			mControlEpLock = controlEpLockType;
			mDataEpLock = dataEpLockType;
			mEndpointControlTimeout = endpoint0Timeout;
			mEndpointLockTimeout = endpointLockTimeout;
		}
	}
}

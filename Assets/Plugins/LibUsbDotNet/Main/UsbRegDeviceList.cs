using System;
using System.Collections;
using System.Collections.Generic;

namespace LibUsbDotNet.Main
{
	public class UsbRegDeviceList : IEnumerable<UsbRegistry>, IEnumerable
	{
		private readonly List<UsbRegistry> mUsbRegistryList;

		public UsbRegistry this[int index] => mUsbRegistryList[index];

		public int Count => mUsbRegistryList.Count;

		public UsbRegDeviceList()
		{
			mUsbRegistryList = new List<UsbRegistry>();
		}

		private UsbRegDeviceList(IEnumerable<UsbRegistry> usbRegDeviceList)
		{
			mUsbRegistryList = new List<UsbRegistry>(usbRegDeviceList);
		}

		IEnumerator<UsbRegistry> IEnumerable<UsbRegistry>.GetEnumerator()
		{
			return mUsbRegistryList.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<UsbRegistry>)this).GetEnumerator();
		}

		public UsbRegistry Find(Predicate<UsbRegistry> findUsbPredicate)
		{
			return mUsbRegistryList.Find(findUsbPredicate);
		}

		public UsbRegDeviceList FindAll(Predicate<UsbRegistry> findUsbPredicate)
		{
			return new UsbRegDeviceList(mUsbRegistryList.FindAll(findUsbPredicate));
		}

		public UsbRegistry FindLast(Predicate<UsbRegistry> findUsbPredicate)
		{
			return mUsbRegistryList.FindLast(findUsbPredicate);
		}

		public UsbRegistry Find(UsbDeviceFinder usbDeviceFinder)
		{
			return mUsbRegistryList.Find(usbDeviceFinder.Check);
		}

		public UsbRegDeviceList FindAll(UsbDeviceFinder usbDeviceFinder)
		{
			return FindAll(usbDeviceFinder.Check);
		}

		public UsbRegistry FindLast(UsbDeviceFinder usbDeviceFinder)
		{
			return mUsbRegistryList.FindLast(usbDeviceFinder.Check);
		}

		public bool Contains(UsbRegistry item)
		{
			return mUsbRegistryList.Contains(item);
		}

		public void CopyTo(UsbRegistry[] array, int offset)
		{
			mUsbRegistryList.CopyTo(array, offset);
		}

		public int IndexOf(UsbRegistry item)
		{
			return mUsbRegistryList.IndexOf(item);
		}

		internal bool Add(UsbRegistry item)
		{
			mUsbRegistryList.Add(item);
			return true;
		}
	}
}

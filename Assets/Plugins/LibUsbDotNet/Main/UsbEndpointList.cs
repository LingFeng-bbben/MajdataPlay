using System.Collections;
using System.Collections.Generic;

namespace LibUsbDotNet.Main
{
	public class UsbEndpointList : IEnumerable<UsbEndpointBase>, IEnumerable
	{
		private readonly List<UsbEndpointBase> mEpList = new List<UsbEndpointBase>();

		public UsbEndpointBase this[int index] => mEpList[index];

		public int Count => mEpList.Count;

		internal UsbEndpointList()
		{
		}

		public IEnumerator<UsbEndpointBase> GetEnumerator()
		{
			return mEpList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mEpList.GetEnumerator();
		}

		public void Clear()
		{
			while (mEpList.Count > 0)
			{
				Remove(mEpList[0]);
			}
		}

		public bool Contains(UsbEndpointBase item)
		{
			return mEpList.Contains(item);
		}

		public int IndexOf(UsbEndpointBase item)
		{
			return mEpList.IndexOf(item);
		}

		public void Remove(UsbEndpointBase item)
		{
			item.Dispose();
		}

		public void RemoveAt(int index)
		{
			UsbEndpointBase item = mEpList[index];
			Remove(item);
		}

		internal UsbEndpointBase Add(UsbEndpointBase item)
		{
			foreach (UsbEndpointBase mEp in mEpList)
			{
				if (mEp.EpNum == item.EpNum)
				{
					return mEp;
				}
			}
			mEpList.Add(item);
			return item;
		}

		internal bool RemoveFromList(UsbEndpointBase item)
		{
			return mEpList.Remove(item);
		}
	}
}

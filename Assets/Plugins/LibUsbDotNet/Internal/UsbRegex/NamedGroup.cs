namespace LibUsbDotNet.Internal.UsbRegex
{
	public struct NamedGroup
	{
		public readonly string GroupName;

		public readonly int GroupNumber;

		public NamedGroup(int GroupNumber, string GroupName)
		{
			this.GroupNumber = GroupNumber;
			this.GroupName = GroupName;
		}
	}
}

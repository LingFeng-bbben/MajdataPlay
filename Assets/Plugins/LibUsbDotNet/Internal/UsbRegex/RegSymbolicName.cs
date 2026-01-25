namespace LibUsbDotNet.Internal.UsbRegex
{
	internal class RegSymbolicName : BaseRegSymbolicName
	{
		public static readonly NamedGroup[] NamedGroups = new NamedGroup[5]
		{
			new NamedGroup(1, "Vid"),
			new NamedGroup(2, "Pid"),
			new NamedGroup(3, "Rev"),
			new NamedGroup(4, "ClassGuid"),
			new NamedGroup(5, "String")
		};

		public new string[] GetGroupNames()
		{
			return new string[5] { "Vid", "Pid", "Rev", "ClassGuid", "String" };
		}

		public new int[] GetGroupNumbers()
		{
			return new int[5] { 1, 2, 3, 4, 5 };
		}

		public new string GroupNameFromNumber(int groupNumber)
		{
			return groupNumber switch
			{
				1 => "Vid", 
				2 => "Pid", 
				3 => "Rev", 
				4 => "ClassGuid", 
				5 => "String", 
				_ => "", 
			};
		}

		public new int GroupNumberFromName(string groupName)
		{
			return groupName.ToLower() switch
			{
				"vid" => 1, 
				"pid" => 2, 
				"rev" => 3, 
				"classguid" => 4, 
				"string" => 5, 
				_ => -1, 
			};
		}
	}
}

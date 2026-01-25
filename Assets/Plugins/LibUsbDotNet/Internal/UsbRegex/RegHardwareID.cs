using System.Text.RegularExpressions;

namespace LibUsbDotNet.Internal.UsbRegex
{
	public class RegHardwareID : Regex
	{
		public enum ENamedGroups
		{
			Vid = 1,
			Pid,
			Rev,
			MI
		}

		private const RegexOptions OPTIONS = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;

		private const string PATTERN = "(Vid_(?<Vid>[0-9A-F]{1,4}))|(Pid_(?<Pid>[0-9A-F]{1,4}))|(Rev_(?<Rev>[0-9]{1,4}))|(MI_(?<MI>[0-9A-F]{1,2}))";

		public static readonly NamedGroup[] NAMED_GROUPS = new NamedGroup[4]
		{
			new NamedGroup(1, "Vid"),
			new NamedGroup(2, "Pid"),
			new NamedGroup(3, "Rev"),
			new NamedGroup(4, "MI")
		};

		private static RegHardwareID __globalInstance;

		public static RegHardwareID GlobalInstance
		{
			get
			{
				if (__globalInstance == null)
				{
					__globalInstance = new RegHardwareID();
				}
				return __globalInstance;
			}
		}

		public RegHardwareID()
			: base("(Vid_(?<Vid>[0-9A-F]{1,4}))|(Pid_(?<Pid>[0-9A-F]{1,4}))|(Rev_(?<Rev>[0-9]{1,4}))|(MI_(?<MI>[0-9A-F]{1,2}))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant)
		{
		}

		public new string[] GetGroupNames()
		{
			return new string[4] { "Vid", "Pid", "Rev", "MI" };
		}

		public new int[] GetGroupNumbers()
		{
			return new int[4] { 1, 2, 3, 4 };
		}

		public new string GroupNameFromNumber(int GroupNumber)
		{
			return GroupNumber switch
			{
				1 => "Vid", 
				2 => "Pid", 
				3 => "Rev", 
				4 => "MI", 
				_ => "", 
			};
		}

		public new int GroupNumberFromName(string GroupName)
		{
			return GroupName switch
			{
				"Vid" => 1, 
				"Pid" => 2, 
				"Rev" => 3, 
				"MI" => 4, 
				_ => -1, 
			};
		}
	}
}

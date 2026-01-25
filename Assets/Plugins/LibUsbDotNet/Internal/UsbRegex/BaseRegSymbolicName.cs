using System.Text.RegularExpressions;

namespace LibUsbDotNet.Internal.UsbRegex
{
	internal class BaseRegSymbolicName : Regex
	{
		private const RegexOptions OPTIONS = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;

		private const string PATTERN = "((&){0,1}Vid_(?<Vid>[0-9A-Fa-f]{1,4})(&){0,1}Pid_(?<Pid>[0-9A-Fa-f]{1,4})((&){0,1}Rev_(?<Rev>[0-9A-Fa-f]{1,4})){0,1})((\\x23{0,1}\\{(?<ClassGuid>([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+))})|(\\x23(?<String>[\\x20-\\x22\\x24-\\x2b\\x2d-\\x7f]+?)(?=\\x23|$)))*";

		public BaseRegSymbolicName()
			: base("((&){0,1}Vid_(?<Vid>[0-9A-Fa-f]{1,4})(&){0,1}Pid_(?<Pid>[0-9A-Fa-f]{1,4})((&){0,1}Rev_(?<Rev>[0-9A-Fa-f]{1,4})){0,1})((\\x23{0,1}\\{(?<ClassGuid>([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+)-([0-9A-Fa-f]+))})|(\\x23(?<String>[\\x20-\\x22\\x24-\\x2b\\x2d-\\x7f]+?)(?=\\x23|$)))*", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant)
		{
		}
	}
}

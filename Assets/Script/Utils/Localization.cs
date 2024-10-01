using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class Localization
    {
        public static Language Current { get; set; } = Language.Default;
        public static Language[] Available { get; private set; } = Array.Empty<Language>();

        public static string GetLocalizedText(MajText textType)
        {
            var table = Current.MappingTable;
            if (!table.ContainsKey(textType))
                return textType.ToString();
            else
                return table[textType];
        }
        public static void GetLocalizedText(MajText textType,out string origin)
        {
            var table = Current.MappingTable;
            if (!table.ContainsKey(textType))
                origin = textType.ToString();
            else
                origin = table[textType];
        }
    }
}

using MajdataPlay.Extensions;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;
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
            var result = table.Find(x => x.Type == textType);
            if (result is not null)
                return result.Content;
            else
                return textType.ToString();
        }
        public static string GetLocalizedText(string origin)
        {
            var table = Current.MappingTable;
            var result = table.Find(x => x.Origin == origin);

            return result?.Content ?? origin;
        }
        public static void GetLocalizedText(MajText textType,out string origin)
        {
            origin = GetLocalizedText(textType);
        }
    }
}

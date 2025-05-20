using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    internal static class StringExtensions
    {
        public static string i18n(this string origin)
        {
            return Localization.GetLocalizedText(origin);
        }
        public static bool Tryi18n(this string origin, out string result)
        {
            return Localization.TryGetLocalizedText(origin, out result);
        }
    }
}

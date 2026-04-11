
using System;

namespace MajdataPlay
{
    internal static class StringExtensions
    {   
        public static string i18n(this string origin, params object[] args)
        {
            Localization.TryGetLocalizedText(origin, out var result);
            if (args == null || args.Length == 0 || !result.Contains('{'))
            {
                return result;
            }
            try
            {
                return string.Format(result, args);
            }
            catch (Exception e)
            {
                MajDebug.LogError($"i18n format failed: key={origin}, template={result}");
                MajDebug.LogError(e);
                return result;
            }
        }

        public static bool Tryi18n(this string origin, out string result)
        {
            return Localization.TryGetLocalizedText(origin, out result);
        }
    }
}

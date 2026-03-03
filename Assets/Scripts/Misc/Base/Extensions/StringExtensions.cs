
namespace MajdataPlay
{
    internal static class StringExtensions
    {   
        public static string i18n(this string origin)
        {
            Localization.TryGetLocalizedText(origin, out var result);
            return result;
        }
        public static bool Tryi18n(this string origin, out string result)
        {
            return Localization.TryGetLocalizedText(origin, out result);
        }
    }
}

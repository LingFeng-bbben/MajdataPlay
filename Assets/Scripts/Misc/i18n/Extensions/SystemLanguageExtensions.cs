using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.i18n;
public static class SystemLanguageExtensions
{
    /// <summary>
    /// Converts <seealso cref="SystemLanguage"/> value into a default IETF BCP-47 locale string
    /// <para>(for example: "en-US", "zh-CN", "ja-JP").</para>
    ///
    /// <para>Because <seealso cref="SystemLanguage"/> does not contain region information,</para>
    /// <para>this method assigns the most commonly used region for each language.</para>
    ///
    /// <para>Example:</para>
    /// <para><seealso cref="SystemLanguage.English"/> -> "en-US"</para>
    /// <para><seealso cref="SystemLanguage.Japanese"/> -> "ja-JP"</para>
    /// <para><seealso cref="SystemLanguage.ChineseSimplified"/> -> "zh-CN"</para>
    /// </summary>
    /// <param name="language">The SystemLanguage value.</param>
    /// <returns>A locale string suitable for localization systems.</returns>
    public static string ToLocale(this SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.Afrikaans => "af-ZA",
            SystemLanguage.Arabic => "ar-SA",
            SystemLanguage.Basque => "eu-ES",
            SystemLanguage.Belarusian => "be-BY",
            SystemLanguage.Bulgarian => "bg-BG",
            SystemLanguage.Catalan => "ca-ES",

            SystemLanguage.Chinese => "zh-CN",
            SystemLanguage.ChineseSimplified => "zh-CN",
            SystemLanguage.ChineseTraditional => "zh-TW",

            SystemLanguage.Czech => "cs-CZ",
            SystemLanguage.Danish => "da-DK",
            SystemLanguage.Dutch => "nl-NL",
            SystemLanguage.English => "en-US",
            SystemLanguage.Estonian => "et-EE",
            SystemLanguage.Faroese => "fo-FO",
            SystemLanguage.Finnish => "fi-FI",
            SystemLanguage.French => "fr-FR",
            SystemLanguage.German => "de-DE",
            SystemLanguage.Greek => "el-GR",
            SystemLanguage.Hebrew => "he-IL",

            SystemLanguage.Hungarian => "hu-HU",
            SystemLanguage.Icelandic => "is-IS",
            SystemLanguage.Indonesian => "id-ID",
            SystemLanguage.Italian => "it-IT",
            SystemLanguage.Japanese => "ja-JP",
            SystemLanguage.Korean => "ko-KR",
            SystemLanguage.Latvian => "lv-LV",
            SystemLanguage.Lithuanian => "lt-LT",

            SystemLanguage.Norwegian => "nb-NO",
            SystemLanguage.Polish => "pl-PL",
            SystemLanguage.Portuguese => "pt-PT",
            SystemLanguage.Romanian => "ro-RO",
            SystemLanguage.Russian => "ru-RU",
            SystemLanguage.SerboCroatian => "sr-RS",

            SystemLanguage.Slovak => "sk-SK",
            SystemLanguage.Slovenian => "sl-SI",
            SystemLanguage.Spanish => "es-ES",
            SystemLanguage.Swedish => "sv-SE",
            SystemLanguage.Thai => "th-TH",
            SystemLanguage.Turkish => "tr-TR",

            SystemLanguage.Ukrainian => "uk-UA",
            SystemLanguage.Vietnamese => "vi-VN",

            SystemLanguage.Unknown => "en-US",

            _ => "en-US"
        };
    }

    /// <summary>
    /// Converts <seealso cref="SystemLanguage"/> into an ISO-639-1 language code
    /// without region information.
    /// 
    /// <para>Example:</para>
    /// <para><seealso cref="SystemLanguage.English"/> -> "en"</para>
    /// <para><seealso cref="SystemLanguage.Japanese"/> -> "ja"</para>
    /// <para><seealso cref="SystemLanguage.ChineseSimplified"/> -> "zh"</para>
    /// </summary>
    /// <param name="language">The SystemLanguage value.</param>
    /// <returns>Two-letter ISO language code.</returns>
    public static string ToLanguageCode(this SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.Afrikaans => "af",
            SystemLanguage.Arabic => "ar",
            SystemLanguage.Basque => "eu",
            SystemLanguage.Belarusian => "be",
            SystemLanguage.Bulgarian => "bg",
            SystemLanguage.Catalan => "ca",

            SystemLanguage.Chinese => "zh",
            SystemLanguage.ChineseSimplified => "zh",
            SystemLanguage.ChineseTraditional => "zh",

            SystemLanguage.Czech => "cs",
            SystemLanguage.Danish => "da",
            SystemLanguage.Dutch => "nl",
            SystemLanguage.English => "en",
            SystemLanguage.Estonian => "et",
            SystemLanguage.Faroese => "fo",
            SystemLanguage.Finnish => "fi",
            SystemLanguage.French => "fr",
            SystemLanguage.German => "de",
            SystemLanguage.Greek => "el",
            SystemLanguage.Hebrew => "he",

            SystemLanguage.Hungarian => "hu",
            SystemLanguage.Icelandic => "is",
            SystemLanguage.Indonesian => "id",
            SystemLanguage.Italian => "it",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.Korean => "ko",
            SystemLanguage.Latvian => "lv",
            SystemLanguage.Lithuanian => "lt",

            SystemLanguage.Norwegian => "no",
            SystemLanguage.Polish => "pl",
            SystemLanguage.Portuguese => "pt",
            SystemLanguage.Romanian => "ro",
            SystemLanguage.Russian => "ru",
            SystemLanguage.SerboCroatian => "sr",

            SystemLanguage.Slovak => "sk",
            SystemLanguage.Slovenian => "sl",
            SystemLanguage.Spanish => "es",
            SystemLanguage.Swedish => "sv",
            SystemLanguage.Thai => "th",
            SystemLanguage.Turkish => "tr",

            SystemLanguage.Ukrainian => "uk",
            SystemLanguage.Vietnamese => "vi",

            _ => "en"
        };
    }
}

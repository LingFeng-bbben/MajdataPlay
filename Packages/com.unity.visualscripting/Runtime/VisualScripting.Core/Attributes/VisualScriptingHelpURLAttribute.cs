#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
#define CAN_USE_CUSTOM_HELP_URL
#endif

using System;
using System.Diagnostics;
using UnityEngine;

namespace Unity.VisualScripting
{
#if CAN_USE_CUSTOM_HELP_URL
using UnityEditor.PackageManager;
    [Conditional("UNITY_EDITOR")]
    class VisualScriptingHelpURLAttribute : HelpURLAttribute
    {
        const string k_BaseURL = "https://docs.unity3d.com/Packages/com.unity.visualscripting@";
        const string k_MidURL = "/api/";
        const string k_EndURL = ".html";
        const string k_FallbackVersion = "1.9";

        static string s_PackageVersion;

        static string PackageVersion
        {
            get
            {
                if (string.IsNullOrEmpty(s_PackageVersion))
                {
                    var packageInfo = PackageInfo.FindForAssetPath("Packages/com.unity.visualscripting");
                    s_PackageVersion = packageInfo == null ? k_FallbackVersion : GetMinorPackageVersionString(packageInfo.version);
                }

                return s_PackageVersion;
            }
        }

        // internal for test
        internal static string GetMinorPackageVersionString(string versionString)
        {
            var split = versionString.Split('.');
            return split.Length < 2 ? $"{split[0]}.0" : $"{split[0]}.{split[1]}";
        }

        public VisualScriptingHelpURLAttribute(Type type)
            : base(HelpURL(type)) {}

        static string HelpURL(Type type)
        {
            return $"{k_BaseURL}{PackageVersion}{k_MidURL}{type.FullName}{k_EndURL}";
        }
    }
#else  //HelpURL attribute is `sealed` in previous Unity versions
    [Conditional("UNITY_EDITOR")]
    class VisualScriptingHelpURLAttribute : Attribute
    {
        public VisualScriptingHelpURLAttribute(Type type) { }
    }
#endif
}

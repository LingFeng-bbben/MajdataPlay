using System.Text.RegularExpressions;
using UnityEditor;

namespace MajdataPlay.Editor
{
    internal sealed class LangVersionPostprocessor : AssetPostprocessor
    {
        private static string OnGeneratedCSProject(string path, string content)
        {
            var pattern = @"<LangVersion>(.*?)<\/LangVersion>";
            var replacement = "<LangVersion>preview</LangVersion>";
            content = Regex.Replace(content, pattern, replacement);
            return content;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace MajdataPlay
{
    public class Language
    {
        public string Code { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        
        public Dictionary<string, string>? Translations { get; init; }
        
        public LangTable[]? MappingTable { get; init; }
        
        private Dictionary<string, string>? _cachedTranslations;
        
        /// <summary>
        /// 获取翻译字典（自动从MappingTable或Translations构建）
        /// </summary>
        internal Dictionary<string, string> GetTranslations()
        {
            if (_cachedTranslations != null)
                return _cachedTranslations;
                
            // 优先使用Dictionary格式
            if (Translations != null && Translations.Count > 0)
            {
                _cachedTranslations = Translations;
                return _cachedTranslations;
            }
            
            if (MappingTable != null && MappingTable.Length > 0)
            {
                _cachedTranslations = new Dictionary<string, string>(MappingTable.Length);
                foreach (var item in MappingTable)
                {
                    if (!string.IsNullOrEmpty(item.Origin))
                        _cachedTranslations[item.Origin] = item.Content;
                }
                return _cachedTranslations;
            }
            
            _cachedTranslations = new Dictionary<string, string>();
            return _cachedTranslations;
        }
        
        public override string ToString()
        {
            return $"{Code} - {Author}";
        }
        public static Language Default => new();
    }
}

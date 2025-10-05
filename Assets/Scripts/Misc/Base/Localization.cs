using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

#nullable enable
namespace MajdataPlay
{
    public static class Localization
    {
        public static event EventHandler<Language>? OnLanguageChanged;
        public static Language Current
        {
            get => _current;
            set
            {
                _current = value;
                if (OnLanguageChanged is not null)
                    OnLanguageChanged(null, value);
            }
        }
        readonly static JsonSerializerSettings jsonReaderSettings = new()
        {
            Formatting = Formatting.Indented,
            Converters = 
            { 
                new StringEnumConverter() 
            }
        };
        public static Language[] Available { get; private set; } = Array.Empty<Language>();

        static bool _isInited = false;
        readonly static object _initLock = new();

        internal static void Init()
        {
            if (_isInited)
            {
                return;
            }
            lock (_initLock)
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;
            }

            try
            {
                var path = MajEnv.LangPath;
                if (!Directory.Exists(path))
                {
                    return;
                }
                var files = new DirectoryInfo(path).GetFiles()
                                                   .Where(x => x.Extension == ".json");
                List<Language> loadedLangs = new();
                foreach (var fileInfo in files)
                {
                    var filePath = fileInfo.FullName;
                    var json = File.ReadAllText(filePath);
                    Language? lang = null;
                    if (Serializer.Json.TryDeserialize(json, out lang, jsonReaderSettings) && lang is not null)
                    {
                        loadedLangs.Add(lang);
                    }
                    else
                    {
                        continue;
                    }
                }
                if (loadedLangs.IsEmpty())
                {
                    return;
                }
                var grouped = loadedLangs.GroupBy(x => x.ToString());
                Available = new Language[grouped.Count()];
                foreach (var (i, grouping) in grouped.WithIndex())
                {
                    Available[i] = grouping.First();
                }
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        /// <summary>
        /// Set language by code and author<para>such like: "zh-CN - Majdata"</para>
        /// </summary>
        /// <param name="langInfo"></param>
        public static void SetLang(string langInfo)
        {
            if (Available.IsEmpty())
                return;
            var result = Available.Find(x => x.ToString() == langInfo);
            if (result is null)
                return;
            Current = result;
        }
        public static void SetLangByCode(string code)
        {
            if (Available.IsEmpty())
                return;
            var result = Available.Find(x => x.Code == code);
            if (result is null)
                return;
            Current = result;
        }

        /// <summary>
        /// Method <c>GetLocalizedText</c> returns the translated text of the parameter or the parameter itself.
        /// </summary>
        public static string GetLocalizedText(string origin)
        {
            TryGetLocalizedText(origin, out var result);

            return result;
        }
        public static bool TryGetLocalizedText(string origin,out string strOut)
        {
            var table = Current.MappingTable;
            var result = table.Find(x => x.Origin == origin);
            strOut = result?.Content ?? origin;

            return result is not null;
        }
        static Language _current = Language.Default;
    }
}


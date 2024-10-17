using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Helpers
{
    internal class LocalizedString
    {
        private readonly ModelFactory factory;
        private readonly IReadOnlyList<string>? defaultLanguages;

        private string registryKey;

        public unsafe LocalizedString(ModelFactory factory)
        {
            this.factory = factory;

            registryKey = string.Empty;

            var tmpKey = factory.RootModel.ConfigRegistryKey;
            if (!string.IsNullOrEmpty(tmpKey))
            {
                tmpKey = tmpKey.Replace('/', '\\');
                var context = new ParameterContext();
                if (context.TryFormat(tmpKey, out var value))
                {
                    registryKey = value;
                }
            }

            const uint MUI_LANGUAGE_NAME = 0x8;
            const uint MUI_MERGE_SYSTEM_FALLBACK = 0x10;
            const uint MUI_MERGE_USER_FALLBACK = 0x20;

            uint langCount = 0;
            uint bufferLen = 0;

            if (Windows.Win32.PInvoke.GetThreadPreferredUILanguages(
                MUI_MERGE_SYSTEM_FALLBACK | MUI_MERGE_USER_FALLBACK | MUI_LANGUAGE_NAME,
                &langCount,
                default,
                &bufferLen))
            {
                var buffer = new char[bufferLen];
                fixed (char* pBuffer = buffer)
                {
                    if (Windows.Win32.PInvoke.GetThreadPreferredUILanguages(
                        MUI_MERGE_SYSTEM_FALLBACK | MUI_MERGE_USER_FALLBACK | MUI_LANGUAGE_NAME,
                        &langCount,
                        pBuffer,
                        &bufferLen))
                    {
                        bool enFlag = false;
                        bool enUSFlag = false;

                        var langs = new List<string>((int)langCount);
                        for (int start = 0, i = 0; i < bufferLen; i++)
                        {
                            if (buffer[i] == '\0')
                            {
                                if (i - start > 0)
                                {
                                    var lang = new string(buffer, start, i - start);
                                    langs.Add(lang);

                                    if (!enFlag && string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase)) enFlag = true;
                                    else if (!enUSFlag && string.Equals(lang, "en-US", StringComparison.OrdinalIgnoreCase)) enUSFlag = true;
                                    else if (string.Equals(lang, "zh-Hans", StringComparison.OrdinalIgnoreCase)) langs.Add("zh-CHS");
                                    else if (string.Equals(lang, "zh-Hant", StringComparison.OrdinalIgnoreCase)) langs.Add("zh-CHT");
                                }
                                start = i + 1;
                            }
                        }

                        langs.Add("default");
                        if (!enFlag) langs.Add("en");
                        if (!enUSFlag) langs.Add("en-US");
                        defaultLanguages = langs;
                    }
                }
            }
        }

        public string Get(Dictionary<string, string?>? strings)
        {
            strings = NormalizeDictionary(strings);
            if (strings == null || strings.Count == 0) return string.Empty;

            string? lang = "";

            if (!string.IsNullOrEmpty(registryKey)
                && SettingsHelper.TryGetValue(registryKey, "Language", out lang)
                && !string.IsNullOrEmpty(lang))
            {
                // 使用注册表中的语言
                var text = GetLanguageFallbackString(strings, lang);
                if (!string.IsNullOrEmpty(text)) return text;
            }

            if (defaultLanguages != null)
            {
                // 使用系统默认语言
                for (int i = 0; i < defaultLanguages.Count; i++)
                {
                    var lang2 = defaultLanguages[i];

                    { if (TryGetValue(strings, lang2, out var value) && !string.IsNullOrEmpty(value)) return value; }
                }
            }
            return string.Empty;
        }

        private static string? GetLanguageFallbackString(Dictionary<string, string?> strings, string lang)
        {
            if (strings == null || string.IsNullOrEmpty(lang)) return null;

            { if (TryGetValue(strings, lang, out var value) && !string.IsNullOrEmpty(value)) return value; }

            var idx = lang.Length;
            while (idx > 0)
            {
                idx--;
                if (lang[idx] == '-')
                {
                    var lang2 = lang[..idx];

                    { if (TryGetValue(strings, lang2, out var value)) return value ?? string.Empty; }
                }
            }

            return string.Empty;
        }

        private static bool TryGetValue(Dictionary<string, string?>? dict, string? key, out string? value)
        {
            value = null;
            if (dict == null || dict.Count == 0 || key == null) return false;

            if (dict.Count > 20) return dict.TryGetValue(key, out value);

            foreach (var item in dict)
            {
                if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = item.Value;
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<string, string?>? NormalizeDictionary(Dictionary<string, string?>? dict)
        {
            if (dict == null) return null;
            if (dict.Count <= 20) return dict;

            return new Dictionary<string, string?>(dict, StringComparer.OrdinalIgnoreCase);
        }

    }
}

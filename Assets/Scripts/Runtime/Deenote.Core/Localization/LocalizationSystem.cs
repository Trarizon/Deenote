using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace Deenote.CoreB.Localization
{
    public class LocalizationSystem
    {
        public const string DefaultLanguageCode = "en";
        internal const string DefaultLanguageName = "English";

        private static LocalizationSystem _ins = new LocalizationSystem();

        internal static void OnDomainReloaded()
        {
            _ins = new LocalizationSystem();
        }

        private readonly Dictionary<string, LanguagePack> _languageDict = new();
        private LanguagePack _currentLanguagePack;
        private LanguagePack _defaultLanguagePack;
        private Action<LanguagePack>? _languageChanged;

        public static Dictionary<string, LanguagePack>.ValueCollection Languages => _ins._languageDict.Values;

        public static LanguagePack CurrentLanguage
        {
            get => _ins._currentLanguagePack;
            set {
                if (_ins._currentLanguagePack == value)
                    return;

                if (!_ins._languageDict.ContainsKey(value.LanguageCode))
                    ThrowHelper.ThrowArgumentException("Language pack is not found in the dictionary.");

                _ins._currentLanguagePack = value;
                _ins._languageChanged?.Invoke(value);
            }
        }

        public static event Action<LanguagePack>? LanguageChanged
        {
            add => _ins._languageChanged += value;
            remove => _ins._languageChanged -= value;
        }

        private LocalizationSystem()
        {
            var folder = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            foreach (var file in files) {
                if (!file.EndsWith(".txt")) continue;
                if (LanguagePack.TryLoad(file, out var pack)) {
                    _languageDict.TryAdd(pack.LanguageCode, pack);
                }
            }

            if (!_languageDict.ContainsKey(DefaultLanguageCode)) {
                Debug.LogWarning("Default language translation file is not found.");
                _languageDict.Add(DefaultLanguageCode, LanguagePack.FallbackDefault);
            }

            _currentLanguagePack = _defaultLanguagePack = _languageDict[DefaultLanguageCode];
        }

        public static string GetText(LocalizableText text) =>
            !text.IsLocalized ? text.TextOrKey
                : _ins._currentLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
                  _ins._defaultLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
                  text.TextOrKey;

        public static bool TrySetLanguage([AllowNull] string languageCode)
        {
            if (languageCode is null)
                return false;
            if (_ins._languageDict.TryGetValue(languageCode, out var pack)) {
                _ins._currentLanguagePack = pack;
                return true;
            }
            return false;
        }

        public static bool TryGetLanguagePack(string languageCode, out LanguagePack languagePack)
        {
            return _ins._languageDict.TryGetValue(languageCode, out languagePack);
        }
    }
}
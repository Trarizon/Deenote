#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.UIFramework.Theme
{
    public sealed class UIThemeManager
    {
        private readonly UIThemeArgs[] _builtinThemes;
        private int _currentThemeIndex;

        internal UIThemeManager()
        {
            _builtinThemes = Resources.LoadAll<UIThemeArgs>("UI/BuiltinThemes");
            Debug.Assert(_builtinThemes.Length >= 1, "We must have at least one theme as default");
            ValidateThemes();
        }

        public IReadOnlyList<UIThemeArgs> Themes => _builtinThemes;

        public UIThemeArgs CurrentTheme
        {
            get => _builtinThemes[_currentThemeIndex];
            set {
                var index = Array.IndexOf(_builtinThemes, value);
                if (index < 0)
                    ThrowHelper.ThrowArgumentException(nameof(value), "$Theme not found: {value.ThemeName}, where did you get it?");
                SetThemeByIndex(index);
            }
        }

        public event Action<UIThemeArgs>? ThemeChanged;

        public void SetThemeByIndex(int index)
        {
            Guard.IsInRangeFor(index, _builtinThemes);
            if (Utils.SetField(ref _currentThemeIndex, index)) {
                ThemeChanged?.Invoke(CurrentTheme);
            }
        }

        public bool TrySetTheme([AllowNull] string themeId)
        {
            if (themeId is null)
                return false;

            for (int i = 0; i < _builtinThemes.Length; i++) {
                if (_builtinThemes[i].ThemeId == themeId) {
                    SetThemeByIndex(i);
                    return true;
                }
            }
            return false;
        }

        public void SetTheme([AllowNull] string themeId, bool fallbackDefaultTheme = false)
        {
            if (TrySetTheme(themeId))
                return;

            if (fallbackDefaultTheme) {
                SetThemeByIndex(0);
                return;
            }

            ThrowHelper.ThrowInvalidOperationException($"Theme not found: {themeId}");
        }

        public void SetDefaultTheme() => SetThemeByIndex(0);

        private void ValidateThemes()
        {
            HashSet<string> visiteds = new();
            foreach (var theme in _builtinThemes) {
                if (!visiteds.Add(theme.ThemeName)) {
                    ThrowHelper.ThrowArgumentException(nameof(UIThemeArgs.ThemeName), $"Duplicate theme name: {theme.ThemeName}");
                }
            }
        }
    }
}

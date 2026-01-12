#nullable enable

using Deenote.UIFramework.Font;
using Deenote.UIFramework.Theme;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Deenote.UIFramework
{
    public static class UISystem
    {
        private static GameObject? _gameObject;

        internal static GameObject GameObject => _gameObject ??= new GameObject(nameof(UISystem));

        #region Theme

        private static UIThemeManager? _theme;
        public static UIThemeManager ThemeManager => _theme ??= new UIThemeManager();

        private static UIResources? _resources;
        internal static UIResources UIResources => _resources ??= Resources.Load<UIResources>($"UI/UIResources");

        #endregion

        #region Font

        private static TMP_FontAsset? _fontAsset;
        public static TMP_FontAsset FontAsset
        {
            get {
                if (_fontAsset is null) {
                    var fontAsset = UIFontManager.LoadSystemFontAssets(UIResources.PreferedFontName);
                    fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    foreach (string name in UIResources.FallbackFontNames) {
                        var fallbackFont = UIFontManager.LoadSystemFontAssets(name);
                        if (fallbackFont != null)
                            fontAsset.fallbackFontAssetTable.Add(fallbackFont);
                        else
                            Debug.LogWarning($"Load font {fallbackFont} failed");
                    }
                    fontAsset.fallbackFontAssetTable.Add(UIResources.FinalFallbackFont);
                    _fontAsset = fontAsset;
                }

                return _fontAsset;
            }
        }

        #endregion

        internal static UIFocusManager FocusManager => UIFocusManager.Instance;

        public static event Action<IFocusable> FocusedControlChanged
        {
            add => FocusManager.FocusingChanged += value;
            remove => FocusManager.FocusingChanged -= value;
        }
    }
}
#nullable enable

using Deenote.UIFramework.Theme;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    [DisallowMultipleComponent]
    public abstract class UIThemedControl : MonoBehaviour
    {
        protected abstract void OnThemeChanged(UIThemeArgs args);

        protected virtual void Awake()
        {
            OnThemeChanged(UISystem.ThemeManager.CurrentTheme);
            UISystem.ThemeManager.ThemeChanged += OnThemeChanged;
        }

        protected virtual void OnDestroy()
        {
            UISystem.ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        protected virtual void OnValidate()
        {
            OnThemeChanged(UISystem.ThemeManager.CurrentTheme);
        }
    }
}
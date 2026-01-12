#nullable enable

using Deenote.Api.Experimental.Plugin;
using Deenote.Localization;
using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.PluginSystem.UI
{
    public sealed class ToolkitButton : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        private IDeenotePlugin _plugin = default!;

        private void Awake()
        {
            _button.Clicked += () =>
            {
                _plugin.ExecuteAsync(PluginContext.Instance);
            };
            LocalizationSystem.LanguageChanged += SetButtonText;
        }

        internal void OnInstantiate(IDeenotePlugin plugin)
        {
            _plugin = plugin;
            SetButtonText(LocalizationSystem.CurrentLanguage);
        }

        private void SetButtonText(LanguagePack lang)
        {
            _button.Text.SetRawText(_plugin.GetName(lang.LanguageCode));
        }
    }
}
#nullable enable

using Deenote.Api.Experimental.Plugin;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote.PluginSystem.UI
{
    public sealed class ToolkitColumn : MonoBehaviour
    {
        [SerializeField] RectTransform _contentTransform = default!;

        internal void OnInstantiate(ToolkitPanelManager manager, ImmutableArray<IDeenotePlugin> plugins)
        {
            foreach (var plugin in plugins) {
                var p = Instantiate(manager.ToolkitButtonPrefab, _contentTransform);
                p.OnInstantiate(plugin);
            }
        }
    }
}

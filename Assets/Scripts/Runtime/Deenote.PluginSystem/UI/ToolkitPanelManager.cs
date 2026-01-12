#nullable enable

using Deenote.Api.Experimental.Plugin;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.PluginSystem.UI
{
    public sealed class ToolkitPanelManager : MonoBehaviour
    {
        [SerializeField] ToolkitButton _toolkitButtonPrefab;
        [SerializeField] ToolkitColumn _toolkitColumnPrefab;
        [SerializeField] ToolkitGroupPanel _toolkitGroupPanelPrefab;
        [SerializeField] RectTransform _contentTransform;

        public ToolkitButton ToolkitButtonPrefab => _toolkitButtonPrefab;
        public ToolkitColumn ToolkitColumnPrefab => _toolkitColumnPrefab;
        public RectTransform ContentTransform => _contentTransform;

        private readonly List<ToolkitGroupPanel> _groups = new();

        public ToolkitGroupPanel AddGroup(IDeenotePluginProvider plugin)
        {
            var rtn = Instantiate(_toolkitGroupPanelPrefab, _contentTransform);
            rtn.OnInstantiated(this, plugin);
            _groups.Add(rtn);
            return rtn;
        }

        public void RemoveGroup(IDeenotePluginProvider plugin)
        {
            for (int i = 0; i < _groups.Count; i++) {
                if (_groups[i].PluginProvider == plugin) {
                    Destroy(_groups[i].gameObject);
                    _groups.RemoveAt(i);
                }
            }
        }
    }
}

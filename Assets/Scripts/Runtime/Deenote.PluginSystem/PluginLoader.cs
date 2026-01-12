#nullable enable

using Deenote.Api.Experimental.Plugin;
using Deenote.PluginSystem.Builtins;
using Deenote.PluginSystem.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Deenote.PluginSystem
{
    public sealed class PluginLoader : MonoBehaviour
    {
        private const string PluginsDirName = "Plugins";
        private const string PluginMetadataFileName = "metadata.json";

        [SerializeField] ToolkitPanelManager _panelManager;

        private readonly List<IDeenotePluginProvider> _builtins = new();
        private readonly List<IDeenotePluginProvider> _plugins = new();

        private void Awake()
        {
            LoadBuitinPlugins();
            LoadUserPlugins();
        }

        private void Start()
        {
            ShowPlugins();
        }

        private void LoadBuitinPlugins()
        {
            _builtins.Add(new CommandButtonsPluginProvider(this));
        }

        private void LoadUserPlugins()
        {
            var dir = Path.Combine(Application.streamingAssetsPath, PluginsDirName);
            if (!Directory.Exists(dir))
                return;

            // StreamingAssets/Plugins
            //   - Trarizon.MyPlugin
            //     - metadata.json
            //     - SomePlugin.dll
            //     - Dependencies or other files

            var pluginDirs = Directory.GetDirectories(dir);
            foreach (var pluginDir in pluginDirs) {
                var metadataFile = Path.Combine(pluginDir, PluginMetadataFileName);
                if (!File.Exists(metadataFile))
                    continue;

                if (!PluginMetadata.TryLoad(metadataFile, out var metadata))
                    continue;

                var assemblyFile = Path.Combine(pluginDir, metadata.EntryDll);
                if (!File.Exists(assemblyFile))
                    continue;

                try {
                    var assembly = Assembly.LoadFile(assemblyFile);
                    var types = assembly.GetTypes();
                    foreach (var type in types) {
                        if (type.IsInterface || type.IsAbstract)
                            continue;

                        if (typeof(IDeenotePluginProvider).IsAssignableFrom(type)) {
                            try {
                                var pluginProvider = (IDeenotePluginProvider)Activator.CreateInstance(type)!;
                                _plugins.Add(pluginProvider);
                            } catch (Exception ex) {
                                Debug.LogError($"Failed to load plugin {type.Name} in {Path.GetFileName(dir)}/{metadata.EntryDll}\n{ex.Message}");
                            }
                        }
                    }
                } catch (Exception ex) {
                    Debug.LogWarning($"Failed to load plugin from {Path.GetFileName(dir)}/{metadata.EntryDll}\n{ex.Message}");
                }
            }

            Debug.Log($"Load {_plugins.Count} plugins in while {pluginDirs.Length} found.");
        }

        public void ReloadUserPlugins()
        {
            foreach (var plugin in _plugins) {
                _panelManager.RemoveGroup(plugin);
            }
            _plugins.Clear();
            LoadUserPlugins();
            foreach (var plugin in _plugins) {
                _panelManager.AddGroup(plugin);
            }
        }

        private void ShowPlugins()
        {
            foreach (var plugin in _builtins) {
                _panelManager.AddGroup(plugin);
            }
            foreach (var plugin in _plugins) {
                _panelManager.AddGroup(plugin);
            }
        }
    }
}

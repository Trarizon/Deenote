#nullable enable

using Deenote.Api.Experimental;
using Deenote.Api.Experimental.Plugin;
using Deenote.PluginSystem.Builtins.Helpers;
using System.Collections.Immutable;

namespace Deenote.PluginSystem.Builtins
{
    public sealed class CommandButtonsPluginProvider : IDeenotePluginProvider
    {
        private PluginLoader _pluginLoader;

        public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; }

        public string? GetGroupName(string languageCode) => languageCode switch {
            "zh" => "基础命令",
            "en" or _ => "Basic Commands",
        };

        public CommandButtonsPluginProvider(PluginLoader pluginLoader)
        {
            _pluginLoader = pluginLoader;

            Plugins = ImmutableArray.Create(
                ImmutableArray.Create(
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Undo", ("zh", "撤销")),
                        context => context.OperationHistory.Undo()),
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Redo", ("zh", "重做")),
                        context => context.OperationHistory.Redo())
                ),
                ImmutableArray.Create(
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Cut", ("zh", "剪切")),
                        context => context.ChartEditor.CutSelectedNotes()),
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Copy", ("zh", "复制")),
                        context => context.ChartEditor.CopySelectedNotes()),
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Paste", ("zh", "粘贴")),
                        context => context.ChartEditor.PasteNotes())
                ),
                ImmutableArray.Create(
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Quantize", ("zh", "吸附格线")),
                        context => context.ChartEditor.EditSelectedNotesCoord(c => context.GridsManager.Quantize(c, GridSnapOptions.Both))),
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Mirror", ("zh", "镜像")),
                        context => context.ChartEditor.EditSelectedNotesPosition(p => -p))
                ),
                ImmutableArray.Create(
                    DeenotePlugin.CreateSync(PluginHelpers.Text("Reload Plugins", ("zh", "重新加载插件")),
                        context => _pluginLoader.ReloadUserPlugins())
                )
            );
        }
    }
}

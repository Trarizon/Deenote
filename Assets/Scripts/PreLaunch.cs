#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.CoreB.Localization;
using Deenote.Plugin;
using Deenote.Runtime.Plugins;
using Deenote.UI;
using Deenote.UI.Dialogs.Elements;
using System.ComponentModel;
using UnityEngine;

namespace Deenote
{
    public static class PreLaunch
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RegisterBuiltinPlugins()
        {
            DeenotePluginManager.RegisterPluginGroup(new OldVersionCompatibility());
            DeenotePluginManager.RegisterPluginGroup(new CommandShortcutButtons());
        }
    }
}
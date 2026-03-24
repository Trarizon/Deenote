#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;

namespace Deenote.Editing.Contexts
{
    internal sealed partial class EnvironmentContext : ObservableObject
    {
        [ObservableProperty] GameVersion _gameVersion;
    }
}

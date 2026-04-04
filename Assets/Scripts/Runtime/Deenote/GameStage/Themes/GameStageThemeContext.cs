#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Library;
using System;

namespace Deenote.GameStage.Themes
{
    public sealed class GameStageThemeContext : INotifyPropertyChanged<GameStageThemeContext>
    {

        public event Action<GameStageThemeContext, PropertyEventArgs>? PropertyChanged;

        private GameStageThemeEntry? _currentTheme_bf;
        public GameStageThemeEntry? CurrentTheme
        {
            get => _currentTheme_bf;
            set {
                if (Utils.SetField(ref _currentTheme_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentTheme)));
                }
            }
        }

        public GameStageThemeContext()
        {
        }
    }
}

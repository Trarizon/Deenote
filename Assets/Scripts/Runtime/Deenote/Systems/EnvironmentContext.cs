using Deenote.CoreB.Notification;
using System;

namespace Deenote.Systems
{
    public class EnvironmentContext : INotifyPropertyChanged<EnvironmentContext>
    {
        private const int DefaultAutoSaveIntervalTime = 5 * 60;

        private AutoSaveOptions _autoSaveOptions_bf;
        public AutoSaveOptions AutoSaveOptions
        {
            get => _autoSaveOptions_bf;
            set {
                if (_autoSaveOptions_bf != value) {
                    _autoSaveOptions_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoSaveOptions)));
                }
            }
        }

        private int _autoSaveIntervalSeconds_bf;
        /// <remarks>
        /// Unit second
        /// </remarks>
        public int AutoSaveIntervalSeconds
        {
            get => _autoSaveIntervalSeconds_bf;
            set {
                if (_autoSaveIntervalSeconds_bf != value) {
                    _autoSaveIntervalSeconds_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoSaveIntervalSeconds)));
                }
            }
        }

        public event Action<EnvironmentContext, PropertyChangedEventArgs>? PropertyChanged;
    }
}

using Deenote.CoreB.Notification;
using System;
using UnityEngine;

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
                    PropertyChanged?.Invoke(this, nameof(AutoSaveOptions));
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
                if (value <= 0)
                    value = DefaultAutoSaveIntervalTime;
                if (_autoSaveIntervalSeconds_bf != value) {
                    _autoSaveIntervalSeconds_bf = value;
                    PropertyChanged?.Invoke(this, nameof(AutoSaveIntervalSeconds));
                }
            }
        }

        private bool _vSync_bf;
        public bool IsVSyncOn
        {
            get => _vSync_bf;
            set {
                if (_vSync_bf != value) {
                    _vSync_bf = value;
                    QualitySettings.vSyncCount = value ? 1 : 0;
                    PropertyChanged?.Invoke(this, nameof(IsVSyncOn));
                }
            }
        }

        private bool _checkUpdateOnStartup_bf;
        public bool CheckUpdateOnStartup
        {
            get => _checkUpdateOnStartup_bf;
            set {
                if (_checkUpdateOnStartup_bf != value) {
                    _checkUpdateOnStartup_bf = value;
                    PropertyChanged?.Invoke(this, nameof(CheckUpdateOnStartup));
                }
            }
        }

        public event Action<EnvironmentContext, PropertyChangedEventArgs>? PropertyChanged;

        public EnvironmentContext()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("project/autosave", (int)AutoSaveOptions);
                configs.Set("project/autosave_interval", AutoSaveIntervalSeconds);
                configs.Set("vsync", IsVSyncOn);
                configs.Set("check_update", CheckUpdateOnStartup);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                AutoSaveOptions = (AutoSaveOptions)configs.GetInt32("project/autosave");
                AutoSaveIntervalSeconds = configs.GetInt32("project/autosave_interval", DefaultAutoSaveIntervalTime);
                IsVSyncOn = configs.GetBoolean("vsync");
                CheckUpdateOnStartup = configs.GetBoolean("check_update", true);
            };
        }
    }
}

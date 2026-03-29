#nullable enable

using Deenote.Core;
using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using Deenote.Library;
using Deenote.ProjectManagement;
using System;

namespace Deenote.Contexts
{
    public sealed class EnvironmentContext : INotifyPropertyChanged<EnvironmentContext>
    {
        private const int DefaultAutoSaveIntervalTime = 5 * 60;

        private GameVersion _gameVersion_bf;
        public GameVersion GameVersion
        {
            get => _gameVersion_bf;
            set {
                if (Utils.SetField(ref _gameVersion_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(GameVersion)));
                }
            }
        }

        private int _autoSaveIntervalTime_bf;
        /// <remarks>
        /// Unit second
        /// </remarks>
        public int AutoSaveIntervalTime
        {
            get => _autoSaveIntervalTime_bf;
            set {
                if (Utils.SetField(ref _autoSaveIntervalTime_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(AutoSaveIntervalTime)));
                }
            }
        }

        private ProjectAutoSaveOption _autoSave_bf;
        public ProjectAutoSaveOption AutoSave
        {
            get => _autoSave_bf;
            set {
                if (Utils.SetField(ref _autoSave_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(AutoSave)));
                }
            }
        }

        public event Action<EnvironmentContext, PropertyEventArgs>? PropertyChanged;

        public EnvironmentContext(SaveSystem saveSystem)
        {
            saveSystem.SavingConfigurations += configs =>
            {
                configs.Set("project/autosave", (int)AutoSave);
                configs.Set("project/autosave_interval", AutoSaveIntervalTime);
            };
            saveSystem.LoadedConfigurations += configs =>
            {
                AutoSave = (ProjectAutoSaveOption)configs.GetInt32("project/autosave", (int)ProjectAutoSaveOption.Off);
                var autosaveintervaltime = configs.GetInt32("project/autosave_interval", -1);
                AutoSaveIntervalTime = autosaveintervaltime <= 0 ? DefaultAutoSaveIntervalTime : autosaveintervaltime;
            };
        }
    }
}

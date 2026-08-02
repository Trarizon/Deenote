using Deenote.Core.Notification;
using Deenote.Models;
using ObservableCollections;
using System;
using UnityEngine.Pool;

namespace Deenote.Contexts.EditorModels
{
    public sealed partial class ChartEditorModel : INotifyPropertyChanged<ChartEditorModel>
    {
        #region Basic properties

        private string _name = "";
        private Difficulty _difficulty;
        private string _level = "";
        private float _speed;
        private int _remapMinVolume;
        private int _remapMaxVolume;

        public string Name
        {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    PropertyChanged?.Invoke(this, new(nameof(Name)));
                }
            }
        }

        public Difficulty Difficulty
        {
            get => _difficulty;
            set {
                if (_difficulty != value) {
                    _difficulty = value;
                    PropertyChanged?.Invoke(this, new(nameof(Difficulty)));
                }
            }
        }

        public string Level
        {
            get => _level;
            set {
                if (_level != value) {
                    _level = value;
                    PropertyChanged?.Invoke(this, new(nameof(Level)));
                }
            }
        }

        public float Speed
        {
            get => _speed;
            set {
                if (_speed != value) {
                    _speed = value;
                    PropertyChanged?.Invoke(this, new(nameof(Speed)));
                }
            }
        }

        public int RemapMinVolume
        {
            get => _remapMinVolume;
            set {
                if (_remapMinVolume != value) {
                    _remapMinVolume = value;
                    PropertyChanged?.Invoke(this, new(nameof(RemapMinVolume)));
                }
            }
        }

        public int RemapMaxVolume
        {
            get => _remapMaxVolume;
            set {
                if (_remapMaxVolume != value) {
                    _remapMaxVolume = value;
                    PropertyChanged?.Invoke(this, new(nameof(RemapMaxVolume)));
                }
            }
        }

        #endregion

        public ObservableList<NoteEditorModel> Notes { get; } = new();
        public event Action<ChartEditorModel, PropertyEventArgs>? PropertyChanged;

        public ChartEditorModel(ChartModel model)
        {
            _name = model.Name;
            _difficulty = model.Difficulty;
            _level = model.Level;
            _speed = model.Speed;
            _remapMinVolume = model.RemapMinVolume;
            _remapMaxVolume = model.RemapMaxVolume;
        }

    
    }
}

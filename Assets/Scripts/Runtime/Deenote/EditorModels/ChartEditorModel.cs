using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library.Collections.Generic;
using ObservableCollections;
using System;
using System.Collections;

namespace Deenote.EditorModels
{
    public class ChartEditorModel : INotifyPropertyChanged<ChartEditorModel?>
    {
        internal ChartModel _model;
        public event Action<ChartEditorModel?, PropertyChangedEventArgs>? PropertyChanged;

        private string _name_bf = "";
        public string Name
        {
            get => _name_bf;
            set {
                if (_name_bf != value) {
                    _name_bf = value;
                    _model.Name = value;
                    PropertyChanged?.Invoke(this, nameof(Name));
                }
            }
        }

        private Difficulty _difficulty_bf;
        public Difficulty Difficulty
        {
            get => _difficulty_bf;
            set {
                if (_difficulty_bf != value) {
                    _difficulty_bf = value;
                    _model.Difficulty = value;
                    PropertyChanged?.Invoke(this, nameof(Difficulty));
                }
            }
        }

        private string _level_bf = "";
        public string Level
        {
            get => _level_bf;
            set {
                if (_level_bf != value) {
                    _level_bf = value;
                    _model.Level = value;
                    PropertyChanged?.Invoke(this, nameof(Level));
                }
            }
        }

        private float _speed_bf;
        public float Speed
        {
            get => _speed_bf;
            set {
                if (_speed_bf != value) {
                    _speed_bf = value;
                    _model.Speed = value;
                    PropertyChanged?.Invoke(this, nameof(Speed));
                }
            }
        }

        private int _remapVMin_bf;
        public int RemapMinVolume
        {
            get => _remapVMin_bf;
            set {
                if (_remapVMin_bf != value) {
                    _remapVMin_bf = value;
                    _model.RemapMinVolume = value;
                    PropertyChanged?.Invoke(this, nameof(RemapMinVolume));
                }
            }
        }

        private int _remapVMax_bf;
        public int RemapMaxVolume
        {
            get => _remapVMax_bf;
            set {
                if (_remapVMax_bf != value) {
                    _remapVMax_bf = value;
                    _model.RemapMaxVolume = value;
                    PropertyChanged?.Invoke(this, nameof(RemapMaxVolume));
                }
            }
        }

        public SortedList<IStageNoteNode> NoteNodes => _model.NoteNodes;
        public SortedList<SoundNoteModel> BackgroundSoundNotes => _model.BackgroundSoundNotes;
        public SortedList<SpeedChangeWarningModel> SpeedChangeWarnings => _model.SpeedChangeWarnings;
        public SortedList<SpeedLineValueModel> SpeedLines => _model.SpeedLines;

        public int MaxCombo => _model.NoteCount;

        public ChartEditorModel(ChartModel model)
        {
            _model = model;
            Name = model.Name;
            Difficulty = model.Difficulty;
            Level = model.Level;
            Speed = model.Speed;
            RemapMinVolume = model.RemapMinVolume;
            RemapMaxVolume = model.RemapMaxVolume;
        }

        public ChartModel ToModel() => _model;
    }
}


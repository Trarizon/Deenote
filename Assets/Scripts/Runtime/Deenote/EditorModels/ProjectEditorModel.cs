using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using ObservableCollections;
using System;
using System.IO;
using UnityEngine;

namespace Deenote.EditorModels
{
    public class ProjectEditorModel : INotifyPropertyChanged<ProjectEditorModel?>
    {
        internal ProjectModel _model;
        public event Action<ProjectEditorModel, PropertyChangedEventArgs>? PropertyChanged;

        private string _musicName_bf = "";
        public string MusicName
        {
            get => _musicName_bf;
            set {
                if (_musicName_bf != value) {
                    _musicName_bf = value;
                    _model.MusicName = value;
                    PropertyChanged?.Invoke(this, nameof(MusicName));
                }
            }
        }
        private string _composer_bf = "";
        public string Composer
        {
            get => _composer_bf;
            set {
                if (_composer_bf != value) {
                    _composer_bf = value;
                    _model.Composer = value;
                    PropertyChanged?.Invoke(this, nameof(Composer));
                }
            }
        }
        private string _chartDesigner_bf = "";
        public string ChartDesigner
        {
            get => _chartDesigner_bf;
            set {
                if (_chartDesigner_bf != value) {
                    _chartDesigner_bf = value;
                    _model.ChartDesigner = value;
                    PropertyChanged?.Invoke(this, nameof(ChartDesigner));
                }
            }
        }

        private byte[] _audioFileBytes_bf = Array.Empty<byte>();
        public byte[] AudioFileData
        {
            get => _audioFileBytes_bf;
            private set {
                if (_audioFileBytes_bf != value) {
                    _audioFileBytes_bf = value;
                    _model.AudioFileData = value;
                    // Notified by SetAudio
                }
            }
        }

        private string _audioFileRelativePath_bf = "";
        /// <summary>
        /// Relative to ProjectFilePath, is not used if not <see cref="SaveAsRefPath"/>, but
        /// still requires this to get the audio type (file extension)
        /// </summary>
        public string AudioFileRelativePath
        {
            get => _audioFileRelativePath_bf;
            private set {
                if (_audioFileRelativePath_bf != value) {
                    _audioFileRelativePath_bf = value;
                    _model.AudioFileRelativePath = value;
                    // Notified by SetAudio
                }
            }
        }

        /// <summary>
        /// Used only to tell GamePlayManager if the AudioClip should be loaded from data or not.
        /// </summary>
        private AudioClip? _decodedClip_bf;
        internal AudioClip? DecodedClip
        {
            get => _decodedClip_bf;
            private set {
                if (_decodedClip_bf != value) {
                    _decodedClip_bf = value;
                    // Notified by SetAudio
                }
            }
        }

        public void SetAudio(byte[] bytes, string fileRelativePath, AudioClip? decodedClip = null)
        {
            AudioFileData = bytes;
            AudioFileRelativePath = Path.GetRelativePath(ProjectFilePath, fileRelativePath);
            DecodedClip = decodedClip;
            PropertyChanged?.Invoke(this, new(nameof(AudioFileData)));
            PropertyChanged?.Invoke(this, new(nameof(AudioFileRelativePath)));
            PropertyChanged?.Invoke(this, new(nameof(DecodedClip)));
        }

        public ObservableList<ChartModel> Charts { get; }

        public ObservableList<Tempo> Tempos { get; }

        private string _projectFilePath_bf = "";
        public string ProjectFilePath
        {
            get => _projectFilePath_bf;
            set {
                if (_projectFilePath_bf != value) {
                    _projectFilePath_bf = value;
                    _model.ProjectFilePath = value;
                    PropertyChanged?.Invoke(this, nameof(ProjectFilePath));
                }
            }
        }

        public ProjectEditorModel(ProjectModel model)
        {
            _model = model;
            MusicName = model.MusicName;
            Composer = model.Composer;
            ChartDesigner = model.ChartDesigner;
            AudioFileRelativePath = model.AudioFileRelativePath;
            ProjectFilePath = model.ProjectFilePath;
            Charts = new(model.Charts);
            Tempos = new();
            Tempos.AddRange(model.Tempos);
        }

        public ProjectEditorModel(ProjectModel model, AudioClip decodedClip) : this(model)
        {
            DecodedClip = decodedClip;
        }

        public ProjectModel ToModel()
        {
            return _model.CloneForSave();
        }

        public ModelOperations.InsertTempoOperation InsertTempo(TempoRange range)
        {
            return this._model.InsertTempo(range);
        }
    }
}

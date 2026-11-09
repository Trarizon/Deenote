#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed partial class ProjectModel : INotifyPropertyChanged<ProjectModel?>
    {
        public event Action<ProjectModel, PropertyChangedEventArgs>? PropertyChanged;

        private string _musicName_bf = "";
        public string MusicName
        {
            get => _musicName_bf;
            set {
                if (_musicName_bf != value) {
                    _musicName_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MusicName)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Composer)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChartDesigner)));
                }
            }
        }

        public byte[] AudioFileData { get; set; }

        private string _audioFileRelativePath_bf = "";
        /// <summary>
        /// Relative to ProjectFilePath, is not used if not <see cref="SaveAsRefPath"/>, but
        /// still requires this to get the audio type (file extension)
        /// </summary>
        public string AudioFileRelativePath
        {
            get => _audioFileRelativePath_bf;
            set {
                if (_audioFileRelativePath_bf != value) {
                    _audioFileRelativePath_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioFileRelativePath)));
                }
            }
        }

        public List<ChartModel> Charts { get; } = new();
        internal List<Tempo> _tempos = new();

        public ReadOnlySpan<Tempo> Tempos => _tempos.AsSpan();

        // Non Serialize

        private string _projectFilePath_bf = "";
        public string ProjectFilePath
        {
            get => _projectFilePath_bf;
            [Obsolete("temp public")]
            set {
                if (_projectFilePath_bf != value) {
                    _projectFilePath_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProjectFilePath)));
                }
            }
        }

        public float? AudioLength { get; set; }

        public ProjectModel(string projectFilePath, byte[] audioFileData, string audioFileRelativePath)
        {
            AudioFileData = audioFileData;
            AudioFileRelativePath = audioFileRelativePath;
            ProjectFilePath = projectFilePath;
        }

        public ProjectModel CloneForSave()
        {
            var proj = new ProjectModel(ProjectFilePath, AudioFileData, AudioFileRelativePath) {
                MusicName = MusicName,
                Composer = Composer,
                ChartDesigner = ChartDesigner,
            };
            proj.Charts.Capacity = Charts.Count;
            foreach (var chart in Charts) {
                proj.Charts.Add(chart.Clone());
            }
            proj._tempos.Capacity = _tempos.Count;
            foreach (var tempo in _tempos) {
                proj._tempos.Add(new Tempo(tempo.Bpm, tempo.StartTime));
            }
            return proj;
        }
    }
}
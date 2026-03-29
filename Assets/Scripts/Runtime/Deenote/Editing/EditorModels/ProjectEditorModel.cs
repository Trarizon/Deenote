#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Projects;
using Deenote.CoreB.Notification;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class ProjectEditorModel : INotifyPropertyChanged<ProjectEditorModel>
    {
        private string _musicName_bf = "";
        public string MusicName
        {
            get => _musicName_bf;
            set {
                if (Utils.SetField(ref _musicName_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(MusicName)));
                }
            }
        }

        private string _composer_bf = "";
        public string Composer
        {
            get => _composer_bf;
            set {
                if (Utils.SetField(ref _composer_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Composer)));
                }
            }
        }

        private string _chartDesigner_bf = "";
        public string ChartDesigner
        {
            get => _chartDesigner_bf;
            set {
                if (Utils.SetField(ref _chartDesigner_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(ChartDesigner)));
                }
            }
        }

        private string _audioFileRelativePath_bf = "";
        public string AudioFileRelativePath
        {
            get => _audioFileRelativePath_bf;
            private set {
                if (Utils.SetField(ref _audioFileRelativePath_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(AudioFileRelativePath)));
                }
            }
        }

        private byte[] _audioFileData = Array.Empty<byte>();

        private AudioClip? _audioClip_bf;
        public AudioClip? AudioClip
        {
            get => _audioClip_bf;
            private set {
                if (Utils.SetField(ref _audioClip_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(AudioClip)));
                }
            }
        }

        private float? _audioLength_bf;
        public float? AudioLength
        {
            get => _audioLength_bf;
            private set {
                if (Utils.SetField(ref _audioLength_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(AudioLength)));
                }
            }
        }

        public List<ChartEditorModel> Charts { get; } = new();
        public List<Tempo> Tempos { get; } = new();

        public string ProjectFilePath { get; private set; }

        public event Action<ProjectEditorModel, PropertyEventArgs>? PropertyChanged;

        public ProjectEditorModel(ProjectModel model, string projectFilePath)
        {
            _musicName_bf = model.MusicName;
            Composer = model.Composer;
            ChartDesigner = model.ChartDesigner;
            _audioFileData = model.AudioFileData;
            AudioFileRelativePath = model.AudioFileRelativePath;
            ProjectFilePath = projectFilePath;

            foreach (var chart in model.Charts) {
                Charts.Add(new ChartEditorModel(chart));
            }
            if (Charts.Count == 0) {
                Charts.Add(new ChartEditorModel());
            }

            foreach (var tempo in model.Tempos) {
                Tempos.Add(tempo);
            }
        }

        public ProjectEditorModel(string projectFilePath, AudioClip clip)
        {
            ProjectFilePath = projectFilePath;
            AudioClip = clip;
            AudioLength = clip.length;

            if (Charts.Count == 0) {
                Charts.Add(new ChartEditorModel());
            }
        }

        #region Charts

        public ChartEditorModel AddChart(ChartModel chart)
        {
            var model = new ChartEditorModel(chart);
            AddChartEditorModel(model);
            return model;
        }

        internal void AddChartEditorModel(ChartEditorModel chart)
        {
            Charts.Add(chart);
            PropertyChanged?.Invoke(this, new(nameof(Charts)));
        }

        public bool RemoveChartAt(int index)
        {
            if ((uint)index >= (uint)Charts.Count)
                return false;
            Charts.RemoveAt(index);

            if (Charts.Count == 0) {
                Charts.Add(new ChartEditorModel());
            }

            PropertyChanged?.Invoke(this, new(nameof(Charts)));
            return true;
        }

        #endregion

        #region Tempos

        /// <returns>Range: [-1, Count)</returns>
        public int GetTempoIndex(float time)
        {
            int i = 0;
            for (; i < Tempos.Count; i++) {
                if (Tempos[i].StartTime > time)
                    break;
            }
            return i - 1;
        }

        /// <returns>Range: [0, Count]</returns>
        public int GetCeilingTempoIndex(float time)
        {
            int i = 0;
            for (; i < Tempos.Count; i++) {
                if (Tempos[i].StartTime >= time)
                    break;
            }
            return i;
        }

        /// <returns>
        /// if <paramref name="index"/> less than 0, return a <see cref="Tempo"/>
        /// with 0 bpm and 0 start time
        /// </returns>
        public Tempo GetActualTempo(int index)
        {
            if (index < 0)
                return new Tempo(0f, 0f);
            return Tempos[index];
        }

        /// <returns>
        /// if <paramref name="index"/> is out of range,
        /// returns 0 or the audio's length
        /// </returns>
        public float GetNonOverflowTempoTime(int index)
        {
            if (index >= Tempos.Count) {
                if (AudioLength is { } len)
                    return len;
                else if (Tempos.Count > 0)
                    return Tempos[^1].StartTime + 3f;
                else
                    return 3f;
            }
            if (index < 0)
                return 0f;
            return Tempos[index].StartTime;
        }

        #endregion

        #region Audio

        public async UniTask<bool> TrySetAudioByFilePathAsync(string audioFilePath, CancellationToken cancellationToken = default)
        {
            var audioRelativePath = Path.GetRelativePath(ProjectFilePath, audioFilePath);
            var audioFileData = await File.ReadAllBytesAsync(audioFilePath);
            using var ms = new MemoryStream(audioFileData);
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(audioRelativePath), cancellationToken);
            if (clip is null)
                return false;

            AudioClip = clip;
            AudioLength = clip.length;
            AudioFileRelativePath = audioRelativePath;
            _audioFileData = audioFileData;

            return true;

        }

        public async UniTask<bool> LoadAudioClipAsync(CancellationToken cancellationToken = default)
        {
            AudioClip = null;
            AudioLength = null;
            using var ms = new MemoryStream(_audioFileData);
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(AudioFileRelativePath), cancellationToken);
            if (clip is null)
                return false;
            AudioClip = clip;
            AudioLength = clip.length;
            return true;
        }

        #endregion

        public ProjectModel ToModel()
        {
            var project = new ProjectModel {
                AudioFileData = _audioFileData,
                AudioFileRelativePath = AudioFileRelativePath,
                ChartDesigner = ChartDesigner,
                Composer = Composer,
                MusicName = MusicName,
            };
            foreach (var chart in Charts) {
                project.Charts.Add(chart.ToModel());
            }
            project.Tempos.AddRange(Tempos.AsSpan());

            return project;
        }
    }
}

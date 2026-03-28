#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Projects;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class ProjectEditorModel : ObservableObject
    {
        [ObservableProperty] string _musicName = "";
        [ObservableProperty] string _composer = "";
        [ObservableProperty] string _chartDesigner = "";
        private byte[] _audioFileData = Array.Empty<byte>();
        [ObservableProperty] string _audioFileRelativePath = "";

        [ObservableProperty] AudioClip? _audioClip;

        public List<ChartEditorModel> Charts { get; } = new();
        public List<Tempo> Tempos { get; } = new();

        public string ProjectFilePath { get; private set; }
        public float? AudioLength { get; private set; }

        public ProjectEditorModel(ProjectModel model, string projectFilePath)
        {
            _musicName = model.MusicName;
            _composer = model.Composer;
            _chartDesigner = model.ChartDesigner;
            _audioFileData = model.AudioFileData;
            _audioFileRelativePath = model.AudioFileRelativePath;
            ProjectFilePath = projectFilePath;
        }

        internal ProjectEditorModel(string projectFilePath, AudioClip clip)
        {
            ProjectFilePath = projectFilePath;
            _audioClip = clip;
            AudioLength = clip.length;
        }

        #region Tempo

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

        public async UniTask<bool> TrySetAndLoadAudioAsync(string audioFilePath, CancellationToken cancellationToken = default)
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
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(AudioFileRelativePath),cancellationToken);
            if (clip is null)
                return false;
            AudioClip = clip;
            AudioLength = clip.length;
            return true;
        }

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

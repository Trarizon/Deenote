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
using UnityEngine;

namespace Deenote.Editing.EditorModels
{
    internal sealed partial class ProjectEditorModel : ObservableObject
    {
        [ObservableProperty] string _musicName = "";
        [ObservableProperty] string _composer = "";
        [ObservableProperty] string _chartDesigner = "";
        [ObservableProperty] byte[] _audioFileData = Array.Empty<byte>();
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

        public async UniTask<bool> LoadAudioClipAsync()
        {
            AudioClip = null;
            AudioLength = null;
            using var ms = new MemoryStream(AudioFileData);
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(AudioFileRelativePath));
            if (clip is null)
                return false;
            AudioClip = clip;
            AudioLength = clip.length;
            return true;
        }

        public ProjectModel ToModel()
        {
            var project = new ProjectModel {
                AudioFileData = AudioFileData,
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

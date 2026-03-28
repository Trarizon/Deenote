#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote
{
#if UNITY_EDITOR

    public static class Fake
    {
        private static ProjectEditorModel? _project;
        private static AudioClip? _audio;

        private const string TestMusic = "finale";

        private static string GetDevPath(string relativePathToAssets)
            => Path.Combine(Application.dataPath, "Dev", relativePathToAssets);

        public static async Task<ProjectEditorModel> GetProject()
        {
            if (_project is not null) return _project;

            //await using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var clip = await AudioUtils.LoadAsync(fs, ".mp3");
            using var fs = File.OpenRead(GetDevPath($"TestCharts/{TestMusic}.mp3"));
            var clip = await AudioUtils.TryLoadAsync(fs, ".mp3");
            if (clip is null) {
                Debug.LogError($"Load test audio failed in {nameof(Fake)}.{nameof(GetProject)}()");
                return null!;
            }
            _audio = clip;

            _project = new ProjectEditorModel("D:/Fake.mp3", clip);
            //Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text
            if (ChartData.TryParse(File.ReadAllText(GetDevPath($"TestCharts/{TestMusic}.hard.json")), out var chart)) {
                var chartModel = new ChartModel(chart) {
                    Level = "10",
                    Difficulty = Difficulty.Hard,
                };
                _project.Charts.Add(new ChartEditorModel(chartModel));
                _project.Tempos.Add(new Tempo(160f, 1.5f));
            }
            else {
                Debug.LogError("Load test chart failed");
            }
            return _project;
        }
    }

#endif
}
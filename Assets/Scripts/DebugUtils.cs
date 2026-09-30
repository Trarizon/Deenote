using Cysharp.Threading.Tasks;
using Deenote.CoreB.Helpers;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR

namespace Deenote
{
    public static class DebugUtils
    {
        private const string TestMusic = "finale";

        private static string GetDevPath(string relativePathToAssets)
            => Path.Combine(Application.dataPath, "Dev", relativePathToAssets);

        public static async UniTask<(ProjectModel, AudioClip)> GetTestProjectAsync()
        {
            using var fs = File.OpenRead(GetDevPath($"TestCharts/{TestMusic}.mp3"));
            var clip = await AudioUtils.TryLoadAsync(fs, ".mp3");
            if (clip is null) {
                Debug.LogError("Load audio failed");
            }

            var project = new ProjectModel("D:/", null!, "Fake.mp3");
            //Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text
            if (ChartModel.TryParse(File.ReadAllText(GetDevPath($"TestCharts/{TestMusic}.hard.json")), out var chart)) {
                // chart.Name = "<cht> name";
                chart.Level = "10";
                chart.Difficulty = Difficulty.Hard;
                project.Charts.Add(chart);
                ((IUndoableOperation)(project.InsertTempo(new TempoRange(160f, 1.5f, 11111f)))).Redo();
            }
            else {
                Debug.LogError("Load test chart failed");
            }
            return (project, clip)!;

        }
    }
}

#endif

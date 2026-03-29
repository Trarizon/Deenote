#nullable enable

using Deenote;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Projects;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

internal static class DebugUtils
{
    private const string TestMusic = "finale";

    private static string GetDevPath(string relativePathToAssets)
        => Path.Combine(Application.dataPath, "Dev", relativePathToAssets);

    public static async Task<ProjectEditorModel> GetTestProject()
    {
        using var fs = File.OpenRead(GetDevPath($"TestCharts/{TestMusic}.mp3"));
        var clip = await AudioUtils.TryLoadAsync(fs, ".mp3");

        if (clip is null) {
            Debug.LogError($"Load test audio failed.");
            return null!;
        }

        var project = new ProjectEditorModel("D:/Fake.mp3", clip);
        //Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text
        if (ChartData.TryParse(File.ReadAllText(GetDevPath($"TestCharts/{TestMusic}.hard.json")), out var chart)) {
            var chartModel = new ChartModel(chart) {
                Level = "10",
                Difficulty = Difficulty.Hard,
            };
            project.Charts.Add(new ChartEditorModel(chartModel));
            project.Tempos.Add(new Tempo(160f, 1.5f));
        }
        else {
            Debug.LogError("Load test chart failed");
        }
        return project;
    }
}
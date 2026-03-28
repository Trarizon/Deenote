#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Projects;
using Deenote.Library;
using System;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    partial class NewProjectDialog
    {
        private const string NewProjectCreatingStatusKey = "NewProject_Status_Creating";
        private const string NewProjectCreatedStatusKey = "NewProject_Status_Created";
        private const string NewProjectCreateCancelledStatusKey = "NewProject_Status_CreateCancelled";

        public async UniTask<Result?> OpenCreateNewAsync()
        {
            OpenSelfModalDialog();
            ResetDialog();

            _cts.CancelAndReset();

            try {
            ReAwaitButtonClick:

                await _createButton.OnClickAsync(_cts.Token);

                if (_projectResultPath is null) {
                    Debug.Assert(false, "Unexcepted value");
                    goto ReAwaitButtonClick;
                }

                string audioFilePath = _audioFilePath.Text;
                if (!File.Exists(audioFilePath)) {
                    await MainWindow.DialogManager.OpenMessageBoxAsync(_audioNotExistsMsgBoxArgs);
                    goto ReAwaitButtonClick;
                }
                if (Directory.Exists(_projectResultPath)) {
                    await MainWindow.DialogManager.OpenMessageBoxAsync(_dirExistsMsgBoxArgs);
                    goto ReAwaitButtonClick;
                }
                if (File.Exists(_projectResultPath)) {
                    var res = await MainWindow.DialogManager.OpenMessageBoxAsync(_fileExistsMsgBoxArgs);
                    if (res != 0)
                        goto ReAwaitButtonClick;
                }

                // Project creation

                MainWindow.StatusBar.SetLocalizedStatusMessage(NewProjectCreatingStatusKey);

                {
                    // LoadAudio
                    // TODO: Lazy Load Audio
                    using var audioFs = File.OpenRead(audioFilePath);
                    AudioClip? clip;
                    SetBlockInput(true);
                    clip = await AudioUtils.TryLoadAsync(audioFs, Path.GetExtension(audioFilePath), _cts.Token);
                    if (clip is null) {
                        await MainWindow.DialogManager.OpenMessageBoxAsync(_audioLoadFailedMsgBoxArgs);
                        SetBlockInput(false);
                        goto ReAwaitButtonClick;
                    }

                    // CreateProject

                    byte[] audioBytes = new byte[audioFs.Length];
                    audioFs.Seek(0, SeekOrigin.Begin);
                    audioFs.Read(audioBytes);

                    var proj = new ProjectModel {
                        MusicName = _projectName.Text,
                        AudioFileData = audioBytes,
                        AudioFileRelativePath = Path.GetRelativePath(_projectResultPath, audioFilePath)
                    };
                    proj.Charts.Add(new ChartModel {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    });

                    MainWindow.StatusBar.SetLocalizedStatusMessage(NewProjectCreatedStatusKey);
                    return new Result(proj, _projectResultPath);
                }
            } catch (OperationCanceledException) {
                MainWindow.StatusBar.SetLocalizedStatusMessage(NewProjectCreateCancelledStatusKey);
                return null;
            } finally {
                SetBlockInput(false);
                CloseSelfModalDialog();
            }
        }

        public readonly record struct Result(
            ProjectModel Project,
            string ProjectPath);
    }
}
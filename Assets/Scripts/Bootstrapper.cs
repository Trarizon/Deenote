using Cysharp.Threading.Tasks;
using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Localization;
using Deenote.GamePlay.Audio;
using Deenote.Systems;
using Deenote.UI;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace Deenote
{
    [DefaultExecutionOrder(-99)]
    internal sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] GameMusicPlayer _gameMusicPlayer;
        [SerializeField] PianoSoundSource _pianoSoundSource;
        [SerializeField] HitSoundSource _hitSoundSource;
        [SerializeField] HitSoundPlayer _hitSoundPlayer;
        [SerializeField] PerspectiveViewPanelView _perspectiveViewPanel;

        private const string ExceptionLogFilePath = "exceptions.log";

        private static readonly MessageBoxArgs _quitUnsavedMsgBoxArgs = new(
            LocalizableText.Localized("Quit_MsgBox_Title"),
            LocalizableText.Localized("QuitUnsaved_MsgBox_Content"),
            LocalizableText.Localized("Quit_MsgBox_Y"),
            LocalizableText.Localized("Quit_MsgBox_N"));

        private static readonly MessageBoxArgs _verUpdMsgBoxArgs = new(
            LocalizableText.Localized("NewVersion_MsgBox_Title"),
            LocalizableText.Localized("NewVersion_MsgBox_Content"),
            LocalizableText.Localized("NewVersion_MsgBox_2"),
            LocalizableText.Localized("NewVersion_MsgBox_1"),
            LocalizableText.Localized("NewVersion_MsgBox_N"));

        void Awake()
        {
            var app = App.Create(_perspectiveViewPanel, _gameMusicPlayer, _pianoSoundSource, _hitSoundSource, _hitSoundPlayer);
            
            Debug.Log("Bootstrapper Awake");
        }

        void Start()
        {
            // Quit

            App.Current.Quitting += QuitRegistration;
            App.Current.Quitted += () =>
            {
                MainSystem.SaveSystem.SaveConfigurations();
            };
            void QuitRegistration(CancelEventArgs e)
            {
                if (!MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange) {
                    return;
                }

                e.Cancel = true;
                var res = MainWindow.DialogManager.OpenMessageBoxAsync(_quitUnsavedMsgBoxArgs)
                    .ContinueWith(val =>
                    {
                        if (val == 0) {
                            App.Current.Quitting -= QuitRegistration;
                            App.Current.Quit();
                        }
                    });
            }

            // Exception

            App.Current.ExceptionHandler.UnhandledExceptionOccurred += e =>
            {
                using var sw = new StreamWriter(ExceptionLogFilePath, append: true);
                if (e.Exception is null) {
                    sw.WriteLine($"Unhandled Exception: {e.Message}\n{e.StackTrace}");
                }
                else {
                    var ex = e.Exception;
                    sw.Write("Unhandled Exception: ");
                    sw.Write(ex.Message);
                    var inner = ex.InnerException;
                    while (inner is not null) {
                        sw.Write($", {inner.Message}");
                        inner = inner.InnerException;
                    }
                    sw.WriteLine();
                    sw.WriteLine(ex.StackTrace);
                }
                sw.WriteLine();
            };

            // Version

#if UNITY_EDITOR
            App.Logger.LogInfo("Ignored version check in editor");
#else
            if (App.Environment.CheckUpdateOnStartup) {
                CheckUpdateAsync().Forget();
            }
#endif
        }

        private async UniTask CheckUpdateAsync()
        {
            var res = await App.Current.VersionManager.CheckUpdateAsync();
            if (res.Kind is VersionManager.UpdateCheckResultKind.UpdateAvailable) {
                var clicked = await MainWindow.DialogManager.OpenMessageBoxAsync(_verUpdMsgBoxArgs,
                    App.Current.VersionManager.CurrentVersion.ToString(),
                    res.LatestVersion.ToString());
                switch (clicked) {
                    case 0: App.Current.VersionManager.OpenReleasePage(); break;
                    case 1: App.Current.VersionManager.OpenDownloadPage(res.LatestVersion); break;
                }
            }
            // We do not pop up any fail message on startup-check
        }
    }
}
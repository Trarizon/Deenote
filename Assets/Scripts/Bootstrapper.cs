using Cysharp.Threading.Tasks;
using Deenote.CoreB.Localization;
using Deenote.UI;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views;
using System.ComponentModel;
using UnityEngine;

namespace Deenote
{
    [DefaultExecutionOrder(-99)]
    internal sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] PerspectiveViewPanelView _perspectiveViewPanel;

        private static readonly MessageBoxArgs _quitUnsavedMsgBoxArgs = new(
            LocalizableText.Localized("Quit_MsgBox_Title"),
            LocalizableText.Localized("QuitUnsaved_MsgBox_Content"),
            LocalizableText.Localized("Quit_MsgBox_Y"),
            LocalizableText.Localized("Quit_MsgBox_N"));

        void Awake()
        {
            var app = App.Create(_perspectiveViewPanel);

            Debug.Log("Bootstrapper Awake");
        }

        void Start()
        {
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
        }
    }
}
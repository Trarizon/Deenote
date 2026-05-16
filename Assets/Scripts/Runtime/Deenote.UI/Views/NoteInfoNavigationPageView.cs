#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core.Editing;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing;
using Deenote.Editing.EditorModels;
using Deenote.GameStage;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UI.Helpers;
using Deenote.UI.Views.Panels;
using Deenote.UIFramework.Controls;
using System;
using UnityEditor;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class NoteInfoNavigationPageView : MonoBehaviour
    {
        private GameStageContext _stageContext;
        private EditorContext _editorContext;
        private ChartNotesEditor _editor;

        [SerializeField] TextBlock _noteHeaderText = default!;
        [SerializeField] TextBox _positionInput = default!;
        [SerializeField] TextBox _timeInput = default!;
        [SerializeField] TextBox _sizeInput = default!;
        [SerializeField] TextBox _durationInput = default!;
        [SerializeField] Button _linkAsHoldButton = default!;
        [SerializeField] ToggleButtonGroup _noteKindToggleGroup = default!;
        [SerializeField] ToggleButton _clickNoteKindToggle = default!;
        [SerializeField] ToggleButton _slideNoteKindToggle = default!;
        [SerializeField] ToggleButton _swipeNoteKindToggle = default!;
        [SerializeField] TextBox _speedInput = default!;
        [SerializeField] Button _speedToPlaceSpeedButton = default!;
        [SerializeField] Button _soundsButton = default!;
        [SerializeField] Button _soundsQuickAddRemoveButton = default!;
        [SerializeField] NoteInfoPianoSoundEditPanel _soundEditPanel = default!;
        [SerializeField] TextBox _shiftInput = default!;
        [SerializeField] TextBox _eventIdInput = default!;
        //[SerializeField] Dropdown _warningTypeDropdown = default!;
        [SerializeField] CheckBox _vibrateCheckBox = default!;

        [SerializeField] GameObject[] _ineffectivePropertyGameObjects = default!;
        private IInteractableControl[] _interactableControls = default!;

        #region Localization Keys

        private const string NoteNonSelectedHeader = "NavPanel_Note_Header";
        private const string NoteSelectedHeader = "NavPanel_NoteSelected_Header";
        private const string MultipleValuesPlaceHolderKey = "NavPanel_NotePropertyMultipleValue_PlaceHolder";

        #endregion

        private const string NoSoundButtonText = "-";

        private float? _selectedNotesSpeed;
        private bool _isSoundQuickAdd;

        private void Awake()
        {
            _stageContext = MainSystem.Contexts.GameStage;
            _editorContext = MainSystem.Contexts.Editor;
            _editor = MainSystem.ChartEditor;

            _interactableControls = new IInteractableControl[] {
                _positionInput, _timeInput, _sizeInput, _durationInput,
                _clickNoteKindToggle, _slideNoteKindToggle, _swipeNoteKindToggle,
                _speedInput, _shiftInput, _eventIdInput,
                _vibrateCheckBox,
            };
        }

        private void Start()
        {
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.IneffectivePropertiesVisible,
                settings =>
                {
                    var visible = settings.IsIneffectivePropertiesVisible;
                    foreach (var go in _ineffectivePropertyGameObjects) {
                        go.SetActive(visible);
                    }
                });

            // Properties
            #region Position Time Size Duration

            _positionInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesPosition(_editorContext.NoteSelection.SelectedNotes, value);
                // Re-sync value, as StageChartEditor may clamp value
                NotifyMultiFloatValueChanged(_positionInput, _editorContext.NoteSelection.SelectedNotes, n => n.Position);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NotePosition,
                editor => NotifyMultiFloatValueChanged(_positionInput, editor.Selector.SelectedNotes, n => n.Position));
            _timeInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesTime(_editorContext.NoteSelection.SelectedNotes, value);
                NotifyMultiFloatValueChanged(_timeInput, _editorContext.NoteSelection.SelectedNotes, n => n.Time);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteTime,
                editor => NotifyMultiFloatValueChanged(_timeInput, editor.Selector.SelectedNotes, n => n.Time));
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NotePositionCoord,
                editor =>
                {
                    NotifyMultiFloatValueChanged(_positionInput, editor.Selector.SelectedNotes, n => n.Position);
                    NotifyMultiFloatValueChanged(_timeInput, editor.Selector.SelectedNotes, n => n.Time);
                });

            _sizeInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesSize(_editorContext.NoteSelection.SelectedNotes, value);
                NotifyMultiFloatValueChanged(_sizeInput, _editorContext.NoteSelection.SelectedNotes, n => n.Size);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteSize,
                editor => NotifyMultiFloatValueChanged(_sizeInput, editor.Selector.SelectedNotes, n => n.Size));

            _durationInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesDuration(_editorContext.NoteSelection.SelectedNotes, value);
                NotifyMultiFloatValueChanged(_durationInput, _editorContext.NoteSelection.SelectedNotes, n => n.Duration);
                SyncFloatInput(_durationInput, value);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteDuration,
                editor => NotifyMultiFloatValueChanged(_durationInput, editor.Selector.SelectedNotes, n => n.Duration));
            _linkAsHoldButton.Clicked += () =>
            {
                var notes = _editorContext.NoteSelection.SelectedNotes;
                Debug.Assert(notes.Length == 2);
                var prev = notes[0];
                var next = notes[1];

                _editor.CreateHoldBetween(prev, next);
            };

            #endregion

            #region Kind Speed Sounds

            _clickNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    _editor.EditNotesKind(_editorContext.NoteSelection.SelectedNotes, NoteKind.Click);
            };
            _slideNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    _editor.EditNotesKind(_editorContext.NoteSelection.SelectedNotes, NoteKind.Slide);
            };
            _swipeNoteKindToggle.IsCheckedChanged += check =>
            {
                if (check)
                    _editor.EditNotesKind(_editorContext.NoteSelection.SelectedNotes, NoteKind.Swipe);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteKind,
                editor => NotifyMultiKindChanged(editor.Selector.SelectedNotes));

            _speedInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesSpeed(_editorContext.NoteSelection.SelectedNotes, value);
                NotifyMultiSpeedValueChanged(_editorContext.NoteSelection.SelectedNotes);
            };
            _speedToPlaceSpeedButton.Clicked += () =>
            {
                if (_selectedNotesSpeed is { } speed)
                    _stageContext.HighlightedNoteSpeed = speed;
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteSpeed,
                editor => NotifyMultiSpeedValueChanged(editor.Selector.SelectedNotes));


            _soundsButton.Clicked += () =>
            {
                _soundEditPanel.IsPanelActive = !_soundEditPanel.IsPanelActive;
                _soundsButton.Image.sprite = _soundEditPanel.IsPanelActive
                    ? MainWindow.Args.UIIcons.NoteInfoSoundsCollapseSprite
                    : MainWindow.Args.UIIcons.NoteInfoSoundsEditSprite;
            };
            _soundsButton.Image.sprite = MainWindow.Args.UIIcons.NoteInfoSoundsEditSprite;
            _soundsQuickAddRemoveButton.Clicked += () =>
            {
                _editor.EditNotesSounds(_editorContext.NoteSelection.SelectedNotes, _isSoundQuickAdd);
            };
            _soundEditPanel.IsDirtyChanged += dirty =>
            {
                _soundsButton.Image.sprite = dirty
                    ? MainWindow.Args.UIIcons.NoteInfoSoundsAcceptSprite
                    : MainWindow.Args.UIIcons.NoteInfoSoundsCollapseSprite;
            };

            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteSounds,
                editor => NotifyMultiSoundsChanged(editor.Selector.SelectedNotes));

            #endregion

            #region Shift Event WarningType Vibrate

            _shiftInput.EditSubmitted += text =>
            {
                if (float.TryParse(text, out var value))
                    _editor.EditNotesShift(_editorContext.NoteSelection.SelectedNotes, value);
                NotifyMultiFloatValueChanged(_shiftInput, _editorContext.NoteSelection.SelectedNotes, n => n.Shift);
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteShift,
                editor => NotifyMultiFloatValueChanged(_shiftInput, editor.Selector.SelectedNotes, n => n.Shift));
            _eventIdInput.EditSubmitted += text =>
            {
                _editor.EditNotesEventId(_editorContext.NoteSelection.SelectedNotes, text);
                // Avoid display place holder
                _eventIdInput.SetPlaceHolderText(LocalizableText.Raw(""));
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteEventId,
                editor => NotifyMultiEventIdChanged(editor.Selector.SelectedNotes));

            //_warningTypeDropdown.ResetOptions(WarningTypeExt.DropdownOptions);
            //_warningTypeDropdown.SelectedIndexChanged += index =>
            //{
            //    if (index >= 0)
            //        MainSystem.StageChartEditor.EditSelectedNotesWarningType(WarningTypeExt.FromIndex(index));
            //};
            //MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
            //    StageChartEditor.NotificationFlag.NoteWarningType,
            //    editor => NotifyMultiWarningTypeChanged(editor.Selector.SelectedNotes));

            _vibrateCheckBox.IsCheckedChanged += check =>
            {
                if (check is { } c) {
                    _editor.EditNotesVibrate(_editorContext.NoteSelection.SelectedNotes, c);
                }
            };
            MainSystem.StageChartEditor.RegisterNotificationAndInvoke(
                StageChartEditor.NotificationFlag.NoteVibrate,
                editor => NotifyMultiBoolValueChanged(_vibrateCheckBox, editor.Selector.SelectedNotes, n => n.Vibrate));

            #endregion

            MainSystem.StageChartEditor.Selector.SelectedNotesChanged += _OnSelectedNotesChanged;
            _OnSelectedNotesChanged(MainSystem.StageChartEditor.Selector);

            void _OnSelectedNotesChanged(StageNoteSelector selector)
            {
                var notes = selector.SelectedNotes;

                _linkAsHoldButton.IsInteractable = notes.Length == 2;

                switch (notes.Length) {
                    case 0:
                        _noteHeaderText.SetLocalizedText(NoteNonSelectedHeader);
                        SetControlsActive(false);
                        _speedToPlaceSpeedButton.IsInteractable = false;
                        NotifyMultiSoundsChanged(notes);
                        break;
                    case 1:
                        _noteHeaderText.SetLocalizedText(NoteSelectedHeader, "1");
                        SetControlsActive(true);
                        var note = notes[0];
                        SyncFloatInput(_positionInput, note.Position);
                        SyncFloatInput(_timeInput, note.Time);
                        SyncFloatInput(_sizeInput, note.Size);
                        SyncFloatInput(_durationInput, note.Duration);
                        switch (note.Kind) {
                            case NoteKind.Click:
                                _clickNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                break;
                            case NoteKind.Slide:
                                _slideNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                break;
                            case NoteKind.Swipe:
                                _swipeNoteKindToggle.SetIsCheckedWithoutNotify(true);
                                break;
                            default:
                                break;
                        }
                        SyncFloatInput(_speedInput, note.Speed);
                        _selectedNotesSpeed = note.Speed;
                        _speedToPlaceSpeedButton.IsInteractable = true;
                        NotifyMultiSoundsChanged(notes);
                        SyncFloatInput(_shiftInput, note.Shift);
                        _eventIdInput.SetValueWithoutNotify(note.EventId);
                        //_warningTypeDropdown.SetValueWithoutNotify(note.WarningType.ToIndex());
                        _vibrateCheckBox.SetValueWithoutNotify(note.Vibrate);
                        break;
                    default:
                        _noteHeaderText.SetLocalizedText(NoteSelectedHeader, notes.Length.ToString());
                        SetControlsActive(true);
                        NotifyMultiFloatValueChanged(_positionInput, notes, n => n.Position);
                        NotifyMultiFloatValueChanged(_timeInput, notes, n => n.Time);
                        NotifyMultiFloatValueChanged(_sizeInput, notes, n => n.Size);
                        NotifyMultiFloatValueChanged(_durationInput, notes, n => n.Duration);
                        NotifyMultiKindChanged(notes);
                        NotifyMultiSpeedValueChanged(notes);
                        NotifyMultiSoundsChanged(notes);
                        NotifyMultiFloatValueChanged(_shiftInput, notes, n => n.Shift);
                        NotifyMultiEventIdChanged(notes);
                        //NotifyMultiWarningTypeChanged(notes);
                        NotifyMultiBoolValueChanged(_vibrateCheckBox, notes, n => n.Vibrate);
                        break;
                }
            }

            void SetControlsActive(bool active)
            {
                if (active) {
                    if (_interactableControls[0].IsInteractable == false) {
                        var textBoxes = ((ReadOnlySpan<IInteractableControl>)_interactableControls.AsSpan()).OfType<IInteractableControl, TextBox>();
                        foreach (var textBox in textBoxes) {
                            textBox.SetPlaceHolderText(LocalizableText.Localized(MultipleValuesPlaceHolderKey));
                        }
                        foreach (var ctrl in _interactableControls)
                            ctrl.IsInteractable = true;
                    }
                }
                else {
                    if (_interactableControls[0].IsInteractable) {
                        var textBoxes = ((ReadOnlySpan<IInteractableControl>)_interactableControls.AsSpan()).OfType<IInteractableControl, TextBox>();
                        foreach (var textBox in textBoxes) {
                            textBox.SetValueWithoutNotify("");
                            textBox.SetPlaceHolderText(LocalizableText.Raw(""));
                        }

                        _noteKindToggleGroup.ForceToggleOff();
                        _soundsButton.Text.SetRawText(NoSoundButtonText);
                        //_warningTypeDropdown.SetValueWithoutNotify(-1);
                        _vibrateCheckBox.SetValueWithoutNotify(null);

                        foreach (var ctrl in _interactableControls)
                            ctrl.IsInteractable = false;
                    }
                }
            }
        }

        private void SyncFloatInput(TextBox textBox, float value)
            => textBox.SetValueWithoutNotify(value.ToString("F3"));

        private void NotifyMultiFloatValueChanged(TextBox textBox, ReadOnlySpan<NoteEditorModel> notes, Func<NoteEditorModel, float> selector)
            => textBox.SetValueWithoutNotify(notes.IsSameForAll(selector, out var value) ? value.ToString("F3") : "");

        private bool NotifyMultiFloatValueChanged(TextBox textBox, ReadOnlySpan<NoteEditorModel> notes, Func<NoteEditorModel, float> selector, out float sameValue)
        {
            bool ret;
            if (notes.IsSameForAll(selector, out sameValue)) {
                ret = true;
                textBox.SetValueWithoutNotify(sameValue.ToString("F3"));
            }
            else {
                ret = false;
                textBox.SetValueWithoutNotify("");
            }
            return ret;
        }

        private void NotifyMultiBoolValueChanged(CheckBox checkBox, ReadOnlySpan<NoteEditorModel> notes, Func<NoteEditorModel, bool> selector)
        {
            if (notes.IsSameForAll(selector, out var value))
                checkBox.SetValueWithoutNotify(value);
            else
                checkBox.SetValueWithoutNotify(null);
        }

        private void NotifyMultiKindChanged(ReadOnlySpan<NoteEditorModel> notes)
        {
            if (notes.IsSameForAll(n => n.Kind, out var kind)) {
                var toggle = kind switch {
                    NoteKind.Click => _clickNoteKindToggle,
                    NoteKind.Slide => _slideNoteKindToggle,
                    NoteKind.Swipe => _swipeNoteKindToggle,
                    _ => ThrowHelper.ThrowInvalidOperationException<ToggleButton>("Unknown note kind"),
                };
                toggle.SetIsCheckedWithoutNotify(true);
            }
            else {
                _noteKindToggleGroup.ForceToggleOff();
            }
        }

        private void NotifyMultiSpeedValueChanged(ReadOnlySpan<NoteEditorModel> notes)
        {
            if (NotifyMultiFloatValueChanged(_speedInput, notes, n => n.Speed, out var speed)) {
                _selectedNotesSpeed = speed;
                _speedToPlaceSpeedButton.IsInteractable = true;
            }
            else {
                _selectedNotesSpeed = null;
                _speedToPlaceSpeedButton.IsInteractable = false;
            }
        }

        private void NotifyMultiSoundsChanged(ReadOnlySpan<NoteEditorModel> notes)
        {
            switch (notes.Length) {
                case 0:
                    _soundsButton.Text.SetRawText(NoSoundButtonText);
                    _soundsQuickAddRemoveButton.IsInteractable = false;
                    break;
                case 1: {
                    var sounds = notes[0].Sounds.AsSpan();
                    _soundsButton.Text.SetRawText(GetSoundsDisplayText(sounds));
                    _soundsQuickAddRemoveButton.IsInteractable = true;
                    SetSoundsQuickActionButton(sounds.Length > 0);
                    break;
                }
                default: {
                    _soundsQuickAddRemoveButton.IsInteractable = true;
                    if (ModelHelpers.HasSameSounds(notes)) {
                        var sounds = notes[0].Sounds.AsSpan();
                        _soundsButton.Text.SetRawText(GetSoundsDisplayText(sounds));
                        SetSoundsQuickActionButton(sounds.Length > 0);
                    }
                    else {
                        _soundsButton.Text.SetRawText(NoSoundButtonText);
                        SetSoundsQuickActionButton(true);
                    }
                    break;
                }

                void SetSoundsQuickActionButton(bool hasSounds)
                {
                    _isSoundQuickAdd = !hasSounds;
                    _soundsQuickAddRemoveButton.Image.sprite = _isSoundQuickAdd
                        ? MainWindow.Args.UIIcons.NoteInfoSoundsQuickAddSprite
                        : MainWindow.Args.UIIcons.NoteInfoSoundsQuickRemoveSprite;
                }
            }

            static string GetSoundsDisplayText(ReadOnlySpan<PianoSoundData> sounds)
            {
                return sounds.Length switch {
                    0 => NoSoundButtonText,
                    1 => sounds[0].ToPitchDisplayString(),
                    2 => $"{sounds[0].ToPitchDisplayString()}, {sounds[1].ToPitchDisplayString()}",
                    _ => sounds.Length.ToString(),
                };
            }
        }

        private void NotifyMultiEventIdChanged(ReadOnlySpan<NoteEditorModel> notes)
        {
            if (notes.IsSameForAll(n => n.EventId, out var eventId)) {
                _eventIdInput.SetValueWithoutNotify(eventId);
                if (eventId is "")
                    // Avoid display place holder
                    _eventIdInput.SetPlaceHolderText(LocalizableText.Raw(""));
            }
            else {
                _eventIdInput.SetValueWithoutNotify("");
                _eventIdInput.SetPlaceHolderText(LocalizableText.Localized(MultipleValuesPlaceHolderKey));
            }
        }

        //private void NotifyMultiWarningTypeChanged(ReadOnlySpan<NoteModel> notes)
        //{
        //    if (notes.IsSameForAll(n => n.WarningType, out var warningType))
        //        _warningTypeDropdown.SetValueWithoutNotify(warningType.ToIndex());
        //    else
        //        _warningTypeDropdown.SetValueWithoutNotify(-1);
        //}
    }
}
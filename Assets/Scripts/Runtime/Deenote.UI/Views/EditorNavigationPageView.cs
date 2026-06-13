#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Contexts;
using Deenote.Core.Editing;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.Editing.Grids;
using Deenote.Editing.NotePlacement;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using System;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class EditorNavigationPageView : MonoBehaviour
    {
        private ProjectContext _projectContext;
        private EditorContext _editorContext;
        private GamePlayContext _gamePlayContext;
        private GameStageContext _stageContext;

        private ChartNotesEditor _editor;
        private StageNotePlacer2 _placer;

        [SerializeField] TextBox _highlightNoteSpeedInput = default!;
        [SerializeField] ToggleButton _applySpeedDiffToggle = default!;
        [SerializeField] ToggleButton _filterNoteSpeedToggle = default!;

        [SerializeField] NumericStepper _musicSpeedNumericStepper = default!;
        [SerializeField] TextBox _horizontalGridCountInput = default!;
        [SerializeField] TextBox _verticalGridCountInput = default!;
        [SerializeField] Button _horizontalGridCountDecButton = default!;
        [SerializeField] Button _horizontalGridCountIncButton = default!;
        [SerializeField] Button _verticalGridCountDecButton = default!;
        [SerializeField] Button _verticalGridCountIncButton = default!;
        [SerializeField] ToggleButton _horizontalGridSnapToggle = default!;
        [SerializeField] ToggleButton _horizontalGridVisibleToggle = default!;
        [SerializeField] ToggleButton _verticalGridSnapToggle = default!;
        [SerializeField] ToggleButton _verticalGridVisibleToggle = default!;

        //[SerializeField] RadioButtonGroup _curveKindRadioGroup = default!;
        [SerializeField] RadioButton _linearCurveRadio = default!;
        [SerializeField] RadioButton _cubicCurveRadio = default!;
        [SerializeField] Button _generateCurveButton = default!;
        [SerializeField] Button _disableCurveButton = default!;
        [SerializeField] TextBox _fillCurveAmountInput = default!;
        [SerializeField] Button _fillCurveButton = default!;
        [SerializeField] ToggleSwitch _curveAutoApplySizeToggle = default!;
        [SerializeField] ToggleSwitch _curveAutoApplySpeedToggle = default!;
        [SerializeField] Button _curveApplySizeButton = default!;
        [SerializeField] Button _curveApplySpeedButton = default!;

        [SerializeField] TextBox _bpmStartTimeInput = default!;
        [SerializeField] TextBox _bpmEndTimeInput = default!;
        [SerializeField] TextBox _bpmValueInput = default!;
        [SerializeField] Button _bpmFillButton = default!;

        private const int MinCurveFillAmount = 0;
        private const int MaxCurveFillAmount = 256;
        private static readonly int[] _predefinedHorizontalGridCount = { 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64 };

        private CurveKind _curveKind;
        private int _curveFillAmount;
        private bool _curveAutoApplySize;
        private bool _curveAutoApplySpeed;
        private float _bpmStartTime;
        private float _bpmEndTime;
        private float _bpmValue;

        private void Awake()
        {
            _projectContext = MainSystem.Contexts.Project;
            _editorContext = MainSystem.Contexts.Editor;
            _gamePlayContext = MainSystem.Contexts.GamePlay;
            _stageContext = MainSystem.Contexts.GameStage;

            _editor = MainSystem.ChartEditor;
            _placer = MainSystem.StageNotePlacer;
        }

        private void Start()
        {
            RegisterNotifications();
            _linearCurveRadio.SetChecked();
        }

        internal void RegisterNotifications()
        {
            // Stage
            {
                _highlightNoteSpeedInput.EditSubmitted += text =>
                {
                    if (float.TryParse(text, out var value))
                        _editorContext.NotePlacement.PlacementNoteSpeed = value;
                    else
                        _highlightNoteSpeedInput.SetValueWithoutNotify(_editorContext.NotePlacement.PlacementNoteSpeed.ToString("F2"));
                };
                _editorContext.NotePlacement.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.PlacementNoteSpeed))) {
                        _highlightNoteSpeedInput.SetValueWithoutNotify(s.PlacementNoteSpeed.ToString("F2"));
                    }
                });

                _applySpeedDiffToggle.IsCheckedChanged += val => _stageContext.IsApplySpeedDifference = val;
                _filterNoteSpeedToggle.IsCheckedChanged += val => _stageContext.IsFilterNoteSpeed = val;
                _stageContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.IsApplySpeedDifference))) {
                        _applySpeedDiffToggle.SetIsCheckedWithoutNotify(s.IsApplySpeedDifference);
                    }
                    if (e.MatchProperty(nameof(s.IsFilterNoteSpeed))) {
                        _filterNoteSpeedToggle.SetIsCheckedWithoutNotify(s.IsFilterNoteSpeed);
                    }
                });

                _musicSpeedNumericStepper.Initialize(GamePlayContext.MinMusicSpeed, GamePlayContext.MaxMusicSpeed);
                _musicSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
                _musicSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
                _musicSpeedNumericStepper.ValueChanged += val => _gamePlayContext.MusicSpeed = val;
                _gamePlayContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.MusicSpeed))) {
                        _musicSpeedNumericStepper.SetValueWithoutNotify(s.MusicSpeed);
                    }
                });
            }

            // Grids
            {
                void SyncHorizontal(int subdivisionPerBeat)
                {
                    _horizontalGridCountInput.SetValueWithoutNotify(subdivisionPerBeat.ToString());
                    _horizontalGridCountDecButton.gameObject.SetActive(subdivisionPerBeat > _predefinedHorizontalGridCount[0]);
                    _horizontalGridCountIncButton.gameObject.SetActive(subdivisionPerBeat < _predefinedHorizontalGridCount[^1]);
                }

                void SyncVertical(int gridCount)
                {
                    _verticalGridCountInput.SetValueWithoutNotify(gridCount.ToString());
                }

                _horizontalGridCountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival))
                        _editorContext.Grids.TimeGrids.SubdivisionPerBeat = ival;
                    SyncHorizontal(_editorContext.Grids.TimeGrids.SubdivisionPerBeat);
                };
                _editorContext.Grids.TimeGrids.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.SubdivisionPerBeat))) {
                        SyncHorizontal(s.SubdivisionPerBeat);
                    }
                });
                _verticalGridCountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival))
                        _editorContext.Grids.PositionGrids.GridCount = ival;
                    SyncVertical(_editorContext.Grids.PositionGrids.GridCount);
                };
                _editorContext.Grids.PositionGrids.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.GridCount))) {
                        SyncVertical(s.GridCount);
                    }
                });

                _horizontalGridCountDecButton.Clicked += () =>
                {
                    var index = _predefinedHorizontalGridCount.AsSpan().FindLowerBoundIndex(_editorContext.Grids.TimeGrids.SubdivisionPerBeat);
                    if (index == 0) return;
                    _editorContext.Grids.TimeGrids.SubdivisionPerBeat = _predefinedHorizontalGridCount[index - 1];
                };
                _horizontalGridCountIncButton.Clicked += () =>
                {
                    var index = _predefinedHorizontalGridCount.AsSpan().FindUpperBoundIndex(_editorContext.Grids.TimeGrids.SubdivisionPerBeat);
                    if (index >= _predefinedHorizontalGridCount.Length) return;
                    _editorContext.Grids.TimeGrids.SubdivisionPerBeat = _predefinedHorizontalGridCount[index];
                };

                _horizontalGridSnapToggle.IsCheckedChanged += val => _placer.SnapToTimeGrids = val;
                _verticalGridSnapToggle.IsCheckedChanged += val => _placer.SnapToPositionGrids = val;
                _placer.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.SnapToPositionGrids))) {
                        _verticalGridSnapToggle.SetIsCheckedWithoutNotify(s.SnapToPositionGrids);
                    }
                    if (e.MatchProperty(nameof(s.SnapToTimeGrids))) {
                        _horizontalGridSnapToggle.SetIsCheckedWithoutNotify(s.SnapToTimeGrids);
                    }
                });

                _horizontalGridVisibleToggle.IsCheckedChanged += val => _stageContext.IsTimeGridsVisible = val;
                _verticalGridVisibleToggle.IsCheckedChanged += val => _stageContext.IsPositionGridsVisible = val;
                _stageContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.IsTimeGridsVisible))) {
                        _horizontalGridVisibleToggle.SetIsCheckedWithoutNotify(s.IsTimeGridsVisible);
                    }
                    if (e.MatchProperty(nameof(s.IsPositionGridsVisible))) {
                        _verticalGridVisibleToggle.SetIsCheckedWithoutNotify(s.IsPositionGridsVisible);
                    }
                });
            }

            // Curves
            {
                _linearCurveRadio.Checked += () => _curveKind = CurveKind.Linear;
                _cubicCurveRadio.Checked += () => _curveKind = CurveKind.Cubic;
                _generateCurveButton.Clicked += () =>
                {
                    _editorContext.Grids.Curves.InitializeCurve(_editorContext.NoteSelection.SelectedNotes, _curveKind);
                    // Remove notes in between
                    _editor.RemoveNotes(_editorContext.NoteSelection.SelectedNotes[1..^1].ToImmutableArray());
                };
                _disableCurveButton.Clicked += _editorContext.Grids.Curves.DisableCurrentCurve;
                _editorContext.Grids.Curves.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.IsCurveOn))) {
                        _disableCurveButton.IsInteractable = s.IsCurveOn;
                    }
                });

                _fillCurveAmountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival)) {
                        _curveFillAmount = Mathf.Clamp(ival, MinCurveFillAmount, MaxCurveFillAmount);
                        if (_editorContext.Grids.Curves.IsCurveOn) {
                            _fillCurveButton.IsInteractable = _curveFillAmount > 0;
                        }
                    }
                    _fillCurveAmountInput.SetValueWithoutNotify(_curveFillAmount.ToString());
                };
                _fillCurveButton.Clicked += FillCurve;
                _curveAutoApplySizeToggle.IsCheckedChanged += val => _curveAutoApplySize = val;
                _curveAutoApplySpeedToggle.IsCheckedChanged += val => _curveAutoApplySpeed = val;
                _curveApplySizeButton.Clicked += () =>
                {
                    var curve = _editorContext.Grids.Curves.SizeCurve;
                    if (curve == null) return;
                    _editor.EditNotesSize(_editorContext.NoteSelection.SelectedNotes, v => curve.GetValue(v) ?? v);
                };
                _curveApplySpeedButton.Clicked += () =>
                {
                    var curve = _editorContext.Grids.Curves.SpeedCurve;
                    if (curve == null) return;
                    _editor.EditNotesSpeed(_editorContext.NoteSelection.SelectedNotes, v => curve.GetValue(v) ?? v);
                };

                _editorContext.NoteSelection.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.SelectedNotes))) {
                        var generatable = s.SelectedNotes.Length >= 2;
                        _generateCurveButton.IsInteractable = generatable;
                        var appliable = s.SelectedNotes.Length > 2;
                        _curveApplySizeButton.IsInteractable = appliable;
                        _curveApplySpeedButton.IsInteractable = appliable;
                    }
                });

                _editorContext.Grids.Curves.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.IsCurveOn))) {
                        _fillCurveButton.IsInteractable = s.IsCurveOn && _curveFillAmount > 0;
                    }
                });
            }

            // BPM
            {
                void SyncFloatInput(TextBox input, float value) => input.SetValueWithoutNotify(value.ToString("F3"));

                _bpmStartTimeInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmStartTime = fval;
                    SyncFloatInput(_bpmStartTimeInput, _bpmStartTime);
                };
                _bpmEndTimeInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval)) {
                        if (_projectContext.CurrentProject is not null)
                            _bpmEndTime = Mathf.Min(fval, _gamePlayContext.MusicLength);
                        else
                            _bpmEndTime = fval;
                    }
                    SyncFloatInput(_bpmEndTimeInput, _bpmEndTime);
                };
                _bpmValueInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmValue = fval;
                    SyncFloatInput(_bpmValueInput, _bpmValue);
                };
                _bpmFillButton.Clicked += () =>
                {
                    _projectContext.AssertProjectLoaded();
                    var endTime = Mathf.Min(_bpmEndTime, _gamePlayContext.MusicLength);
                    _editor.InsertTempo(new TempoRange(_bpmValue, _bpmStartTime, endTime));
                };

                _editorContext.NoteSelection.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.SelectedNotes))) {
                        var selectedNotes = s.SelectedNotes;
                        if (selectedNotes.IsEmpty)
                            return;

                        float start = selectedNotes[0].Time;
                        float end = selectedNotes[^1].Time;
                        _bpmStartTime = start;
                        _bpmEndTime = end;
                        SyncFloatInput(_bpmStartTimeInput, _bpmStartTime);
                        SyncFloatInput(_bpmEndTimeInput, _bpmEndTime);

                        if (selectedNotes.Length == 1)
                            return;

                        float interval = end - start;
                        if (interval < Tempo.MinBeatLineInterval)
                            return;
                        _bpmValue = 60f / interval;
                        SyncFloatInput(_bpmValueInput, _bpmValue);
                    }
                });

                _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.CurrentChart))) {
                        var chartLoaded = s.CurrentChart is not null;
                        _bpmFillButton.IsInteractable = chartLoaded;
                    }
                });
            }
        }

        private void FillCurve()
        {
            var curves = _editorContext.Grids.Curves;
            if (!curves.IsCurveOn)
                return;

            using var so_notes = SpanOwner<NoteData>.Allocate(_curveFillAmount);
            var notes = so_notes.Span;

            for (int i = 0; i < _curveFillAmount; i++) {
                var time = Mathf.Lerp(curves.StartTime, curves.EndTime, (float)(i + 1) / (_curveFillAmount + 1));
                var pos = curves.PositionCurve.GetValue(time)!.Value;
                var note = _editorContext.NotePlacement.ClonePrototypeData();
                note.PositionCoord = new(pos, time);
                if (_curveAutoApplySize)
                    note.Size = curves.SizeCurve.GetValue(time)!.Value;
                if (_curveAutoApplySpeed)
                    note.Speed = curves.SpeedCurve.GetValue(time)!.Value;
            }

            _editor.AddNotes(notes);
        }
    }
}
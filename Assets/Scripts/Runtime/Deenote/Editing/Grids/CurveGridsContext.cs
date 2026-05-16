#nullable enable

using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Deenote.Editing.Grids
{
    public sealed class CurveGridsContext : INotifyPropertyChanged<CurveGridsContext>
    {
        private const int CubicCurveSegmentCount = 400;

        private readonly List<NoteEditorModel> _interpolationNotes = new();
        private CurveKind? _curveKind;

        [MemberNotNullWhen(true, nameof(PositionCurve), nameof(SizeCurve), nameof(SpeedCurve))]
        public bool IsCurveOn => _curveKind is not null;

        public float StartTime => _interpolationNotes[0].Time;
        public float EndTime => _interpolationNotes[^1].Time;

        private SplineCurve? _positionCurveData;
        public SplineCurve? PositionCurve => _positionCurveData;

        private SplineCurve? _sizeCurveData;
        public SplineCurve? SizeCurve
        {
            get {
                if (!IsCurveOn) return null;
                return _sizeCurveData ??= _curveKind switch {
                    CurveKind.Linear => SplineCurve.Cubic<NoteEditorModel>(_interpolationNotes.AsSpan(), n => n.Time, n => n.Size),
                    CurveKind.Cubic => SplineCurve.Linear<NoteEditorModel>(_interpolationNotes.AsSpan(), n => n.Time, n => n.Size),
                    _ => throw new SwitchExpressionException(_curveKind),
                };
            }
        }

        private SplineCurve? _speedCurveData;
        public SplineCurve? SpeedCurve
        {
            get {
                if (!IsCurveOn) return null;
                return _speedCurveData ??= _curveKind switch {
                    CurveKind.Linear => SplineCurve.Cubic<NoteEditorModel>(_interpolationNotes.AsSpan(), n => n.Time, n => n.Speed),
                    CurveKind.Cubic => SplineCurve.Linear<NoteEditorModel>(_interpolationNotes.AsSpan(), n => n.Time, n => n.Speed),
                    _ => throw new SwitchExpressionException(_curveKind),
                };
            }
        }

        public event Action<CurveGridsContext, PropertyEventArgs>? PropertyChanged;

        public void InitializeCurve(ReadOnlySpan<NoteEditorModel> interpolationNotes, CurveKind kind)
        {
            ModelAsserts.AssertInOrderViaTimeUnique(interpolationNotes);

            _interpolationNotes.Clear();

            if (interpolationNotes.Length < 2) {
                DisableCurrentCurve();
                return;
            }

            _interpolationNotes.AddRange(interpolationNotes);
            _positionCurveData = kind switch {
                CurveKind.Cubic => SplineCurve.Cubic(interpolationNotes, n => n.Time, n => n.Position),
                CurveKind.Linear => SplineCurve.Linear(interpolationNotes, n => n.Time, n => n.Position),
                _ => throw new SwitchExpressionException(kind),
            };
            _curveKind = kind;
            _sizeCurveData = null;
            _speedCurveData = null;
            PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsCurveOn)));
        }

        public void DisableCurrentCurve()
        {
            if (_curveKind is not null) {
                _curveKind = null;
                _positionCurveData = null;
                _sizeCurveData = null;
                _speedCurveData = null;
                PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsCurveOn)));
            }
        }
    }
}

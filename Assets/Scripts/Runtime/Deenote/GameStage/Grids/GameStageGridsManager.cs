#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.CoreB.Models;
using Deenote.Editing;
using Deenote.Editing.Grids;
using Deenote.Editing.NotePlacement;
using Deenote.GamePlay;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.GameStage.Grids
{
    internal sealed class GameStageGridsManager
    {
        private const int CubicCurveSegmentCount = 400;

        private readonly GridsContext _context;
        private readonly GamePlayContext _gamePlay;
        private readonly GameStageContext _stage;
        private PerspectiveLinesRenderer? _linesRenderer;

        public GameStageGridsManager(GridsContext grids, GamePlayContext gamePlay, GameStageContext stage)
        {
            _context = grids;
            _gamePlay = gamePlay;
            _stage = stage;
        }

        internal void SetLinesRenderer(PerspectiveLinesRenderer linesRenderer)
        {
            if (_linesRenderer is not null) {
                _linesRenderer.LineCollecting -= LineRenderer_LineCollecting;
            }
            _linesRenderer = linesRenderer;
            _linesRenderer.LineCollecting += LineRenderer_LineCollecting;
        }

        private void LineRenderer_LineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (_stage.IsTimeGridsVisible) {
                SubmitTimeGridLines(collector);
            }
            if (_stage.IsPositionGridsVisible) {
                SubmitPositionLines(collector);
            }
            SubmitCurveLines(collector);
        }

        private void SubmitTimeGridLines(PerspectiveLinesRenderer.LineCollector collector)
        {
            var startTime = _gamePlay.CurrentTime;
            var theme = _stage.ThemeContext.CurrentTheme;
            if (theme is null)
                return;

            // OPTIMIZE: Use note appear time would be better
            var endTime = startTime + theme.NoteCoordStrategy.GetNoteActiveAheadTime(_stage.ActualNoteFallSpeed, _stage.ActualPlacementNoteSpeed);

            var gridConfig = theme.GridLineConfig;
            float minx = theme.NoteCoordStrategy.PositionToWorldX(NoteConstraints.StageMinPosition);
            float maxx = theme.NoteCoordStrategy.PositionToWorldX(NoteConstraints.StageMaxPosition);
            int counter = 0;
            foreach (var grid in _context.TimeGrids.EnumerateGrids(startTime, endTime)) {
                if (++counter >= 500) {
                    Debug.LogError("infinite loop in GameStageGridsManager.SubmitTimeGridLines");
                    break;
                }
                var (color, width) = grid.Kind switch {
                    TimeGridsContext.GridKind.Subdivision => (_stage.CustomSubdivisionLineColor ?? gridConfig.SubdivisionLineColor, gridConfig.TimeGridSubdivisionLineWidth),
                    TimeGridsContext.GridKind.Beat => (_stage.CustomBeatLineColor ?? gridConfig.BeatLineColor, gridConfig.TimeGridBeatLineWidth),
                    TimeGridsContext.GridKind.Tempo => (_stage.CustomTempoLineColor ?? gridConfig.TempoLineColor, gridConfig.TimeGridTempoLineWidth),
                    _ => throw new SwitchExpressionException(grid.Kind)
                };
                var z = theme.NoteCoordStrategy.TimeToZ(grid.Time - startTime, _stage.ActualNoteFallSpeed, _stage.HighlightedNoteSpeed);
                collector.AddLine(new Vector2(minx, z), new Vector2(maxx, z), color, width);
            }
        }

        private void SubmitPositionLines(PerspectiveLinesRenderer.LineCollector collector)
        {
            var theme = _stage.ThemeContext.CurrentTheme;
            if (theme is null)
                return;

            var gridConfig = theme.GridLineConfig;
            var minz = theme.NoteCoordStrategy.TimeToZ(0f, _stage.ActualNoteFallSpeed, _stage.HighlightedNoteSpeed);
            var maxz = theme.NoteCoordStrategy.TimeToZ(theme.Config.GetMaxWorldZ(), _stage.ActualNoteFallSpeed, _stage.HighlightedNoteSpeed);
            for (int i = 0; i < _context.PositionGrids.GridCount; i++) {
                var pos = _context.PositionGrids.GetGridPosition(i);
                var x = theme.NoteCoordStrategy.PositionToWorldX(pos);
                var color = _stage.CustomSubdivisionLineColor ?? gridConfig.PositionGridLineColor;
                var width = i == 0 || i == _context.PositionGrids.GridCount - 1
                    ? gridConfig.PositionGridBorderWidth
                    : gridConfig.PositionGridLineWidth;
                collector.AddLine(new Vector2(x, minz), new Vector2(x, maxz), color, width);
            }
        }

        private void SubmitCurveLines(PerspectiveLinesRenderer.LineCollector collector)
        {
            var theme = _stage.ThemeContext.CurrentTheme;
            if (theme is null)
                return;

            if (_context.Curves.PositionCurve is null) {
                return;
            }

            var curve = _context.Curves.PositionCurve;
            var currentTime = _gamePlay.CurrentTime;
            var stageMaxTime = currentTime + theme.NoteCoordStrategy.GetNoteActiveAheadTime(_stage.ActualNoteFallSpeed, _stage.HighlightedNoteSpeed);
            if (curve.MinX >= stageMaxTime || curve.MaxX <= currentTime) {
                return;
            }
            float min = Mathf.Max(curve.MinX, currentTime);
            float max = Mathf.Min(curve.MaxX, stageMaxTime);
            var coords = (stackalloc NoteCoord[CubicCurveSegmentCount]);
            for (int i = 0; i < coords.Length; i++) {
                float time = Mathf.Lerp(min, max, (float)i / coords.Length);
                float pos = curve.GetValue(time)!.Value;
                coords[i] = NoteCoord.ClampPosition(pos, time);
            }

            var config = theme.GridLineConfig;
            var strip = (stackalloc Vector2[coords.Length]);
            for (int i = 0; i < strip.Length; i++) {
                var coord = coords[i];
                strip[i] = new Vector2(
                    theme.NoteCoordStrategy.PositionToWorldX(coord.Position),
                    theme.NoteCoordStrategy.TimeToZ(coord.Time - currentTime, _stage.ActualNoteFallSpeed, _stage.HighlightedNoteSpeed));
            }
            collector.AddLineStrip(strip, config.CurveLineColor, config.CurveLineWidth);
        }
    }
}
#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.CoreB.Models;

namespace Deenote.Editing.Grids
{
    public sealed class GridsContext
    {
        public TimeGridsContext TimeGrids { get; }
        public PositionGridsContext PositionGrids { get; }
        public CurveGridsContext Curves { get; }

        public GridsContext(ProjectContext project, SaveSystem storage)
        {
            TimeGrids = new TimeGridsContext(project, storage);
            PositionGrids = new PositionGridsContext(storage);
            Curves = new CurveGridsContext();
        }

        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedTime = snapTime ? TimeGrids.GetNearestGrid(coord.Time).Value ?? coord.Time : coord.Time;
            float snappedPosition = snapPosition ? Curves.PositionCurve?.GetValue(snappedTime) ?? PositionGrids.GetNearestGrid(coord.Position) : coord.Position;
            return new NoteCoord(snappedPosition, snappedTime);
        }
    }
}

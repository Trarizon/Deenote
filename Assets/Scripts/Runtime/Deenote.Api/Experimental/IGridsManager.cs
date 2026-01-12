#nullable enable

namespace Deenote.Api.Experimental
{
    public interface IGridsManager
    {
        NoteCoord Quantize(NoteCoord coord, GridSnapOptions snapOptions);
    }

    public enum GridSnapOptions
    {
        Position = 1,
        TimeGrid = 1 << 1,
        Both = 1 << 2,
    }
}

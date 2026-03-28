#nullable enable

namespace Deenote.CoreB.Models.Notes
{
    public interface INoteTime
    {
        float Time { get; }
    }

    public interface INoteSpeed : INoteTime
    {
        float Speed { get; }
    }

    public interface INoteLocation : INoteSpeed
    {
        float Position { get; }

        NoteCoord PositionCoord => new(Position, Time);
    }
}

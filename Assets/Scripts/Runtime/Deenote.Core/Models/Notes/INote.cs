#nullable enable

namespace Deenote.CoreB.Models.Notes
{
    public interface INoteTime
    {
        float Time { get; }
    }

    public interface INoteLocation : INoteTime
    {
        float Position { get; }
        float Speed { get; }
    }
}

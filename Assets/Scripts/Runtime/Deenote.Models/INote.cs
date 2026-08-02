namespace Deenote.Models
{
    public interface INoteTime
    {
        float Time { get; }
    }

    public interface IReadOnlyNoteLink
    {
        IReadOnlyNoteLink? NextLink { get; }
        IReadOnlyNoteLink? PrevLink { get; }
    }

    public interface INoteLink<T> : IReadOnlyNoteLink
        where T : INoteLink<T>
    {
        new T? NextLink { get; set; }
        new T? PrevLink { get; set; }

        IReadOnlyNoteLink? IReadOnlyNoteLink.NextLink => NextLink!;
        IReadOnlyNoteLink? IReadOnlyNoteLink.PrevLink => PrevLink!;
    }
}
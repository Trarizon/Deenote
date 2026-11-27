namespace Deenote.EditorModels
{
    public interface INoteLink
    {
        INoteLink? PrevLink { get; }
        INoteLink? NextLink { get; }
    }

    public interface INoteLink<T> : INoteLink where T : INoteLink
    {
        new T? PrevLink { get; }
        new T? NextLink { get; }

        INoteLink? INoteLink.NextLink => NextLink;
        INoteLink? INoteLink.PrevLink => PrevLink;
    }
}
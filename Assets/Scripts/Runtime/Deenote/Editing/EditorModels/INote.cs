#nullable enable

using Deenote.CoreB.Models.Notes;

namespace Deenote.Editing.EditorModels
{
    internal interface INoteUnique
    {
        private static uint _uid = 0;

        uint Uid { get; }

        protected static uint GetUid()
        {
            unchecked {
                return ++_uid;
            }
        }
    }

    internal interface INoteTimeUnique : INoteUnique, INoteTime { }

    internal interface INoteReadOnlyLink : INoteTimeUnique
    {
        INoteReadOnlyLink? NextLink { get; }
        INoteReadOnlyLink? PrevLink { get; }
    }

    internal interface INoteLink : INoteLocation, INoteLink<INoteLink>
    {
        new INoteLink? NextLink { get; set; }
        new INoteLink? PrevLink { get; set; }

        INoteLink? INoteLink<INoteLink>.NextLink
        {
            get=> NextLink;
            set => NextLink = value!;
        }

        INoteLink? INoteLink<INoteLink>.PrevLink
        {
            get => PrevLink;
            set => PrevLink = value!;
        }
    }

    internal interface INoteLink<TSelf> : INoteReadOnlyLink where TSelf : INoteLink<TSelf>
    {
        new TSelf? NextLink { get; set; }
        new TSelf? PrevLink { get; set; }

        INoteReadOnlyLink? INoteReadOnlyLink.NextLink => NextLink!;
        INoteReadOnlyLink? INoteReadOnlyLink.PrevLink => PrevLink!;
    }

    internal interface ICollidableNote : INoteLocation
    {
        int CollisionCount { get; set; }
    }

    internal interface IGameNote : INoteTime, INoteSpeed, INoteLocation
    {
        float Size { get; }
        NoteKind Kind { get; }
        float Duration { get; }
    }
}

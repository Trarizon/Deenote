#nullable enable

namespace Deenote.Editing.EditorModels
{
    internal interface INoteLinkNode
    {
        INoteLinkNode? NextLink { get; set; }
        INoteLinkNode? PrevLink { get; set; }
    }
}

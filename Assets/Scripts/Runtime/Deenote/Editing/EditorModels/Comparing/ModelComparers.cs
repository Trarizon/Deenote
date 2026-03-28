#nullable enable

namespace Deenote.Editing.EditorModels.Comparing
{
    internal static class ModelComparers
    {
        public static NoteTimeUniqueComaparer ViaTimeUnique { get; } = new();
        public static NoteUniqueComparer ViaUnique { get; } = new();
    }
}

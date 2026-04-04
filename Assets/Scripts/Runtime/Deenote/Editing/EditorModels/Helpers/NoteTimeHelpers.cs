#nullable enable

namespace Deenote.Editing.EditorModels.Helpers
{
    internal static class NoteTimeHelpers
    {
        public static float GetPseudoTime(float time, float speed, float currentTime)
        {
            return currentTime + (time - currentTime) * speed;
        }
    }
}

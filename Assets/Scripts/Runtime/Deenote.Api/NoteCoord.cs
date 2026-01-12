#nullable enable

namespace Deenote.Api
{
    public struct NoteCoord
    {
        public float Position { get; set; }
        public float Time { get; set; }

        public NoteCoord(float position, float time)
        {
            Position = position;
            Time = time;
        }
    }
}

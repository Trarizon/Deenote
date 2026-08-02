using Newtonsoft.Json;
using System.Collections.Generic;

namespace Deenote.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public readonly struct NoteLinkIterator
    {
        private readonly NoteData _head;

        [JsonProperty("notes")]
        public IEnumerable<NoteData> Notes
        {
            get {
                NoteData? current = _head;
                while (current is not null) {
                    yield return current;
                    current = current.NextLink;
                }
            }
        }

        public NoteLinkIterator(NoteData linkHead) => _head = linkHead;

        internal readonly struct Deserializer
        {
            public readonly IEnumerable<NoteData> Notes;

            [JsonConstructor]
            public Deserializer(IEnumerable<NoteData> notes) => Notes = notes;
        }
    }
}
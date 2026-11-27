using CommunityToolkit.Diagnostics;
using Deenote.Core;
using Deenote.Entities.Models;
using System;
using UnityEngine.Pool;

namespace Deenote.EditorModels
{
    public static class NoteLinkHelpers
    {
        public static void CloneLinkInfos<TSource>(ReadOnlySpan<TSource> source, ReadOnlySpan<NotePrototypeModel> target)
            where TSource : class, INoteLink
        {
            Guard.HasSizeEqualTo(source, target.Length);

            foreach (var note in target) {
                NotePrototypeModel.Marshal.UnlinkNext(note);
                NotePrototypeModel.Marshal.UnlinkPrev(note);
            }

            // <next, prev>
            using var dp = DictionaryPool<INoteLink, NotePrototypeModel>.Get(out var slideLookup);

            for (int i = 0; i < source.Length; i++) {
                var link = source[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, target[i]);
            }

            for (int i = 0; i < source.Length; i++) {
                var from = source[i];
                var to = target[i];

                if (slideLookup.ContainsKey(from)) {
                    var prevLink = from.PrevLink;
                    NotePrototypeModel? copiedPrev = default;

                    // Find the nearest previous link in 'source'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Asserts.NotNull(copiedPrev);
                        NotePrototypeModel.Marshal.Link(copiedPrev, to);
                    }
                }
            }

        }

        public static void CloneLinkInfos(ReadOnlySpan<NotePrototypeModel> source, ReadOnlySpan<NoteModel> target)
        {
            Guard.HasSizeEqualTo(source, target.Length);

            foreach (var note in target) {
                UnlinkNext(note);
                UnlinkPrev(note);
            }

            // <next, prev>
            using var dp = DictionaryPool<NotePrototypeModel, NoteModel>.Get(out var slideLookup);

            for (int i = 0; i < source.Length; i++) {
                var link = source[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, target[i]);
            }

            for (int i = 0; i < source.Length; i++) {
                var from = source[i];
                var to = target[i];

                if (slideLookup.ContainsKey(from)) {
                    var prevLink = from.PrevLink;
                    NoteModel? copiedPrev = default;

                    // Find the nearest previous link in 'source'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Asserts.NotNull(copiedPrev);
                        Link(copiedPrev, to);
                    }
                }
            }
        }

        public static void CloneLinkInfos(ReadOnlySpan<NoteModel> source, ReadOnlySpan<NotePrototypeModel> target)
        {
            Guard.HasSizeEqualTo(source, target.Length);

            foreach (var note in target) {
                NotePrototypeModel.Marshal.UnlinkNext(note);
                NotePrototypeModel.Marshal.UnlinkPrev(note);
            }

            // <next, prev>
            using var dp = DictionaryPool<NoteModel, NotePrototypeModel>.Get(out var slideLookup);

            for (int i = 0; i < source.Length; i++) {
                var link = source[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, target[i]);
            }

            for (int i = 0; i < source.Length; i++) {
                var from = source[i];
                var to = target[i];

                if (slideLookup.ContainsKey(from)) {
                    var prevLink = from.PrevLink;
                    NotePrototypeModel? copiedPrev = default;

                    // Find the nearest previous link in 'source'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Asserts.NotNull(copiedPrev);
                        NotePrototypeModel.Marshal.Link(copiedPrev, to);
                    }
                }
            }
        }

        // Marshal, these should move to NoteData later

        private static void Link(NoteModel prev, NoteModel next)
        {
            prev._nextLink = next;
            next._prevLink = prev;
        }

        private static void UnlinkNext(NoteModel note)
        {
            if (note._nextLink is not null)
                note._nextLink._prevLink = null;
            note._nextLink = null;
        }

        private static void UnlinkPrev(NoteModel note)
        {
            if (note._prevLink is not null)
                note._prevLink._nextLink = null;
            note._prevLink = null;
        }
    }
}
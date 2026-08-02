using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using UnityEngine.Pool;

namespace Deenote.Models.Helpers
{
    public static class NoteLinkHelpers
    {
        public static void CloneLinkInfos<TSource, TTarget>(Span<TSource> source, Span<TTarget> target)
            where TSource : IReadOnlyNoteLink
            where TTarget : INoteLink<TTarget>
            => CloneLinkInfos((ReadOnlySpan<TSource>)source, (ReadOnlySpan<TTarget>)target);

        public static void CloneLinkInfos<TSource, TTarget>(ReadOnlySpan<TSource> source, ReadOnlySpan<TTarget> target)
            where TSource : IReadOnlyNoteLink
            where TTarget : INoteLink<TTarget>
        {
            Guard.HasSizeEqualTo(target, source.Length);

            // Clear previous links
            foreach (var note in target) {
                if (note.NextLink is not null)
                    note.NextLink.PrevLink = default;
                if (note.PrevLink is not null)
                    note.PrevLink.NextLink = default;
                note.PrevLink = default;
                note.NextLink = default;
            }

            // <next, prev>
            using var dp_slideLookup = DictionaryPool<IReadOnlyNoteLink, TTarget>.Get(out var slideLookup);

            for (int i = 0; i < source.Length; i++) {
                var link = source[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, target[i]);
            }

            for (int i = 0; i < source.Length; i++) {
                var from = source[i];
                var to = target[i];

                if (from.PrevLink is not null || from.NextLink is not null) {
                    var prevLink = from.PrevLink;
                    TTarget copiedPrev = default!;

                    // Find the nearest previous link in 'links'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Debug.Assert(copiedPrev is not null);
                        copiedPrev!.NextLink = to;
                        to.PrevLink = copiedPrev;
                    }
                }
            }
        }
    }
}

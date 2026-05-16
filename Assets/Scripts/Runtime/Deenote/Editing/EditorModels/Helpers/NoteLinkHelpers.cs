#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Comparing;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Editing.EditorModels.Helpers
{
    internal static class NoteLinkHelpers
    {
        public static void ReorderLink(INoteLink node)
        {
            var curr = node;
            var prev = curr.PrevLink;
            while (prev is not null && ModelComparers.ViaTimeUnique.Less(prev, node)) {
                curr = prev;
                prev = curr.PrevLink;
            }

            // Node should move backward
            if (curr != node) {
                Debug.Assert(node.PrevLink is not null);
                node.PrevLink!.NextLink = node.NextLink;
                if (node.NextLink is not null) {
                    node.NextLink.PrevLink = node.PrevLink;
                }

                node.NextLink = curr;
                node.PrevLink = prev;
                curr.PrevLink = node;
                if (prev is not null) {
                    prev.NextLink = node;
                }
                return;
            }

            var next = curr.NextLink;
            while (next is not null && ModelComparers.ViaTimeUnique.Less(node, next)) {
                curr = next;
                next = curr.NextLink;
            }

            // Node should move forward
            if (curr != node) {
                Debug.Assert(node.NextLink is not null);
                node.NextLink!.PrevLink = node.PrevLink;
                if (node.PrevLink is not null) {
                    node.PrevLink.NextLink = node.NextLink;
                }
                node.PrevLink = curr;
                curr.NextLink = node;
                node.NextLink = next;
                if (next is not null) {
                    next.PrevLink = node;
                }
            }

            AssertLinkOrder(node);
        }

        public static void UnlinkRemainingChain<T>(INoteLink<T> node) where T : INoteLink<T>
        {
            var prev = node.PrevLink;
            var next = node.NextLink;
            if (prev is not null) {
                prev.NextLink = next;
            }
            if (next is not null) {
                next.PrevLink = prev;
            }
            node.NextLink = default;
            node.PrevLink = default;
        }

        public static void LinkReplace<T>(T prev, T next)
            where T : INoteLink<T>
        {
            if (prev.NextLink is not null) {
                prev.NextLink.PrevLink = default;
            }
            if (next.PrevLink is not null) {
                next.PrevLink.NextLink = default;
            }
            prev.NextLink = next;
            next.PrevLink = prev;
        }

        public static void LinkExchange<T>(T prev, T next)
            where T : INoteLink<T>
        {
            if (prev.NextLink is not null) {
                prev.NextLink.PrevLink = next.PrevLink;
            }
            if (next.PrevLink is not null) {
                next.PrevLink.NextLink = prev.NextLink;
            }
            prev.NextLink = next;
            next.PrevLink = prev;
        }

        public static void InsertAfter<T>(T node, T prev)
            where T : INoteLink<T>
        {
            if (prev.NextLink is not null) {
                prev.NextLink.PrevLink = node;
            }
            node.NextLink = prev.NextLink;
            node.PrevLink = prev;
            prev.NextLink = node;
        }

        public static void InsertBefore<T>(T node, T next)
            where T : INoteLink<T>
        {
            if (next.PrevLink is not null) {
                next.PrevLink.NextLink = node;
            }
            node.NextLink = next;
            node.PrevLink = next.PrevLink;
            next.PrevLink = node;
        }

        public static void CloneLinkInfos<TTarget>(ReadOnlySpan<NoteData> source, ReadOnlySpan<TTarget> target)
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
            using var dp_slideLookup = DictionaryPool<NoteData, TTarget>.Get(out var slideLookup);

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

        public static void CloneLinkInfos<TLink>(ReadOnlySpan<TLink> links, ReadOnlySpan<NoteData> datas)
            where TLink : INoteReadOnlyLink
        {
            Guard.HasSizeEqualTo(datas, links.Length);
            foreach (var note in datas) {
                NoteData.Marshal.UnlinkNext(note);
                NoteData.Marshal.UnlinkPrev(note);
            }

            // <next, prev>
            using var dp_slideLookup = DictionaryPool<INoteReadOnlyLink, NoteData>.Get(out var slideLookup);

            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, datas[i]);
            }

            for (int i = 0; i < links.Length; i++) {
                var from = links[i];
                var to = datas[i];

                if (slideLookup.ContainsKey(from)) {
                    var prevLink = from.PrevLink;
                    NoteData copiedPrev = default!;

                    // Find the nearest previous link in 'links'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Debug.Assert(copiedPrev is not null);
                        NoteData.Marshal.Link(copiedPrev!, to);
                    }
                }
            }
        }

        public static void CloneLinkInfos(ReadOnlySpan<NoteData> links, ReadOnlySpan<NoteData> datas)
        {
            Guard.HasSizeEqualTo(datas, links.Length);
            foreach (var note in datas) {
                NoteData.Marshal.UnlinkNext(note);
                NoteData.Marshal.UnlinkPrev(note);
            }

            // <next, prev>
            using var dp_slideLookup = DictionaryPool<NoteData, NoteData>.Get(out var slideLookup);

            for (int i = 0; i < links.Length; i++) {
                var link = links[i];
                if (link.PrevLink is not null || link.NextLink is not null)
                    slideLookup.Add(link, datas[i]);
            }

            for (int i = 0; i < links.Length; i++) {
                var from = links[i];
                var to = datas[i];

                if (slideLookup.ContainsKey(from)) {
                    var prevLink = from.PrevLink;
                    NoteData copiedPrev = default!;

                    // Find the nearest previous link in 'links'
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Debug.Assert(copiedPrev is not null);
                        NoteData.Marshal.Link(copiedPrev!, to);
                    }
                }
            }
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private static void AssertLinkOrder(INoteLink node)
        {
            var prev = node.PrevLink;
            while (prev is not null) {
                if (ModelComparers.ViaTimeUnique.Greater(prev, node)) {
                    Debug.LogAssertion($"Broken link order: {prev.Time}:{prev.Uid} > cur:{node.Time}:{node.Uid}");
                    return;
                }
            }

            var next = node.NextLink;
            while (next is not null) {
                if (ModelComparers.ViaTimeUnique.Less(node, next)) {
                    Debug.LogAssertion($"Broken link order: cur:{node.Time}:{node.Uid} > {next.Time}:{next.Uid}");
                    return;
                }
            }
        }
    }
}

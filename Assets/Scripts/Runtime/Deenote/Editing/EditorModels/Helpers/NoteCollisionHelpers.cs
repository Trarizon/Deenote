#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels.Helpers
{
    internal static class NoteCollisionHelpers
    {
        private const float NoteTimeCollisionThreshold = 0.001f;
        private const float NotePositionCollisionThreshold = 0.01f;

        private static bool IsTimeCollided(float left, float right)
            => Math.Abs(left - right) < NoteTimeCollisionThreshold;

        private static bool IsPositionCollided(float left, float right)
            => Math.Abs(left - right) < NotePositionCollisionThreshold;

        private static CollisionResult Collide<T>(ReadOnlySpan<T> notes, NoteCoord coord, ICollidableNote? except = null)
            where T : class, ICollidableNote
        {
            var index = notes.BinarySearch(new NoteTimeComparable(coord.Time));
            NumberUtils.FlipNegative(ref index);

            List<ICollidableNote> collidedNotes = new();

            for (int i = index; i < notes.Length; i++) {
                var cmp = notes[i];
                if (except == cmp)
                    continue;
                if (!IsTimeCollided(coord.Time, cmp.Time))
                    break;
                if (IsPositionCollided(coord.Position, cmp.Position))
                    collidedNotes.Add(cmp);
            }

            for (int i = index - 1; i >= 0; i--) {
                var cmp = notes[i];
                if (except == cmp)
                    continue;
                if (!IsTimeCollided(coord.Time, cmp.Time))
                    break;
                if (IsPositionCollided(coord.Position, cmp.Position))
                    collidedNotes.Add(cmp);
            }

            return new CollisionResult(coord, collidedNotes);
        }

        public static void UpdateCollisionsPreAdding(ChartEditorModel chart, ICollidableNote note)
        {
            var collisions = Collide<NoteEditorModel>(chart.Notes.AsSpan(), new NoteCoord(note.Time, note.Position), note);
            foreach (var collision in collisions.CollidedNotes) {
                collision.CollisionCount++;
            }
            note.CollisionCount += collisions.CollidedNotes.Length;
        }

        public static void UpdateCollisionsPostRemoving(ChartEditorModel chart, ICollidableNote note)
        {
            var collisions = Collide<NoteEditorModel>(chart.Notes.AsSpan(), new NoteCoord(note.Time, note.Position), note);
            foreach (var collision in collisions.CollidedNotes) {
                collision.CollisionCount--;
            }
            note.CollisionCount -= collisions.CollidedNotes.Length;
        }

        public static void ReupdateCollisionPreMoving(ChartEditorModel chart, ICollidableNote note)
        {
            var collisions = Collide<NoteEditorModel>(chart.Notes.AsSpan(), new NoteCoord(note.Time, note.Position), note);
            foreach (var collision in collisions.CollidedNotes) {
                collision.CollisionCount--;
            }
            note.CollisionCount -= collisions.CollidedNotes.Length;
        }

        public static void ReupdateCollisionPostMoving(ChartEditorModel chart, ICollidableNote note)
        {
            var collisions = Collide<NoteEditorModel>(chart.Notes.AsSpan(), new NoteCoord(note.Time, note.Position), note);
            foreach (var collision in collisions.CollidedNotes) {
                collision.CollisionCount++;
            }
            note.CollisionCount += collisions.CollidedNotes.Length;
        }

        public static void InitializeCollision(ChartEditorModel chart)
        {
            for (int i = 0; i < chart.Notes.Count; i++) {
                var note = chart.Notes[i];
                for (int j = i + 1; j < chart.Notes.Count; j++) {
                    var cmp = chart.Notes[j];
                    if (!IsTimeCollided(note.Time, cmp.Time))
                        break;
                    if (IsPositionCollided(note.Position, cmp.Position)) {
                        note.CollisionCount++;
                        cmp.CollisionCount++;
                    }
                }
            }
        }

        public readonly struct CollisionResult
        {
            private readonly List<ICollidableNote> _notes;
            public NoteCoord Coord { get; }
            public ReadOnlySpan<ICollidableNote> CollidedNotes => _notes.AsSpan();

            public CollisionResult(NoteCoord coord, List<ICollidableNote> notes)
            {
                Coord = coord;
                _notes = notes;
            }
        }
    }
}

using Deenote.CoreB.Helpers;
using Deenote.Models;
using Deenote.Models.Comparisons;
using System;
using System.Collections.Generic;

namespace Deenote.Contexts.EditorModels.Helpers
{
    internal static class NoteCollisionHelpers
    {
        private const float NoteTimeCollisionThreshold = 0.001f;
        private const float NotePositionCollisionThreshold = 0.01f;

        private static bool IsTimeCollided(float left, float right)
            => Math.Abs(left - right) < NoteTimeCollisionThreshold;

        private static bool IsPositionCollided(float left, float right)
            => Math.Abs(left - right) < NotePositionCollisionThreshold;

        private static CollisionResult Collide<T>(ReadOnlySpan<T> notes, NoteCoord coord, INoteCollision? except = null)
            where T : class, INoteCollision
        {
            var index = notes.BinarySearch(new NoteTimeComparable(coord.Time));
            if (index < 0) index = ~index;

            List<INoteCollision> collidedNotes = new();

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

        public static void UpdateCollisionsPreAdding(ChartEditorModel chart, INoteCollision note)
        {
            var collisions = Collide<NoteEditorModel>(chart.Notes.AsSpan(), new NoteCoord(note.Time, note.Position), note);
            foreach (var collision in collisions.CollidedNotes) {
                collision.CollisionCount++;
            }
            note.CollisionCount += collisions.CollidedNotes.Length;
        }

        public readonly struct CollisionResult
        {
            private readonly List<INoteCollision> _notes;
            public NoteCoord Coord { get; }
            public ReadOnlySpan<INoteCollision> CollidedNotes => _notes.AsSpan();

            public CollisionResult(NoteCoord coord, List<INoteCollision> notes)
            {
                Coord = coord;
                _notes = notes;
            }
        }
    }
}

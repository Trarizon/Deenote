#nullable enable

using Deenote.CoreB.Models;
using UnityEngine;

namespace Deenote.GameStage
{
    public interface IGameStageNoteCoordStrategy
    {
        float GetNoteActiveAheadTime(float fallSpeed, float noteSpeed);
        float PositionToWorldX(float position);
        float WorldXToPosition(float x);
        float TimeToZ(float time, float fallSpeed, float noteSpeed);
        float ZToTime(float z, float fallSpeed, float noteSpeed);
    }

    public static class GameStageNoteCoordStrategyExtensions
    {
        public static (float X, float Z) CoordToXZ(this IGameStageNoteCoordStrategy strategy, NoteCoord coord, float fallSpeed, float noteSpeed)
            => (strategy.PositionToWorldX(coord.Position), strategy.TimeToZ(coord.Time, fallSpeed, noteSpeed));
    }

    internal class DefaultGameStageNoteCoordStrategy : IGameStageNoteCoordStrategy
    {
        private readonly GameStageConfig _config;

        internal DefaultGameStageNoteCoordStrategy(GameStageConfig config)
        {
            _config = config;
        }

        public float GetNoteActiveAheadTime(float fallSpeed, float noteSpeed)
            => _config.NoteAppearAheadTimeFactor / GetActualFallSpeed(fallSpeed) / noteSpeed;

        public float PositionToWorldX(float position)
            => position * _config.NotePosToWorldXFactor;

        public float WorldXToPosition(float x)
            => x / _config.NotePosToWorldXFactor;

        public float TimeToZ(float time, float fallSpeed, float noteSpeed)
            => time * GetActualFallSpeed(fallSpeed) * noteSpeed * _config.NoteTimeToWorldZFactor;

        public float ZToTime(float z, float fallSpeed, float noteSpeed)
            => z / GetActualFallSpeed(fallSpeed) / noteSpeed / _config.NoteTimeToWorldZFactor;

        protected virtual float GetActualFallSpeed(float displayFallSpeed)
            => 3 * Mathf.Pow(1.4f, displayFallSpeed);
    }
}
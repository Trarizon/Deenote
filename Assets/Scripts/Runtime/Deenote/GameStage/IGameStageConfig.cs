using Deenote.CoreB.Models.Notes;
using System;

namespace Deenote.GameStage
{
    internal interface IGameStageConfig
    {
        float NotePosToWorldXFactor { get; }
        float NoteTimeToWorldZFactor { get; }
        float NoteHitEffectMaxDuration { get; }
        float NoteActiveAheadTimeFactor { get; }

        float GetNoteActiveAheadTime(float noteFallSpeed);
        float GetNoteFallSpeedInternal(float noteFallSpeed)
            => 3 * MathF.Pow(1.4f, noteFallSpeed);
    }

    internal sealed class DeemoGameStageConfig : IGameStageConfig
    {
        private GameStageConfig _config;

        public DeemoGameStageConfig(GameStageConfig config) => _config = config;

        public float NotePosToWorldXFactor => _config.NotePosToWorldXFactor;

        public float NoteTimeToWorldZFactor => _config.NoteTimeToWorldZFactor;

        public float NoteHitEffectMaxDuration => _config.NoteHitEffectMaxDuration;

        public float NoteActiveAheadTimeFactor => _config.NoteAppearAheadTimeFactor;

        public float GetNoteActiveAheadTime(float noteFallSpeed)
        {
            var fallSpeed = GetNoteFallSpeedInternal(noteFallSpeed);
            return NoteActiveAheadTimeFactor / fallSpeed;
        }

        public float GetNoteFallSpeedInternal(float noteFallSpeed)
            => 3 * MathF.Pow(1.4f, noteFallSpeed);
    }

    internal static class GameStageConfigExtensions
    {
        public static float EvaluateNoteActiveAheadTime(this IGameStageConfig config, float noteFallSpeed, float noteSpeed)
            => config.GetNoteActiveAheadTime(noteFallSpeed) / noteSpeed;

        public static float EvaluateNoteActiveTime(this IGameStageConfig config, float noteFallSpeed, float time, float speed)
            => time - config.EvaluateNoteActiveAheadTime(noteFallSpeed, speed);

        public static float EvaluateNoteWorldZ(this IGameStageConfig config, float noteFallSpeed, float localTime, float speed)
            => localTime * speed * config.GetNoteFallSpeedInternal(noteFallSpeed) * config.NoteTimeToWorldZFactor;

        public static float GetMaxWorldZ(this IGameStageConfig config)
            => config.NoteActiveAheadTimeFactor * config.NoteTimeToWorldZFactor;
    }
}

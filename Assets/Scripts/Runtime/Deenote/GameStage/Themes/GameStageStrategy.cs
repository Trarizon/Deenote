using Deenote.GameStage.World;
using Deenote.Replica;

namespace Deenote.GameStage.Themes
{
    public interface IGameStageStrategy
    {
        float GetNoteFallSpeedInternal(float noteFallSpeed);
    }

    internal class DeemoGameStageStrategy : IGameStageStrategy
    {
        public float GetNoteFallSpeedInternal(float noteFallSpeed)
            => DeemoReplica.DisplayFallSpeedToPlaneFallSpeed(noteFallSpeed);
    }
}

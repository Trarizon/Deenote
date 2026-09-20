using Deenote.Entities.Models;

namespace Deenote.GameStage.Models
{
    internal readonly struct GameStageNodeView
    {
        private readonly IStageNoteNode _node;
        private readonly GameStageManager _context;

        public GameStageNodeView(IStageNoteNode node, GameStageManager context)
        {
            _node = node;
            _context = context;
        }

        public float Speed => _context.IsApplySpeedDifference ? _node.Speed : 1f;

        public float ActiveAheadTime => _context.StandardNoteActiveAheadTime / Speed;

        public float ActiveTime => _node.Time - ActiveAheadTime;

        public float GetPseudoTime(float currentTime)
            => currentTime + (_node.Time - currentTime) * Speed;
    }
}

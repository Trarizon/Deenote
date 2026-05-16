#nullable enable

using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Helpers;
using System.Runtime.Remoting.Messaging;

namespace Deenote.GameStage.Models
{
    internal readonly struct GameStageNodeView
    {
        private readonly IGameStageNoteNode _node;
        private readonly GameStageContext _context;

        public IGameStageNoteNode NoteNode => _node;

        public GameStageNodeView(IGameStageNoteNode node, GameStageContext context)
        {
            _node = node;
            _context = context;
        }

        public float Speed => _context.IsApplySpeedDifference ? _node.Speed : 1;

        public float GetNodeActiveTime()
        {
            return _node.Time - GetNodeActiveAheadTime();
        }

        public float GetNodeActiveAheadTime()
        {
            var aheadTime = _context.ThemeContext.CurrentTheme.Config.GetNoteActiveAheadTime(_context.ActualNoteFallSpeed);
            return aheadTime / Speed;
        }

        public float GetPseudoTime(float currentTime)
            => NoteTimeHelpers.GetPseudoTime(_node.Time, Speed, currentTime);
    }
}

#nullable enable

using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Comparing;
using System;
using System.Collections.Generic;

namespace Deenote.GameStage.Models
{
    internal sealed class GameStageNodeActiveTimeComparer : IComparer<IGameStageNoteNode>
    {
        private readonly GameStageContext _context;
        public GameStageNodeActiveTimeComparer(GameStageContext context) => _context = context;
        public int Compare(IGameStageNoteNode x, IGameStageNoteNode y)
        {
            var l = new GameStageNodeView(x,_context);
            var r = new GameStageNodeView(y,_context);
            var cmp= Comparer<float>.Default.Compare(l.GetNodeActiveTime(), r.GetNodeActiveTime());
            if (cmp != 0)
                return cmp;
            cmp = ModelComparers.ViaTimeUnique.Compare(x, y);
            return cmp;
        }
    }

    internal readonly struct GameStageNodeActiveTimeComparable : IComparable<IGameStageNoteNode>
    {
        private readonly GameStageContext _context;
        private readonly float _time;
        public GameStageNodeActiveTimeComparable(float time, GameStageContext context)
        {
            _context = context;
            _time = time;
        }

        public int CompareTo(IGameStageNoteNode other)
            => Comparer<float>.Default.Compare(_time, new GameStageNodeView(other, _context).GetNodeActiveTime());
    }
}

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;

namespace Deenote.GameStage.Models
{
    internal sealed class GameStageNodeActiveTimeComparer : IComparer<IStageNoteNode>
    {
        private readonly GameStageManager _context;

        public GameStageNodeActiveTimeComparer(GameStageManager context)
        {
            _context = context;
        }

        public int Compare(IStageNoteNode x, IStageNoteNode y)
        {
            var xv = new GameStageNodeView(x, _context);
            var yv = new GameStageNodeView(y, _context);
            var cmp = xv.ActiveTime.CompareTo(yv.ActiveTime);
            if (cmp != 0)
                return cmp;

            cmp = NodeTimeUniqueComparer.Instance.Compare(x, y);
            return cmp;
        }
    }

    internal readonly struct GameStageNodeActiveTimeComparable : IComparable<IStageNoteNode>
    {
        private readonly GameStageManager _context;
        private readonly float _time;
        public GameStageNodeActiveTimeComparable(float time, GameStageManager context)
        {
            _time = time;
            _context = context;
        }

        public int CompareTo(IStageNoteNode other)
        {
            var xv = new GameStageNodeView(other, _context);
            var cmp = _time.CompareTo(xv.ActiveTime);
            return cmp;
        }
    }
}

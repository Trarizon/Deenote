using System;
using UnityEngine;

namespace Deenote.GameStage.Grids
{
    [CreateAssetMenu(
        fileName = nameof(GridLineConfig),
        menuName = $"Deenote/GameStage/{nameof(GridLineConfig)}")]
    public sealed class GridLineConfig : ScriptableObject
    {
        public LineData LinkLineData;
        public LineData SubBeatLineData;
        public LineData BeatLineData;
        public LineData TempoLineData;
        public LineData PositionGridLineData;
        public LineData PositionGridBorderData;
        public LineData CurveLineData;

        [Serializable]
        public struct LineData
        {
            public Color Color;
            public float Width;

            public Color ColorWithAlpha(float alpha)
                => Color with { a = Color.a * alpha };
        }
    }
}
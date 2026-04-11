#nullable enable

using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Grids
{
    [MovedFrom("Deenote.GameStage")]
    [CreateAssetMenu(
        fileName = nameof(GridLineConfig),
        menuName = "Deenote/GameStage/GridLineConfig")]
    public sealed class GridLineConfig : ScriptableObject
    {
        public Color LinkLineColor = new(1f, 233f / 255f, 135f / 255f);
        public Color SubdivisionLineColor = new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);
        public Color BeatLineColor = new(0.5f, 0f, 0f, 1f);
        public Color TempoLineColor = new(0f, 0.5f, 0.5f, 1f);
        public Color PositionGridLineColor = new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);
        public Color CurveLineColor = new(85f / 255, 192f / 255, 1f);

        public float LinkLineWidth = 2f;
        public float TimeGridSubdivisionLineWidth = 2f;
        public float TimeGridBeatLineWidth = 3f;
        public float TimeGridTempoLineWidth = 4f;
        public float PositionGridLineWidth = 2f;
        public float PositionGridBorderWidth = 4f;
        public float CurveLineWidth = 2f;
    }
}

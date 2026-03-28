#nullable enable

using Deenote.CoreB.Models;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        /// <summary>
        /// The time from a note(speed==1) is activated to falls on the judgeline as Sudden+ is 0
        /// </summary>
        /// <remarks>
        /// We start to track note when note appears as if sudden+ is 0, and sets its
        /// visibility according to <see cref="StageNoteAppearAheadTime"/>
        /// </remarks>
        //public float StageNoteActiveAheadTime
        //{
        //    get {
        //        var args = Stage!.Args;
        //        var fallSpeed = ActualNoteFallSpeed;
        //        return args.NotePanelBaseLength / args.NoteTimeToZBaseMultiplierFunction.GetY(fallSpeed) / fallSpeed;
        //    }
        //}

        //public float GetStageNoteActiveAheadTime(float noteSpeed) => StageNoteActiveAheadTime / GetDisplayNoteSpeed(noteSpeed);
        //public float GetStageNoteActiveTime(IStageNoteNode node) => node.Time - GetStageNoteActiveAheadTime(node.Speed);

        /// <summary>
        /// The time from a note(speed==1) appears to falls on the judgeline
        /// </summary>
        //public float StageNoteAppearAheadTime => StageNoteActiveAheadTime * Stage.VisibleRangeCullingRatio;
        //public float GetStageNoteAppearAheadTime(float noteSpeed) => StageNoteAppearAheadTime / GetDisplayNoteSpeed(noteSpeed);
        //public float GetStageNoteAppearTime(IStageNoteNode node) => node.Time - GetStageNoteAppearAheadTime(node.Speed);

        internal float GetDisplayNoteSpeed(float speed) => IsApplySpeedDifference ? speed : 1f;

        /// <summary>
        /// Get the time as if the note has speed == 1 and it falls on the current position in world
        /// <br/>
        /// The return value may be useless if note is not active on stage
        /// </summary>
        internal float GetNotePseudoTime(float time, float noteSpeed)
        {
            var currentTime = MusicPlayer.Time;
            return currentTime + (time - currentTime) * GetDisplayNoteSpeed(noteSpeed);
        }

        #region Converters

        //public float ConvertWorldXToNoteCoordPosition(float x)
        //    => x / (Stage!.Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        //public float ConvertWorldZToNoteCoordTime(float z, float noteSpeed = 1f)
        //    => ConvertWorldZToNoteCoordTimeBase(z) / ActualNoteFallSpeed / GetDisplayNoteSpeed(noteSpeed);

        //public float ConvertNoteCoordPositionToWorldX(float position)
        //    => position * (Stage!.Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        //public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
        //    => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time);

        //public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
        //    => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time) / Stage!.Args.HoldSpritePrefab.Sprite.bounds.size.y;
        
        //public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord, float noteSpeed = 1f)
        //    => Stage!.EvaluateNoteWorldXZ(coord,noteSpeed);

        public bool TryConvertPerspectiveViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, float noteSpeed, out NoteCoord coord)
        {
            AssertStageLoaded();

            if (Stage.TryEvaluatePerspectiveViewportPointToLocalNoteCoord(perspectiveViewPanelViewportPoint, noteSpeed, out coord)) {
                coord.Time += MusicPlayer.Time;
                return true;
            }
            return false;
        }

        //internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 perspectiveViewPanelViewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
        //{
        //    AssertStageLoaded();
        //    return Stage.TryRaycastPerspectiveViewportPointToNote(perspectiveViewPanelViewportPoint, out note);
        //}

        //private float ConvertNoteCoordTimeToWorldZBase(float time)
        //    => time * Stage!.Args.NoteTimeToZBaseMultiplierFunction.GetY(ActualNoteFallSpeed);

        //private float ConvertWorldZToNoteCoordTimeBase(float z)
        //    => z / Stage!.Args.NoteTimeToZBaseMultiplierFunction.GetY(ActualNoteFallSpeed);

        #endregion
    }
}
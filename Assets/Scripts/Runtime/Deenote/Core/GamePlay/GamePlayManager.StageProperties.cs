#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GameStage;
using Deenote.Replica;
using System;
using System.Diagnostics.CodeAnalysis;
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
        [Obsolete("Use GameStageManager.StageNoteActiveAheadTime instead")]
        public float StageNoteActiveAheadTime
        {
            get {
                return App.GameStageManager.StageNoteActiveAheadTime;
            }
        }

        [Obsolete("Use GameStageManager.GetStageNoteActiveAheadTime instead")]
        public float GetStageNoteActiveAheadTime(float noteSpeed) => App.GameStageManager.GetStageNoteActiveAheadTime(noteSpeed);
        [Obsolete("Use GameStageManager.GetStageNoteActiveTime instead")]
        public float GetStageNoteActiveTime(IStageNoteNode node) => App.GameStageManager.GetStageNoteActiveTime(node);

        /// <summary>
        /// The time from a note(speed==1) appears to falls on the judgeline
        /// </summary>
        [Obsolete("Use GameStageManager.StageNoteAppearAheadTime instead")]
        public float StageNoteAppearAheadTime => App.GameStageManager.StageNoteAppearAheadTime;
        [Obsolete("Use GameStageManager.GetStageNoteAppearAheadTime instead")]
        public float GetStageNoteAppearAheadTime(float noteSpeed) => App.GameStageManager.GetStageNoteAppearAheadTime(noteSpeed);
        [Obsolete("Use GameStageManager.GetStageNoteAppearTime instead")]
        public float GetStageNoteAppearTime(IStageNoteNode node) => App.GameStageManager.GetStageNoteAppearTime(node);

        [Obsolete("Use GameStageManager.GetDisplayNoteSpeed instead")]
        internal float GetDisplayNoteSpeed(float speed) => App.GameStageManager.GetDisplayNoteSpeed(speed);

        /// <summary>
        /// Get the time as if the note has speed == 1 and it falls on the current position in world
        /// <br/>
        /// The return value may be useless if note is not active on stage
        /// </summary>
        [Obsolete("Use GameStageManager.GetNotePseudoTime instead")]
        internal float GetNotePseudoTime(float time, float noteSpeed)
        {
            return App.GameStageManager.GetNotePseudoTime(time, noteSpeed);
        }

        #region Converters

        [Obsolete("Use GameStageManager.ConvertWorldXToNoteCoordPosition instead")]
        public float ConvertWorldXToNoteCoordPosition(float x)
            => App.GameStageManager.ConvertWorldXToNoteCoordPosition(x);

        [Obsolete("Use GameStageManager.ConvertWorldZToNoteCoordTime instead")]
        public float ConvertWorldZToNoteCoordTime(float z, float noteSpeed = 1f)
            => App.GameStageManager.ConvertWorldZToNoteCoordTime(z, noteSpeed);

        [Obsolete("Use GameStageManager.ConvertNoteCoordPositionToWorldX instead")]
        public float ConvertNoteCoordPositionToWorldX(float position)
            => App.GameStageManager.ConvertNoteCoordPositionToWorldX(position);

        [Obsolete("Use GameStageManager.ConvertNoteCoordTimeToWorldZ instead")]
        public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
            => App.GameStageManager.ConvertNoteCoordTimeToWorldZ(time, noteSpeed);

        [Obsolete("Use GameStageManager.ConvertNoteCoordTimeToHoldScaleY instead")]
        public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
            => App.GameStageManager.ConvertNoteCoordTimeToHoldScaleY(time, noteSpeed);

        [Obsolete("Use GameStageManager.ConvertNoteCoordToWorldPosition instead")]
        public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord, float noteSpeed = 1f)
            => App.GameStageManager.ConvertNoteCoordToWorldPosition(coord, noteSpeed);

        [Obsolete("Use GameStageManager.TryConvertPerspectiveViewportPointToNoteCoord instead")]
        public bool TryConvertPerspectiveViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, float noteSpeed, out NoteCoord coord)
            => App.GameStageManager.TryConvertPerspectiveViewportPointToNoteCoord(perspectiveViewPanelViewportPoint, noteSpeed, out coord);

        [Obsolete("Use GameStageManager.TryRaycastPerspectiveViewportPointToNote instead")]
        internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 perspectiveViewPanelViewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
            => App.GameStageManager.TryRaycastPerspectiveViewportPointToNote(perspectiveViewPanelViewportPoint, out note);

        #endregion
    }
}
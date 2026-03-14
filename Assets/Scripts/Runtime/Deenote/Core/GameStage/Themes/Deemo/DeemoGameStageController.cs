#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.Library.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    public sealed class DeemoGameStageController : GameStageController
    {
        private static readonly int HoldCullMaxZPropertyId = Shader.PropertyToID("_CullMaxZ");

        [Space]
        [SerializeField] Camera _backgroundCamera = default!;
        [SerializeField] DeemoGameStageJudgeLineEffect _judgeLineEffect = default!;
        [SerializeField] DeemoGameStageBackgroundAnimation _backgroundAnimation = default!;
        [SerializeField] TMP_Text _staveMusicNameText = default!;
        [SerializeField] Material _holdBodyCullMaterial = default!;

        protected internal override void Initialize(GamePlayManager gamePlayManager)
        {
            base.Initialize(gamePlayManager);

            gamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void OnDestroy()
        {
            GamePlay.UnregisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
            MainSystem.ProjectManager.UnregisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void _OnActiveNotesUpdated(GamePlayManager manager)
        {
            manager.AssertChartLoaded();

            // Update judge line hit effect
            var previousHitNode = GamePlay.NotesManager.GetPreviousHitNote();
            if (previousHitNode is null) {
                _judgeLineEffect.SetHitEffect(null);
                return;
            }

            var hitTime = previousHitNode.Time;
            var deltaTime = manager.MusicPlayer.Time - hitTime;
            Debug.Assert(deltaTime >= 0);

            _judgeLineEffect.SetHitEffect(deltaTime);
        }

        private void _OnProjectNameChanged(ProjectManager manager)
        {
            if (manager.IsProjectLoaded())
                _staveMusicNameText.text = manager.CurrentProject.MusicName;
        }

        protected override void OnIsStageEffectOnChanged(bool value)
        {
            base.OnIsStageEffectOnChanged(value);

            _judgeLineEffect.BreathingEnabled = value;
            _backgroundAnimation.enabled = value;
        }

        protected override void OnVisiblaRangeCullingRatioChanged(float value)
        {
            base.OnVisiblaRangeCullingRatioChanged(value);

            var time = NoteActiveAheadTime * value;
            var z = GamePlay.Stage.EvaluateNoteWorldZ(time);
            _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
        }

        public override void ApplyCameraTargetTexture(RenderTexture renderTexture)
        {
            base.ApplyCameraTargetTexture(renderTexture);
            _backgroundCamera.targetTexture = renderTexture;
            _backgroundCamera.Render();
        }

        protected override float ConvertSuddenPlusToVisibleRangeCullingRatio(float suddenPlus)
        {
            var x = NotePlane.Origin.X;
            var minZ = NotePlane.Origin.Z;
            var maxZ = EvaluateNoteWorldZ(NoteActiveAheadTime);

            TryConvertNotePlanePositionToRaycastableViewportPoint((x, minZ), out var minVp);
            TryConvertNotePlanePositionToRaycastableViewportPoint((x, maxZ), out var maxVp);

            var vp = new Vector2(maxVp.x, Mathf.Lerp(maxVp.y, minVp.y, suddenPlus));
            TryConvertRaycastableViewportPointToNotePlanePosition(vp, out var pos);

            return Mathf.InverseLerp(minZ, maxZ, pos.Z);
        }
    }
}
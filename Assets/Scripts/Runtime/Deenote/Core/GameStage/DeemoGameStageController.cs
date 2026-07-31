#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Args;
using Deenote.Core.Project;
using Deenote.GameStage.World.Deemo;
using Deenote.Library.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.Core.GameStage
{
    public sealed class DeemoGameStageController : GameStageController
    {
        [Header("Effect")]
        [SerializeField] TMP_Text _staveMusicNameText = default!;
        [SerializeField] DeemoStageBackgroundAnimation _backgroundAnimation;
        [SerializeField] DeemoStageJudgeLineEffect _judgeLineEffect;

        [Header("Args")]
        [SerializeField] DeemoGameStageArgs _deemoArgs = default!;

        public DeemoGameStageArgs DeemoArgs => _deemoArgs;

        protected internal override void OnInstantiate(GamePlayManager manager)
        {
            base.OnInstantiate(manager);

            _manager.RegisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void OnDestroy()
        {
            _manager.UnregisterNotification(
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
            var previousHitNode = _manager.NotesManager.GetPreviousHitNote();
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
    }
}
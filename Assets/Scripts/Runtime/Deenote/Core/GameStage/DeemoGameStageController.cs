#nullable enable

using Deenote.Core.Project;
using Deenote.GameStage;
using Deenote.GameStage.World.Deemo;
using Deenote.Library.Components;
using TMPro;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public sealed class DeemoGameStageController : GameStageController
    {
        [Header("Effect")]
        [SerializeField] TMP_Text _staveMusicNameText = default!;
        [SerializeField] DeemoStageBackgroundAnimation _backgroundAnimation;
        [SerializeField] DeemoStageJudgeLineEffect _judgeLineEffect;

        protected internal override void OnInstantiate(GameStageManager stage)
        {
            base.OnInstantiate(stage);

            _stage.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameStageManager.NoteFallSpeed))
                    _OnActiveNotesUpdated();
            };
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void OnDestroy()
        {
            _stage.PropertyChanged -= (s, e) =>
            {
                if (e.PropertyName == nameof(GameStageManager.NoteFallSpeed))
                    _OnActiveNotesUpdated();
            };
            MainSystem.ProjectManager.UnregisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void _OnActiveNotesUpdated()
        {
            _stage.AssertStageLoaded();

            // Update judge line hit effect
            var previousHitNode = _stage.NotesManager.GetPreviousHitComboNode();
            if (previousHitNode is null) {
                _judgeLineEffect.SetHitEffect(null);
                return;
            }

            var hitTime = previousHitNode.Time;
            var deltaTime = _stage.MusicTime - hitTime;
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
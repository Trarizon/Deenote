#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.CoreB.Notification;
using Deenote.GameStage;
using Deenote.GameStage.UI;
using Deenote.GameStage.World.Deemo;
using Deenote.Library.Components;
using System;
using TMPro;
using TriInspector;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public sealed class DeemoGameStageController : GameStageController
    {
        private static readonly int HoldCullMaxZPropertyId = Shader.PropertyToID("_CullMaxZ");

        [Title("Deemo")]
        [SerializeField] Camera _perspectiveCamera;
        [SerializeField] Camera _backgroundCamera;

        [Header("Effect")]
        [SerializeField] TMP_Text _staveMusicNameText = default!;
        [SerializeField] DeemoStageBackgroundAnimation _backgroundAnimation;
        [SerializeField] DeemoStageJudgeLineEffect _judgeLineEffect;
        [Space]
        [SerializeField] Material _holdBodyCullMaterial = default!;

        private GameStageManager _stageManager;

        protected override Camera PerspectiveCamera => _perspectiveCamera;

        void Awake()
        {
            _stageManager = App.GameStageManager;
        }

        protected internal override void OnInstantiate(GamePlayManager manager)
        {
            base.OnInstantiate(manager);

            _stageManager.ForegroundViewPanel.ViewSizeChanged += _OnViewSizeChanged;
            _OnViewSizeChanged(_stageManager.ForegroundViewPanel);

            _manager.RegisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);

            App.ProjectManager.RegisterPropertyChangedAndInvoke((s, e, t) =>
            {
                s.CurrentProject2.AsNestedRegisterPropertyChangedAndInvoke(nameof(s.CurrentProject2), e, t, (s, e, t) =>
                {
                    if (s is null)
                        return;
                    if (e.Match(nameof(s.MusicName))) {
                        _staveMusicNameText.text = s.MusicName;
                    }
                });
            }).UnregisterWhenGameObjectDestroyed(this);
        }

        private void OnDestroy()
        {
            _stageManager.ForegroundViewPanel.ViewSizeChanged -= _OnViewSizeChanged;
            _manager.UnregisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
        }

        private void _OnViewSizeChanged(IGameStagePerspectiveViewPanel panel)
        {
            var rt = panel.ViewRendererTexture;

            // Background camera

            _backgroundCamera.targetTexture = rt;
            _backgroundCamera.Render();

            // Perspective camera

            if (_perspectiveCamera.targetTexture != rt) {
                _perspectiveCamera.targetTexture = rt;
            }
            var width = rt.width;
            var height = rt.height;
            var h = 9f / 16f * width / height;
            _perspectiveCamera.rect = new Rect(0f, 0f, 1f, h);
        }

        private void _OnActiveNotesUpdated(GamePlayManager manager)
        {
            manager.AssertChartLoaded();

            // Update judge line hit effect
            var previousHitNode = App.GameStageManager.NotesManager.GetPreviousHitNote();
            if (previousHitNode is null) {
                _judgeLineEffect.SetHitEffect(null);
                return;
            }

            var hitTime = previousHitNode.Time;
            var deltaTime = manager.MusicPlayer.Time - hitTime;
            Debug.Assert(deltaTime >= 0);

            _judgeLineEffect.SetHitEffect(deltaTime);
        }

        protected override void OnIsStageEffectOnChanged(bool value)
        {
            base.OnIsStageEffectOnChanged(value);

            _judgeLineEffect.BreathingEnabled = value;
            _backgroundAnimation.enabled = value;
        }

        protected override void OnVisibleRangePercentageChanged(float value)
        {
            base.OnVisibleRangePercentageChanged(value);
            var time = _manager.StageNoteActiveAheadTime * value;
            var z = _manager.ConvertNoteCoordTimeToWorldZ(time);
            _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
        }
    }
}
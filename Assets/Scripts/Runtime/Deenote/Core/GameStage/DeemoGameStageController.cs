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
        [SerializeField] Camera _backgroundCamera;

        [Header("Effect")]
        [SerializeField] TMP_Text _staveMusicNameText = default!;
        [SerializeField] DeemoStageBackgroundAnimation _backgroundAnimation;
        [SerializeField] DeemoStageJudgeLineEffect _judgeLineEffect;
        [Space]
        [SerializeField] Material _holdBodyCullMaterial = default!;

        private GameStageManager _stageManager = default!;

        protected internal override void OnInstantiate(GameStageManager stageManager)
        {
            base.OnInstantiate(stageManager);

            _stageManager = stageManager;
            _stageManager.NotesManager.ActiveNotesChanged += _OnActiveNotesChanged;

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
            _stageManager.NotesManager.ActiveNotesChanged -= _OnActiveNotesChanged;
        }

        private void _OnActiveNotesChanged(GameStageNotesManager s, GameStageNotesManager.ActiveNotesChangedEventArgs e)
        {
            var previousHitNode = s.GetPreviousHitNote();
            if (previousHitNode is null) {
                _judgeLineEffect.SetHitEffect(null);
                return;
            }

            var hitTime = previousHitNode.Time;
            var deltaTime = s.CurrentTime - hitTime;
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

        protected override void OnRenderedViewChanged(RenderTexture renderTexture)
        {
            // Background camera

            _backgroundCamera.targetTexture = renderTexture;
            _backgroundCamera.Render();

            // Perspective camera

            var width = renderTexture.width;
            var height = renderTexture.height;
            var h = 9f / 16f * width / height;
            PerspectiveCamera.rect = new Rect(0f, 0f, 1f, h);
        }
    }
}
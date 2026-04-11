#nullable enable

using Deenote.Contexts;
using Deenote.CoreB.Notification;
using Deenote.GameStage;
using TMPro;
using UnityEngine;

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

        protected internal override void Initialize(GameStageContext context)
        {
            base.Initialize(context);
            context.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.IsStageEffectOn))) {
                    _judgeLineEffect.BreathingEnabled = s.IsStageEffectOn;
                    _backgroundAnimation.enabled = s.IsStageEffectOn;
                }
                if (e.MatchProperty(nameof(s.SuddenPlus))) {
                    var ratio = ConvertSuddenPlusToVisibleRangeCullingRatio(s.SuddenPlus);
                    var config = ThemeEntry.Config;
                    var z = config.GetMaxWorldZ() * ratio;
                    _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
                }
            });
            
            context.NotesContext.Updated += (s) =>
            {
                var previousHitNode = s.GetPreviousHitComboNode();
                if (previousHitNode is null) {
                    _judgeLineEffect.SetHitEffect(null);
                    return;
                }

                var hitTime = previousHitNode.Time;
                var deltaTime = s.CurrentTime - hitTime;
                Debug.Assert(deltaTime >= 0);

                _judgeLineEffect.SetHitEffect(deltaTime);
            };
            context.ProjectContext.RegisterNestedPropertyChangedAndInvoke(x => x.CurrentProject, nameof(ProjectContext.CurrentProject), (s, e) =>
            {
                if (e.MatchProperty(nameof(s.MusicName))) {
                    _staveMusicNameText.text = s.MusicName;
                }
            });
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
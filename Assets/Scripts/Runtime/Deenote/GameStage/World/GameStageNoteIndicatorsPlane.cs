using Deenote.CoreB.Notification;
using Deenote.CoreB.Unity;
using Deenote.Editing;
using Deenote.Entities;
using Deenote.GamePlay;
using UnityEngine;

namespace Deenote.GameStage.World
{
    public sealed class GameStageNoteIndicatorsPlane : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;
        [SerializeField] Transform _content;

        public Transform ContentTransform => _content;

        private GamePlayManager2 _gamePlayManager = default!;
        private StageNotesPlacer2 _placer = default!;

        internal void OnInstantiate(GamePlayManager2 gamePlayManager, StageNotesPlacer2 placer)
        {
            _gamePlayManager = gamePlayManager;
            _placer = placer;

            _placer.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.AnchorCoord))) {
                    var localCoord = s.AnchorCoord - new NoteCoord(0, _gamePlayManager.CurrentTime);
                    var (x, z) = _stage.StageManager.ConvertCoordToWorld(localCoord, _stage.StageManager.PlacementNoteSpeed, true);
                    _content.WithLocalPositionXZ(x, z);
                }
            }).UnregisterWhenGameObjectDestroyed(this);
            _stage.StageManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.IsIndicatorsVisible))) {
                    ContentTransform.gameObject.SetActive(s.IsIndicatorsVisible);
                }
            }).UnregisterWhenGameObjectDestroyed(this);
        }
    }
}

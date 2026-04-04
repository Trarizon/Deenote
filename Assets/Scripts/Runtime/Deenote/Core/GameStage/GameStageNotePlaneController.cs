#nullable enable

using Deenote.Core.GameStage.Themes;
using Deenote.GameStage.Themes;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class GameStageNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageThemeConfig _config = default!;
        [SerializeField] GameStageController _stage = default!;

        public Transform ContentTransform => transform;
        public GameStageController GameStage => _stage;

        public Plane Plane => new Plane(transform.up, transform.position);

        public (float X, float Z) Origin => (0f, 0f);

        private void Awake()
        {
            
        }
    }
}

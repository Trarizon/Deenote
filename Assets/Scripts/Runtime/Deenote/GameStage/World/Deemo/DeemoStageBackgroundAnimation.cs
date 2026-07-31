using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GameStage.World.Deemo
{
    public sealed class DeemoStageBackgroundAnimation : MonoBehaviour
    {
        [SerializeField] Image _maskImage;

        [Title("Configs")]
        [SerializeField] float _period;
        [MinMaxSlider(0, 1)]
        [SerializeField] Vector2 _alphaRange;

        private float _time;

        void Update()
        {
            _time += Time.deltaTime;
            SetFrame(_time);
        }

        void OnDisable()
        {
            _time = 0;
            _maskImage.color = Color.white;
        }

        private void SetFrame(float time)
        {
            var ratio = Mathf.Sin(time * 2 * Mathf.PI / _period);
            ratio = Mathf.InverseLerp(-1f, 1f, ratio);
            _maskImage.color = Color.white with { a = Mathf.Lerp(_alphaRange.x, _alphaRange.y, ratio) };
        }
    }
}

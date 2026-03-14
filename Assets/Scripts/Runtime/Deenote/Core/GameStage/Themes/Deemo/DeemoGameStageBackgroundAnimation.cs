#nullable enable

using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    [ExecuteAlways]
    internal sealed class DeemoGameStageBackgroundAnimation : MonoBehaviour
    {
        [SerializeField] Image _maskImage = default!;

        [Header("Configs")]
        [SerializeField] float _period;
        [MinMaxSlider(0f, 1f)]
        [SerializeField] Vector2 _alphaRange;

        private float _time;

        private void Update()
        {
            _time = Time.time % _period;
            //_time += Time.deltaTime;
            //_time %= _period;
            SetFrame();
        }

        private void OnDisable()
        {
            _time = 0;
            _maskImage.color = Color.white;
        }

        private void SetFrame()
        {
            var ratio = Mathf.Sin(_time * (2f * Mathf.PI / _period));
            ratio = Mathf.InverseLerp(-1f, 1f, ratio);
            _maskImage.color = Color.white with { a = Mathf.Lerp(_alphaRange.x, _alphaRange.y, ratio) };
        }
    }
}

#nullable enable

using System;
using UnityEngine;

namespace Deenote.Systems
{
    internal sealed class MonoBehaviourHooks : MonoBehaviour
    {
        public event Action<float>? Tick;
        public event Action<bool>? ApplicationFocusChanged;
        public event Action<bool>? ApplicationPauseChanged;

        private void Update()
        {
            Tick?.Invoke(Time.deltaTime);
        }

        private void OnApplicationFocus(bool focus) => ApplicationFocusChanged?.Invoke(focus);
        private void OnApplicationPause(bool pause) => ApplicationPauseChanged?.Invoke(pause);
    }
}

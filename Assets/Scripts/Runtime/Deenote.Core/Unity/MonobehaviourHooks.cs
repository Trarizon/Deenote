using System;
using UnityEngine;

namespace Deenote.Core.Unity
{
    public sealed class MonoBehaviourHooks : MonoBehaviour
    {
        public event Action? Tick;
        public event Action<bool>? ApplicationFocusChanged;
        public event Action<bool>? ApplicationPauseChanged;

        void Update() => Tick?.Invoke();
        private void OnApplicationFocus(bool focus) => ApplicationFocusChanged?.Invoke(focus);
        private void OnApplicationPause(bool pause) => ApplicationPauseChanged?.Invoke(pause);
    }
}

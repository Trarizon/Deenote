using Deenote.CoreB.Unity;
using System;
using UnityEngine;

namespace Deenote.CoreB
{
    public abstract class Application
    {
        private static Application _current = default!;
        public static Application Current
        {
            get => _current!;
            protected set {
                if (_current is not null) {
                    Debug.LogError("Application.Current is already set");
                    return;
                }
                _current = value;
            }
        }

        public Deenote.Core.Logging.Logger Logger { get; } = new();

        private readonly UnityHooks _unity;

        protected Application()
        {
            _unity = new UnityHooks();
            _unity.Hooks.Tick += () =>
            {
                UnscaledTick?.Invoke(Time.unscaledDeltaTime);
            };

            UnityEngine.Application.wantsToQuit += () =>
            {
                var args = new System.ComponentModel.CancelEventArgs();
                Quitting?.Invoke(args);
                return !args.Cancel;
            };
            UnityEngine.Application.quitting += () => Quitted?.Invoke();
        }

        public T AddGlobalComponent<T>() where T : Component
            => _unity.SharedGameObject.AddComponent<T>();


        internal static void OnDomainReloaded()
        {
            _current = null!;
        }

        public event Action<bool>? FocusChanged
        {
            add => _unity.Hooks.ApplicationFocusChanged += value;
            remove => _unity.Hooks.ApplicationFocusChanged -= value;
        }

        public event Action<bool>? PauseChanged
        {
            add => _unity.Hooks.ApplicationPauseChanged += value;
            remove => _unity.Hooks.ApplicationPauseChanged -= value;
        }

        public event Action<float>? UnscaledTick;

        public event Action<System.ComponentModel.CancelEventArgs>? Quitting;
        public event Action? Quitted;

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
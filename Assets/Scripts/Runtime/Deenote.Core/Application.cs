using Deenote.CoreB.Unity;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.CoreB
{
    public abstract class Application
    {
        public static Application Current { get; private set; } = default!;

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
                var args = new CancelEventArgs();
                Quitting?.Invoke(args);
                return !args.Cancel;
            };
            UnityEngine.Application.quitting += () => Quitted?.Invoke();
        }

        protected static void Apply(Application application)
        {
            Current = application;
        }

        internal static void OnDomainReloaded()
        {
            Current = null!;
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

        public event Action<CancelEventArgs>? Quitting;
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
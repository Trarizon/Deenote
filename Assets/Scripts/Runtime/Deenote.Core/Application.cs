using Cysharp.Threading.Tasks;
using Deenote.Core.Unity;
using Deenote.CoreB.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        protected async UniTask<T> LoadSceneAsync<T>(string sceneName, LoadSceneMode mode) where T : SceneObject<T>
        {
            var utcs = new UniTaskCompletionSource<SceneObject<T>>();
            SceneObject<T>._loadedCallback += Cb;
            try {
                Logger.LogTrace($"Loading scene {sceneName}");

                await SceneManager.LoadSceneAsync(sceneName, mode).ToUniTask();

                Logger.LogTrace($"Scene {sceneName} loaded");
            } catch {
                SceneObject<T>._loadedCallback -= Cb;
            }
            var so = await utcs.Task;

            Logger.LogTrace($"SceneObject {sceneName} awaked");
            return (T)so;

            void Cb(SceneObject<T> obj)
            {
                utcs.TrySetResult(obj);
                SceneObject<T>._loadedCallback -= Cb;
            }
        }
    }
}
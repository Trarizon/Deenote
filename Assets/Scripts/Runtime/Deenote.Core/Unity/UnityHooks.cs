using Deenote.Core.Unity;
using Deenote.CoreB.Localization;
using UnityEngine;

namespace Deenote.CoreB.Unity
{
    public class UnityHooks
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReloaded()
        {
            Application.OnDomainReloaded();
            LocalizationSystem.OnDomainReloaded();
        }

        private GameObject? _sharedGameObject;
        public GameObject SharedGameObject
        {
            get {
                if (_sharedGameObject is null) {
                    _sharedGameObject = new GameObject("UnityHooks.SharedGameObject");
                    UnityEngine.Object.DontDestroyOnLoad(_sharedGameObject);
                }
                return _sharedGameObject;
            }
        }

        private MonoBehaviourHooks? _hooks;
        internal MonoBehaviourHooks Hooks => _hooks ??= SharedGameObject.AddComponent<MonoBehaviourHooks>();
    }
}

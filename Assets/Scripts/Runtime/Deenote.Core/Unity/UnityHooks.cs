using Deenote.CoreB.Localization;
using UnityEngine;

namespace Deenote.CoreB.Unity
{
    public static class UnityHooks
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReloaded()
        {
            LocalizationSystem.OnDomainReloaded();
        }
    }
}

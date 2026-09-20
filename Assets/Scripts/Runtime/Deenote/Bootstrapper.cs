using UnityEngine;

namespace Deenote
{
    [DefaultExecutionOrder(-99)]
    internal sealed class Bootstrapper : MonoBehaviour
    {
        void Awake()
        {
            var app = new App();
        }
    }
}
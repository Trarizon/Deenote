using UnityEngine;

namespace Deenote
{
    public sealed class Bootstrap : MonoBehaviour
    {

        private void Awake()
        {
            App app = App.Create();
        }
    }
}

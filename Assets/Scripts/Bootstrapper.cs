using Deenote.UI.Views;
using UnityEngine;

namespace Deenote
{
    [DefaultExecutionOrder(-99)]
    internal sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] PerspectiveViewPanelView _perspectiveViewPanel;
        void Awake()
        {
            var app = App.Create(_perspectiveViewPanel);
        }
    }
}
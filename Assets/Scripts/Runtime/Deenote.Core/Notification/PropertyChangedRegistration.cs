using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace Deenote.CoreB.Notification
{
    public interface IPropertyChangedRegistration
    {
        void Unregister();
    }

    partial class INotifyPropertyChangedExtensions
    {
        public static T UnregisterWhenGameObjectDestroyed<T>(this T registration, GameObject gameObject)
            where T : IPropertyChangedRegistration
        {
            gameObject.GetAsyncDestroyTrigger().CancellationToken.RegisterWithoutCaptureExecutionContext((state) =>
            {
                ((T)state).Unregister();
            }, registration);

            return registration;
        }

        public static T UnregisterWhenGameObjectDestroyed<T>(this T registration, Component component)
            where T : IPropertyChangedRegistration
            => registration.UnregisterWhenGameObjectDestroyed(component.gameObject);
    }
}

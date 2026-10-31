using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.CoreB.Notification
{
    public interface IPropertyChangedRegistration
    {
        void Unregister();
    }

    public readonly struct PropertyChangedRegistration<T> : IPropertyChangedRegistration where T : INotifyPropertyChanged<T>
    {
        internal readonly T _self;
        internal readonly Action<T, PropertyChangedEventArgs> _action;
        internal PropertyChangedRegistration(T self, Action<T, PropertyChangedEventArgs> action)
        {
            _self = self;
            _action = action;
        }

        public void Unregister() => _self.PropertyChanged -= _action;
    }

    public readonly struct PropertyChangedRegistrationWithNest<T> : IPropertyChangedRegistration where T : INotifyPropertyChanged<T>
    {
        private readonly PropertyChangedRegistration<T> _registration;
        private readonly IPropertyChangedRegistration _nest;

        internal PropertyChangedRegistrationWithNest(PropertyChangedRegistration<T> registration, IPropertyChangedRegistration nest)
        {
            _registration = registration;
            _nest = nest;
        }

        public void Unregister()
        {
            _registration.Unregister();
            _nest.Unregister();
        }
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

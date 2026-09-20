using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.CoreB.Notification
{
    public interface INotifyPropertyChanged<T> where T : INotifyPropertyChanged<T>
    {
        event Action<T, PropertyChangedEventArgs>? PropertyChanged;
    }

    public readonly struct PropertyChangedEventArgs
    {
        public static PropertyChangedEventArgs AnyProperty => default;

        public string? PropertyName { get; }

        public PropertyChangedEventArgs(string? propertyName) => PropertyName = propertyName;

        public bool Match(string propertyName)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName;

        public bool MatchAny(string propertyName0, string propertyName1)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName0 || PropertyName == propertyName1;
    }

    public readonly struct PropertyChangedRegistration<T>
    {
        internal readonly T _self;
        internal readonly Action<T, PropertyChangedEventArgs> _action;
        internal PropertyChangedRegistration(T self, Action<T, PropertyChangedEventArgs> action)
        {
            _self = self;
            _action = action;
        }
    }

    public static class INotifyPropertyChangedExtensions
    {
        public static void Invoke<T>(this Action<T, PropertyChangedEventArgs> action, T self, string propertyName)
            where T : INotifyPropertyChanged<T>
            => action(self, new PropertyChangedEventArgs(propertyName));

        public static PropertyChangedRegistration<T> RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyChangedEventArgs> action) where T : INotifyPropertyChanged<T>
        {
            action(self, PropertyChangedEventArgs.AnyProperty);
            self.PropertyChanged += action;
            return new PropertyChangedRegistration<T>(self, action);
        }

        public static PropertyChangedRegistration<T> UnregisterWhenGameObjectDestroyed<T>(this PropertyChangedRegistration<T> registration, GameObject gameObject) where T : INotifyPropertyChanged<T>
        {
            gameObject.GetAsyncDestroyTrigger().CancellationToken.RegisterWithoutCaptureExecutionContext((state) =>
            {
                var box = (StrongBox<PropertyChangedRegistration<T>>)state;
                box.Value._self.PropertyChanged -= box.Value._action;
            }, new StrongBox<PropertyChangedRegistration<T>>(registration));
            
            return registration;
        }

        public static PropertyChangedRegistration<T> UnregisterWhenGameObjectDestroyed<T>(this PropertyChangedRegistration<T> registration, Component component) where T : INotifyPropertyChanged<T>
            => registration.UnregisterWhenGameObjectDestroyed(component.gameObject);
    }
}

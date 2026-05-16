#nullable enable

using System;
using UnityEngine;

namespace Deenote.CoreB.Notification
{
    public interface INotifyPropertyChanged<T>
    {
        event Action<T, PropertyEventArgs>? PropertyChanged;
    }

    public interface INotifyPropertyChanging<T>
    {
        event Action<T, PropertyEventArgs>? PropertyChanging;
    }

    public readonly struct PropertyEventArgs
    {
        public static PropertyEventArgs AllProperties => default;

        public string PropertyName { get; }
        public PropertyEventArgs(string propertyName) => PropertyName = propertyName;

        public bool MatchProperty(string propertyName)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName;
    }

    public readonly struct PropertyChangedRegistration<T>
    {
        private readonly T _self;
        private readonly Action<T, PropertyEventArgs> _handler;

        internal PropertyChangedRegistration(T self, Action<T, PropertyEventArgs> handler)
        {
            _self = self;
            _handler = handler;
        }

        public void UnregisterWhenGameObjectDisabled(GameObject go)
        {
        }
    }

    public readonly struct PropertyChangingRegistration<T>
    {
        private readonly T _self;
        private readonly Action<T, PropertyEventArgs> _handler;

        internal PropertyChangingRegistration(T self, Action<T, PropertyEventArgs> handler)
        {
            _self = self;
            _handler = handler;
        }

        public void UnregisterWhenGameObjectDisabled(GameObject go)
        {
        }
    }

    public static class NotifyPropertyChangedHelpers
    {
        public static void RegisterNestedPropertyChangedAndInvoke<T, T1>(this T self, Func<T, T1?> property, string propName, Action<T1, PropertyEventArgs> action)
            where T : INotifyPropertyChanged<T>, INotifyPropertyChanging<T>
            where T1 : INotifyPropertyChanged<T1>
        {
            self.RegisterPropertyChangingAndInvoke((s, e) =>
            {
                if (e.MatchProperty(propName)) {
                    var prop = property(s);
                    if (prop is not null)
                        prop.PropertyChanged -= action;
                }
            });
            self.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(propName)) {
                    property(s)?.RegisterPropertyChangedAndInvoke(action);
                }
            });
        }

        public static void RegisterNestedPropertyChangedAndInvokeNullable<T, T1>(this T self, Func<T, T1?> property, string propName, Action<T1?, PropertyEventArgs> action)
            where T : INotifyPropertyChanged<T>, INotifyPropertyChanging<T>
            where T1 : INotifyPropertyChanged<T1>
        {
            self.RegisterPropertyChangingAndInvoke((s, e) =>
            {
                if (e.MatchProperty(propName)) {
                    var prop = property(s);
                    if (prop is not null)
                        prop.PropertyChanged -= action;
                }
            });
            self.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(propName)) {
                    var prop = property(s);
                    if (prop is not null)
                        prop.PropertyChanged += action;
                    action(prop, PropertyEventArgs.AllProperties);
                }
            });
        }

        public static PropertyChangedRegistration<T> RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyEventArgs> action)
            where T : INotifyPropertyChanged<T>
        {
            self.PropertyChanged += action;
            action(self, PropertyEventArgs.AllProperties);
            return new PropertyChangedRegistration<T>(self, action);
        }

        public static PropertyChangingRegistration<T> RegisterPropertyChangingAndInvoke<T>(this T self, Action<T, PropertyEventArgs> action)
            where T : INotifyPropertyChanging<T>
        {
            self.PropertyChanging += action;
            action(self, PropertyEventArgs.AllProperties);
            return new PropertyChangingRegistration<T>(self, action);
        }
    }
}

using System;

namespace Deenote.CoreB.Notification
{
    public interface INotifyPropertyChanging<T> where T : INotifyPropertyChanging<T>
    {
        event Action<T, PropertyChangingEventArgs>? PropertyChanging;
    }

    public readonly struct PropertyChangingEventArgs
    {
        public static PropertyChangingEventArgs AnyProperty => default;

        public string? PropertyName { get; }

        public PropertyChangingEventArgs(string? propertyName) => PropertyName = propertyName;

        public bool Match(string propertyName)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName;

        public bool MatchAny(string propertyName0, string propertyName1)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName0 || PropertyName == propertyName1;
    }

    public readonly struct PropertyChangingRegistration<T> where T : INotifyPropertyChanging<T>
    {
        internal readonly T _self;
        internal readonly Action<T, PropertyChangingEventArgs> _action;
        internal PropertyChangingRegistration(T self, Action<T, PropertyChangingEventArgs> action)
        {
            _self = self;
            _action = action;
        }

        public void Unregister()
        {
            _self.PropertyChanging -= _action;
        }
    }

    partial class INotifyPropertyChangedExtensions
    {
        public static void Invoke<T>(this Action<T, PropertyChangingEventArgs> action, T self, string propertyName)
            where T : INotifyPropertyChanging<T>
            => action(self, new PropertyChangingEventArgs(propertyName));

        public static void Invoke<T>(this Action<T, PropertyChangingEventArgs> action, T self, string propertyName, string propertyName2)
            where T : INotifyPropertyChanging<T>
        {
            action(self, new PropertyChangingEventArgs(propertyName));
            action(self, new PropertyChangingEventArgs(propertyName2));
        }
    }
}

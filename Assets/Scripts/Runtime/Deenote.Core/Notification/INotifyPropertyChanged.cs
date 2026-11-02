using Deenote.CoreB.Notification;
using System;

namespace Deenote.CoreB.Notification
{
    public interface INotifyPropertyChanged<T> where T : INotifyPropertyChanged<T>?
    {
        event Action<T, PropertyChangedEventArgs>? PropertyChanged;
    }

    public readonly struct PropertyChangedEventArgs
    {
        public static PropertyChangedEventArgs All => default;

        public string? PropertyName { get; }

        public PropertyChangedEventArgs(string? propertyName) => PropertyName = propertyName;

        public bool Match(string propertyName)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName;

        public bool MatchAny(string propertyName0, string propertyName1)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName0 || PropertyName == propertyName1;
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

    public static partial class INotifyPropertyChangedExtensions
    {
        public static void Invoke<T>(this Action<T, PropertyChangedEventArgs> action, T self, string propertyName)
            where T : INotifyPropertyChanged<T>
            => action(self, new PropertyChangedEventArgs(propertyName));

        public static void Invoke<T>(this Action<T, PropertyChangedEventArgs> action, T self, string propertyName, string propertyName2)
            where T : INotifyPropertyChanged<T>
        {
            action(self, new PropertyChangedEventArgs(propertyName));
            action(self, new PropertyChangedEventArgs(propertyName2));
        }

        public static PropertyChangedRegistration<T> RegisterPropertyChanged<T>(this T self, Action<T, PropertyChangedEventArgs> action) where T : INotifyPropertyChanged<T>
        {
            self.PropertyChanged += action;
            return new PropertyChangedRegistration<T>(self, action);
        }

        public static PropertyChangedRegistration<T> RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyChangedEventArgs> action) where T : INotifyPropertyChanged<T>
        {
            action(self, PropertyChangedEventArgs.All);
            self.PropertyChanged += action;
            return new PropertyChangedRegistration<T>(self, action);
        }
    }
}

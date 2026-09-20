using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

    public static class INotifyPropertyChangedExtensions
    {
        public static void Invoke<T>(this Action<T, PropertyChangedEventArgs> action, T self, string propertyName)
            where T : INotifyPropertyChanged<T>
            => action(self, new PropertyChangedEventArgs(propertyName));

        public static void RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyChangedEventArgs> action) where T : INotifyPropertyChanged<T>
        {
            action(self, PropertyChangedEventArgs.AnyProperty);
            self.PropertyChanged += action;
        }
    }
}

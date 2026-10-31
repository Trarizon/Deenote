using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Deenote.CoreB.Notification;
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
            action(self, PropertyChangedEventArgs.AnyProperty);
            self.PropertyChanged += action;
            return new PropertyChangedRegistration<T>(self, action);
        }
    }
}


static class Design
{
    static void Run()
    {
        var parent = new Parent();

        parent.RegisterPropertyChanged((s, e) =>
        {
            if (e.Match(nameof(s.Name))) {
                // ...
            }
            if (e.Match(nameof(s.Desc))) {
                // ...
            }
        });//.UnregisterWhenGameObjectDestroyed(this);

        parent.RegisterPropertyChanged((s, e, r) =>
        {
            if (e.Match(nameof(s.Child))) {
                s.Child.RegisterAsNestedPropertyChanged(s, nameof(s.Child), r, (s, e, r) =>
                {
                    if (e.Match(nameof(s.Name))) {
                        // ...
                    }
                    if (e.Match(nameof(s.GrandChild))) {
                        s.GrandChild.RegisterAsNestedPropertyChanged(s, nameof(s.GrandChild), r, (s, e, r) =>
                        {
                            if (e.Match(nameof(s.Name))) {
                                // ...
                            }
                        });
                    }
                });
            }
        });//.UnregisterWhenGameObjectDestroyed(this);

    }


    class Parent : INotifyPropertyChanged<Parent>, INotifyPropertyChanging<Parent>
    {
        public string Name { get; }
        public string Desc { get; }
        public Child Child { get; set; } = default!;
        public event Action<Parent, PropertyChangedEventArgs>? PropertyChanged;
        public event Action<Parent, PropertyChangingEventArgs>? PropertyChanging;
    }

    class Child : INotifyPropertyChanged<Child>, INotifyPropertyChanging<Child>
    {
        public string Name { get; }
        public GrandChild GrandChild { get; set; } = default!;
        public event Action<Child, PropertyChangedEventArgs>? PropertyChanged;
        public event Action<Child, PropertyChangingEventArgs>? PropertyChanging;
    }

    class GrandChild : INotifyPropertyChanged<GrandChild>, INotifyPropertyChanging<GrandChild>
    {
        public string Name { get; }
        public event Action<GrandChild, PropertyChangedEventArgs>? PropertyChanged;
        public event Action<GrandChild, PropertyChangingEventArgs>? PropertyChanging;
    }
}
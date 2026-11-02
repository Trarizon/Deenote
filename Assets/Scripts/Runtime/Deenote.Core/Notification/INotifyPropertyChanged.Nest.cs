using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.CoreB.Notification
{
    public struct NestPropertyRegistrationHandle
    {
        internal ObservableBindings Bindings;
        internal ushort Version;
    }

    internal abstract class ObservableBindings : IPropertyChangedRegistration
    {
        private readonly List<(string PropName, ObservableBindings? Bindings)> _listeners = new();
        // Version incremented when PropertyChanged is called, so we can know if we should replace or append to _listeners when call Reuse()
        protected ushort _version;

        [return: NotNullIfNotNull("propertyValue")]
        public ObservableBindings? AddOrUpdateBindings<T>(string propertyName, T? propertyValue, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action, ushort version)
            where T : class?, INotifyPropertyChanged<T>?
        {
            var span = _listeners.AsSpan();
            foreach (ref var item in span) {
                if (item.PropName == propertyName) {
                    if (item.Bindings == null) {
                        if (propertyValue is not null)
                            return item.Bindings = new ObservableBindings<T>(propertyValue, action);
                        // if self is null, we needn't register any event handler, so keep item.Bindings as null
                        return null;
                    }
                    else {
                        // A type's defined property will always be the same type, so we directly cast it to ObservableBindings<T>
                        var typed = (ObservableBindings<T>)item.Bindings;
                        if (this._version == version) {
                            typed.Append(action);
                        }
                        else {
                            typed.Replace(propertyValue, action);
                        }
                        return item.Bindings;
                    }
                }
            }
            // if self is null, we needn't register any event handler, so keep item.Bindings as null
            if (propertyValue is null)
                return null;

            var result = new ObservableBindings<T>(propertyValue, action);
            _listeners.Add((propertyName, result));
            return result;
        }

        protected void UnregisterChildren()
        {
            foreach (var item in _listeners) {
                item.Bindings?.Unregister();
            }
            _listeners.Clear();
        }

        public abstract void Unregister();
    }

    internal sealed class ObservableBindings<T> : ObservableBindings where T : class?, INotifyPropertyChanged<T>?
    {
        internal T? _sender;
        private Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle>? _listeners;

        public ObservableBindings(T sender, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
        {
            _sender = sender;
            _listeners = action;
            if (_sender is not null)
                _sender.PropertyChanged += PropertyChanged;
        }

        public void Replace(T? sender, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
        {
            UnregisterChildren();
            if (_sender != null) {
                _sender.PropertyChanged -= PropertyChanged;
            }
            _sender = sender;
            _listeners = action;
            if (_sender is not null)
                _sender.PropertyChanged += PropertyChanged;
        }

        public void Append(Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
        {
            _listeners += action;
        }

        private void PropertyChanged(T sender, PropertyChangedEventArgs e)
        {
            ushort version;
            unchecked {
                version = (ushort)(_version + 2);
                _version++;
            }
            // We pass _version + 2 to listener, and set _version to _version + 1, and so that we ensure the first registration use Replace instead of Append
            // And before Replace, we set _version to version.
            // 
            // Take _version = 0 as example.
            // We pass version = 2 to listener, and set _version to 1.
            // When AsNestedRegisterPropertyChanged is called, and we find a existing binding with propertyName, this moment, _version = 1 and version from parameter = 2
            // So we forced to call Replace. And before call Replace, we set _version to version(2), so that the next time we call AsNestedRegisterPropertyChanged in same scope, we get _version == version == 2, and use Append instead of Replace.
            // And the next time PropertyChanged calls, we set _version = 3 and pass version = 4 ...
            _listeners?.Invoke(sender, e, new NestPropertyRegistrationHandle {
                Bindings = this,
                Version = unchecked(version),
            });
        }

        public override void Unregister()
        {
            UnregisterChildren();
            if (_sender is not null) {
                _sender.PropertyChanged -= PropertyChanged;
                _sender = default;
            }
            _listeners = null;
        }
    }

    partial class INotifyPropertyChangedExtensions
    {
        public static IPropertyChangedRegistration RegisterPropertyChanged<T>(this T self, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
            where T : class, INotifyPropertyChanged<T>
        {
            var bindings = new ObservableBindings<T>(self, action);
            return bindings;
        }

        public static IPropertyChangedRegistration RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
            where T : class, INotifyPropertyChanged<T>
        {
            var bindings = new ObservableBindings<T>(self, action);
            action.Invoke(self, PropertyChangedEventArgs.All, new NestPropertyRegistrationHandle {
                Bindings = bindings,
            });
            return bindings;
        }

        public static void AsNestedRegisterPropertyChanged<T>(this T? self, string propertyName, NestPropertyRegistrationHandle handle, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
            where T : class, INotifyPropertyChanged<T>
        {
            // Assert handle._sender.[propertyName] == self
            var bindings = handle.Bindings.AddOrUpdateBindings(propertyName, self, action, handle.Version);
        }

        public static void AsNestedRegisterPropertyChangedAndInvoke<T>(this T self, string propertyName, NestPropertyRegistrationHandle handle, Action<T, PropertyChangedEventArgs, NestPropertyRegistrationHandle> action)
            where T : class?, INotifyPropertyChanged<T>?
        {
            // Assert handle._sender.[propertyName] == self
            var bindings = handle.Bindings.AddOrUpdateBindings(propertyName, self, action, handle.Version);

            action.Invoke(self, PropertyChangedEventArgs.All, new NestPropertyRegistrationHandle {
                // When self is null, the call site won't able to access any property of self,
                // and then the handle is useless, so its safe to just use default(NestPropertyRegistrationHandle)
                Bindings = bindings!,
            });
        }

        public static void RegisterNestedPropertyChangedAndInvoke<T, TProperty>(this T self, string propertyName, Func<T, TProperty> getter, Action<TProperty, PropertyChangedEventArgs> action)
            where T : class, INotifyPropertyChanged<T>
            where TProperty : class, INotifyPropertyChanged<TProperty>
        {
            self.RegisterPropertyChanged((s, e, h) =>
            {
                if (e.Match(propertyName)) {
                    getter(s).AsNestedRegisterPropertyChangedAndInvoke(propertyName, h, (p, e, h) =>
                    {
                        action.Invoke(p, e);
                    });
                }
            });
        }
    }
}

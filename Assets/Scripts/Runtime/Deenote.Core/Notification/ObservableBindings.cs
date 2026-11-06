using ObservableCollections;
using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Notification
{
    public readonly struct ObservableBindingToken
    {
        internal readonly ObservableBinding Binding;
        internal ObservableBindingToken(ObservableBinding binding)
        {
            Binding = binding;
        }
    }

    internal interface IObservableBinding<T, THandler> : IPropertyChangedRegistration where THandler : Delegate
    {
        void Update(T value, THandler handler);
        void Append(THandler handler);
    }

    internal abstract class ObservableBinding : IPropertyChangedRegistration
    {
        // _version updates when _PropertyChanged invoked;
        // reregistration happened when _PropertyChanged invoked;
        // so we use _child.Version to record which version the binding last updated,
        // if (_child.Version == _version) means this is not first `AsNestedRegisterPropertyChangedAndInvoke` in listener
        // so we append the handler
        // otherwise, we know the property maybe changed, so we update the binding
        private List<(string PropertyName, uint Version, ObservableBinding Binding)>? _children;
        protected uint _version;

        public ObservableBindingToken AddOrUpdate<T, THandler, TBinding>(string propertyName, T propertyValue, THandler handler, Func<TBinding> factory)
            where THandler : Delegate
            where TBinding : ObservableBinding, IObservableBinding<T, THandler>
        {
            var span = _children.AsSpan();
            foreach (ref var item in span) {
                if (item.PropertyName != propertyName)
                    continue;

                var typed = (IObservableBinding<T, THandler>)item.Binding;
                if (_version == item.Version) {
                    typed.Append(handler);
                }
                else {
                    typed.Update(propertyValue, handler);
                    item.Version = _version;
                }
                return new ObservableBindingToken(item.Binding);
            }

            // not found, append new binding

            // if property is null, there's no need to register handler, so keep binding null
            if (propertyValue is null)
                return default;

            var binding = factory();
            binding.Update(propertyValue, handler);

            _children ??= new();
            _children.Add((propertyName, _version, binding));

            return new ObservableBindingToken(binding);
        }

        public ObservableBindingToken AddOrUpdate<TProperty>(string propertyName, TProperty propertyValue, Action<TProperty, PropertyChangedEventArgs, ObservableBindingToken> handler)
            where TProperty : INotifyPropertyChanged<TProperty?>?
            => AddOrUpdate(propertyName, propertyValue, handler, static () => new ObservableBinding<TProperty>());

        public ObservableBindingToken AddOrUpdate<TElement>(string propertyName, IObservableCollection<TElement> collection, CollectionChangedNestedEventHandler<TElement> handler)
            => AddOrUpdate(propertyName, collection, handler, static () => new ObservableCollectionBinding<TElement>());

        protected void UnregisterChildren()
        {
            if (_children is null)
                return;

            foreach (var item in _children) {
                item.Binding.Unregister();
            }
            _children.Clear();
        }

        public abstract void Unregister();
    }

    internal sealed class ObservableBinding<T> : ObservableBinding, IObservableBinding<T, Action<T, PropertyChangedEventArgs, ObservableBindingToken>>
        where T : INotifyPropertyChanged<T?>?
    {
        private T _self = default!;
        private Action<T, PropertyChangedEventArgs, ObservableBindingToken>? _handlers;

        public ObservableBinding() { }

        public void Update(T value, Action<T, PropertyChangedEventArgs, ObservableBindingToken> handler)
        {
            UnregisterChildren();
            if (_self is not null) {
                _self.PropertyChanged -= _PropertyChanged;
            }

            _self = value;
            _handlers = handler;

            if (_self is not null) {
                _self.PropertyChanged += _PropertyChanged;
            }
        }

        public void Append(Action<T, PropertyChangedEventArgs, ObservableBindingToken> handler)
        {
            _handlers += handler;
        }

        private void _PropertyChanged(T? sender, PropertyChangedEventArgs e)
        {
            uint version;
            unchecked {
                version = _version + 2;
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
            _handlers?.Invoke(sender!, e, new(this));
        }

        public override void Unregister()
        {
            UnregisterChildren();
            if (_self is not null)
                _self.PropertyChanged -= _PropertyChanged;
            _handlers = null;
        }
    }

    internal sealed class ObservableCollectionBinding<T> : ObservableBinding, IObservableBinding<IObservableCollection<T>, CollectionChangedNestedEventHandler<T>>
    {
        private IObservableCollection<T> _self = default!;
        private CollectionChangedNestedEventHandler<T>? _handlers;

        public ObservableCollectionBinding() { }

        public void Update(IObservableCollection<T> value, CollectionChangedNestedEventHandler<T> handler)
        {
            UnregisterChildren();
            if (_self is not null) {
                _self.CollectionChanged -= _CollectionChanged;
            }

            _self = value;
            _handlers = handler;

            if (_self is not null) {
                _self.CollectionChanged += _CollectionChanged;
            }
        }

        public void Append(CollectionChangedNestedEventHandler<T> handler)
        {
            _handlers += handler;
        }

        private void _CollectionChanged(in NotifyCollectionChangedEventArgs<T> e)
        {
            uint version;
            unchecked {
                version = _version + 2;
                _version++;
            }
            _handlers?.Invoke(CollectionChangedEventArgs<T>.From(e), new(this));
        }

        public override void Unregister()
        {
            UnregisterChildren();
            if (_self is not null)
                _self.CollectionChanged -= _CollectionChanged;
            _handlers = null;
        }
    }
}

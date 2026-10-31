using System;

namespace Deenote.CoreB.Notification
{
    public interface INestPropertyChangedRegistrationBranch : IPropertyChangedRegistration
    {
        internal void Add(NestedPropertyRegistrationNonRoot nest);
        internal void Remove(NestedPropertyRegistrationNonRoot nest);

        internal struct ChildrenLinkedList
        {
            NestedPropertyRegistrationNonRoot? _head;

            public void Add(NestedPropertyRegistrationNonRoot nest)
            {
                nest.Next = _head;
                _head = nest;
            }

            public void Remove(NestedPropertyRegistrationNonRoot unregister)
            {
                if (_head is null)
                    return;
                if (_head == unregister) {
                    _head = _head.Next;
                    return;
                }
                var prev = _head;
                var cur = prev.Next;
            Loop:
                if (cur is null)
                    return;
                else if (cur == unregister) {
                    prev.Next = cur.Next;
                    return;
                }
                else {
                    prev = cur;
                    cur = cur.Next;
                    goto Loop;
                }
            }

            public void UnregisterAll()
            {
                var head = _head;
                while (head != null) {
                    var cur = head;
                    head = cur.Next;
                    cur.Next = null;
                    cur.Unregister();
                }
                _head = null;
            }
        }
    }

    internal abstract class NestedPropertyRegistrationNonRoot : IPropertyChangedRegistration
    {
        public NestedPropertyRegistrationNonRoot? Next;
        public abstract void Unregister();
    }

    internal sealed class NestedPropertyRegistrationRoot<T> : INestPropertyChangedRegistrationBranch
        where T : INotifyPropertyChanged<T>, INotifyPropertyChanging<T>
    {
        private readonly T _self;
        private readonly Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> _listener;
        private INestPropertyChangedRegistrationBranch.ChildrenLinkedList _head;

        internal NestedPropertyRegistrationRoot(T self, Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> action)
        {
            _self = self;
            _listener = action;

            _self.PropertyChanged += PropertyChanged;
        }

        private void PropertyChanged(T s, PropertyChangedEventArgs e) => _listener(s, e, this);
        public void Add(NestedPropertyRegistrationNonRoot unregister) => _head.Add(unregister);
        public void Remove(NestedPropertyRegistrationNonRoot unregister) => _head.Remove(unregister);
        public void Unregister()
        {
            _self.PropertyChanged -= PropertyChanged;
            _head.UnregisterAll();
        }
    }

    internal sealed class NestedPropertyRegistrationLeaf<T, TOwner> : NestedPropertyRegistrationNonRoot
        where T : INotifyPropertyChanged<T>
        where TOwner : INotifyPropertyChanged<TOwner>, INotifyPropertyChanging<TOwner>
    {
        private readonly TOwner _owner;
        private readonly string _propertyName;
        private readonly T _value;
        private readonly INestPropertyChangedRegistrationBranch _registration;
        private readonly Action<T, PropertyChangedEventArgs> _listener;

        public NestedPropertyRegistrationLeaf(T value, TOwner owner, string propertyName, INestPropertyChangedRegistrationBranch registration, Action<T, PropertyChangedEventArgs> action)
        {
            _value = value;
            _owner = owner;
            _propertyName = propertyName;
            _registration = registration;
            _listener = action;

            _owner.PropertyChanging += Owner_PropertyChanging;
            _value.PropertyChanged += _listener;
            _registration.Add(this);
        }

        private void Owner_PropertyChanging(TOwner s, PropertyChangingEventArgs e)
        {
            if (e.Match(_propertyName)) {
                Unregister();
            }
        }

        public override void Unregister()
        {
            _owner.PropertyChanging -= Owner_PropertyChanging;
            _value.PropertyChanged -= _listener;
            _registration.Remove(this);
        }
    }

    internal sealed class NestedPropertyRegistrationBranch<T, TOwner> : NestedPropertyRegistrationNonRoot, INestPropertyChangedRegistrationBranch
        where T : INotifyPropertyChanged<T>
        where TOwner : INotifyPropertyChanged<TOwner>, INotifyPropertyChanging<TOwner>
    {
        private readonly TOwner _owner;
        private readonly string _propertyName;
        private readonly T _value;
        private readonly INestPropertyChangedRegistrationBranch _registration;
        private readonly Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> _listener;

        private INestPropertyChangedRegistrationBranch.ChildrenLinkedList _head;

        public NestedPropertyRegistrationBranch(T value, TOwner owner, string propertyName, INestPropertyChangedRegistrationBranch registration, Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> action)
        {
            _value = value;
            _owner = owner;
            _propertyName = propertyName;
            _registration = registration;
            _listener = action;

            _owner.PropertyChanging += Owner_PropertyChanging;
            _value.PropertyChanged += Value_PropertyChanged;
            _registration.Add(this);
        }

        private void Owner_PropertyChanging(TOwner s, PropertyChangingEventArgs e)
        {
            if (e.Match(_propertyName)) {
                Unregister();
            }
        }

        private void Value_PropertyChanged(T s, PropertyChangedEventArgs e)
        {
            _listener(s, e, this);
        }

        public void Add(NestedPropertyRegistrationNonRoot unregister) => _head.Add(unregister);

        public void Remove(NestedPropertyRegistrationNonRoot unregister) => _head.Remove(unregister);

        public override void Unregister()
        {
            _owner.PropertyChanging -= Owner_PropertyChanging;
            _value.PropertyChanged -= Value_PropertyChanged;
            _registration.Remove(this);

            _head.UnregisterAll();
        }
    }

    partial class INotifyPropertyChangedExtensions
    {
        public static void RegisterAsNestedPropertyChanged<T, TOwner>(this T value, TOwner owner, string propertyName, INestPropertyChangedRegistrationBranch registration, Action<T, PropertyChangedEventArgs> action)
            where T : INotifyPropertyChanged<T>
            where TOwner : INotifyPropertyChanged<TOwner>, INotifyPropertyChanging<TOwner>
        {
            new NestedPropertyRegistrationLeaf<T, TOwner>(value, owner, propertyName, registration, action);
        }

        public static void RegisterAsNestedPropertyChanged<T, TOwner>(this T value, TOwner owner, string propertyName, INestPropertyChangedRegistrationBranch registration, Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> action)
            where T : INotifyPropertyChanged<T>, INotifyPropertyChanging<T>
            where TOwner : INotifyPropertyChanged<TOwner>, INotifyPropertyChanging<TOwner>
        {
            new NestedPropertyRegistrationBranch<T, TOwner>(value, owner, propertyName, registration, action);
        }

        public static IPropertyChangedRegistration RegisterPropertyChanged<T>(this T self, Action<T, PropertyChangedEventArgs, INestPropertyChangedRegistrationBranch> action) where T : INotifyPropertyChanged<T>, INotifyPropertyChanging<T>
        {
            var nr = new NestedPropertyRegistrationRoot<T>(self, action);
            return nr;
        }
    }
}

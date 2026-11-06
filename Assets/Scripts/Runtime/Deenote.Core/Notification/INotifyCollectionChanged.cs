using Deenote.CoreB.Notification;
using ObservableCollections;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Deenote.CoreB.Notification
{
    public readonly ref struct CollectionChangedEventArgs<T>
    {
        public readonly NotifyCollectionChangedAction Action;
        public readonly ReadOnlySpan<T> OldItems;
        public readonly ReadOnlySpan<T> NewItems;
        public readonly int OldIndex;
        public readonly int NewIndex;

        internal CollectionChangedEventArgs(NotifyCollectionChangedAction action, ReadOnlySpan<T> oldItems, ReadOnlySpan<T> newItems, int oldIndex, int newIndex)
        {
            Action = action;
            OldItems = oldItems;
            NewItems = newItems;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }

        internal static CollectionChangedEventArgs<T> From(in NotifyCollectionChangedEventArgs<T> e)
        {
            if (e.Action is NotifyCollectionChangedAction.Reset)
                return new CollectionChangedEventArgs<T>(e.Action, ReadOnlySpan<T>.Empty, ReadOnlySpan<T>.Empty, 0, 0);

            ReadOnlySpan<T> oldItems, newItems;
            if (e.IsSingleItem) {
                oldItems = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in e.OldItem), 1);
                newItems = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in e.NewItem), 1);
            }
            else {
                oldItems = e.OldItems;
                newItems = e.NewItems;
            }
            return new CollectionChangedEventArgs<T>(e.Action, oldItems, newItems, e.OldStartingIndex, e.NewStartingIndex);
        }
    }

    public delegate void CollectionChangedEventHandler<T>(CollectionChangedEventArgs<T> e);
    public delegate void CollectionChangedNestedEventHandler<T>(CollectionChangedEventArgs<T> e, ObservableBindingToken t);

    public readonly struct CollectionChangedRegistration<T> : IPropertyChangedRegistration
    {
        internal readonly IObservableCollection<T> _self;
        internal readonly NotifyCollectionChangedEventHandler<T> _action;
        internal CollectionChangedRegistration(IObservableCollection<T> self, NotifyCollectionChangedEventHandler<T> action)
        {
            _self = self;
            _action = action;
        }

        public void Unregister() => _self.CollectionChanged -= _action;
    }

    partial class InpcRegistration
    {
        public static CollectionChangedRegistration<T> RegisterCollectionChanged<T>(this IObservableCollection<T> collection, CollectionChangedEventHandler<T> action)
        {
            NotifyCollectionChangedEventHandler<T> reg = action._ConvertHandler;
            collection.CollectionChanged += reg;
            return new CollectionChangedRegistration<T>(collection, reg);
        }

        public static CollectionChangedRegistration<T> RegisterCollectionChangedAndInvoke<T>(this IObservableCollection<T> collection, CollectionChangedEventHandler<T> action)
        {
            NotifyCollectionChangedEventHandler<T> reg = action._ConvertHandler;
            collection.CollectionChanged += reg;
            action._ConvertHandler(NotifyCollectionChangedEventArgs<T>.Reset());
            return new CollectionChangedRegistration<T>(collection, reg);
        }

        private static void _ConvertHandler<T>(this CollectionChangedEventHandler<T> handler, in NotifyCollectionChangedEventArgs<T> e)
        {
            handler(CollectionChangedEventArgs<T>.From(e));
        }
    }
}

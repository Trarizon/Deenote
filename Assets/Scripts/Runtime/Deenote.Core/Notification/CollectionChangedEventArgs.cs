using System;
using System.Runtime.InteropServices;

namespace Deenote.CoreB.Notification
{
    public enum CollectionChangedAction
    {
        Reset, // No old items
        Replace, // OldItems are removed, NewItems are added

        // No items removed or added, first bit is 1
        Move = int.MinValue,
        Sort,
    }

    public readonly ref struct CollectionChangedEventArgs<T>
    {
        public readonly CollectionChangedAction Action { get; }
        public readonly ReadOnlySpan<T> OldItems { get; }
        public readonly ReadOnlySpan<T> NewItems { get; }
        public readonly int OldIndex { get; }
        public readonly int NewIndex { get; }

        public ReadOnlySpan<T> AddedItems => (Action & (CollectionChangedAction)int.MinValue) != 0 ? ReadOnlySpan<T>.Empty : NewItems;
        public ReadOnlySpan<T> RemovedItems => (Action & (CollectionChangedAction)int.MinValue) != 0 ? ReadOnlySpan<T>.Empty : OldItems;

        private CollectionChangedEventArgs(CollectionChangedAction action, int oldIndex, int newIndex, ReadOnlySpan<T> oldItems, ReadOnlySpan<T> newItems)
        {
            Action = action;
            OldIndex = oldIndex;
            NewIndex = newIndex;
            OldItems = oldItems;
            NewItems = newItems;
        }

        public static CollectionChangedEventArgs<T> Replace(int index, ReadOnlySpan<T> oldItems, ReadOnlySpan<T> newItems)
            => new(CollectionChangedAction.Replace, index, index, oldItems, newItems);

        public static CollectionChangedEventArgs<T> Replace(int index, T oldItem, T newItem)
            => Replace(index, MemoryMarshal.CreateReadOnlySpan(ref oldItem, 1), MemoryMarshal.CreateReadOnlySpan(ref newItem, 1));

        public static CollectionChangedEventArgs<T> Add(int index, ReadOnlySpan<T> items)
            => new(CollectionChangedAction.Replace, index, index, ReadOnlySpan<T>.Empty, items);

        public static CollectionChangedEventArgs<T> Add(int index, T item)
            => Add(index, MemoryMarshal.CreateReadOnlySpan(ref item!, 1));

        public static CollectionChangedEventArgs<T> Remove(int index, ReadOnlySpan<T> items)
            => new(CollectionChangedAction.Replace, index, index, items, ReadOnlySpan<T>.Empty);

        public static CollectionChangedEventArgs<T> Remove(int index, T item)
            => Remove(index, MemoryMarshal.CreateReadOnlySpan(ref item!, 1));

        public static CollectionChangedEventArgs<T> Move(int oldIndex, int newIndex, ReadOnlySpan<T> items)
            => new(CollectionChangedAction.Move, oldIndex, newIndex, items, items);

        public static CollectionChangedEventArgs<T> Reset(ReadOnlySpan<T> items)
            => new(CollectionChangedAction.Reset, 0, 0, ReadOnlySpan<T>.Empty, items);

        public static CollectionChangedEventArgs<T> Reset(INotifyCollectionChangedResettable<T> collection)
            => collection.GetResetEventArgs();

        public static CollectionChangedEventArgs<T> Sort(int index, ReadOnlySpan<T> items)
            => new(CollectionChangedAction.Sort, index, index, items, items);
    }
}
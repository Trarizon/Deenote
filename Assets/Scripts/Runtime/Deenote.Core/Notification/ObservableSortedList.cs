#nullable enable

using System;

namespace Deenote.CoreB.Notification
{
    public sealed class ObservableSortedList<T> : INotifyCollectionChanged<ObservableSortedList<T>, T>
    {
        public event CollectionChangeEventHandler<ObservableSortedList<T>, T>? CollectionChanged;

        public void Add(ReadOnlySpan<T> notes)
        {
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs.Add(notes));
        }

        public void Remove(ReadOnlySpan<T> notes)
        {
            CollectionChanged?.Invoke(this, CollectionChangedEventArgs.Remove(notes));
        }
    }
}

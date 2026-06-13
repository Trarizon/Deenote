#nullable enable

using System;

namespace Deenote.CoreB.Notification
{
    public interface INotifyCollectionChanged<TSelf, TItem>
    {
        event CollectionChangeEventHandler<TSelf, TItem>? CollectionChanged;
    }

    public interface ICollectionChangedEvent<TSelf, TItem>
    {
        event CollectionChangeEventHandler<TSelf, TItem>? Event;
    }

    public sealed class CollectionChangedEvent<TSelf, TItem> : ICollectionChangedEvent<TSelf, TItem>
    {
        public event CollectionChangeEventHandler<TSelf, TItem>? Event;

        public void Invoke(TSelf sender, CollectionChangedEventArgs<TItem> e)
        {
            Event?.Invoke(sender, e);
        }
    }

    public delegate void CollectionChangeEventHandler<TSender, TItem>(TSender sender, CollectionChangedEventArgs<TItem> e);

    public enum CollectionChangeAction
    {
        Refresh, Add, Remove, PropertyChanged
    }

    public readonly ref struct CollectionChangedEventArgs<T>
    {
        public static CollectionChangedEventArgs<T> RefreshAll => default;

        public ReadOnlySpan<T> Notes { get; }
        public CollectionChangeAction Action { get; }
        public PropertyEventArgs PropertyChangedArgs { get; }

        internal CollectionChangedEventArgs(ReadOnlySpan<T> notes, CollectionChangeAction action, PropertyEventArgs propertyChangedArgs)
        {
            Notes = notes;
            Action = action;
            PropertyChangedArgs = propertyChangedArgs;
        }
    }

    public static class CollectionChangedEventArgs
    {
        public static CollectionChangedEventArgs<T> RefreshAll<T>()
            => CollectionChangedEventArgs<T>.RefreshAll;
            
        public static CollectionChangedEventArgs<T> Add<T>(ReadOnlySpan<T> added)
            => new CollectionChangedEventArgs<T>(added, CollectionChangeAction.Add, PropertyEventArgs.AllProperties);

        public static CollectionChangedEventArgs<T> Remove<T>(ReadOnlySpan<T> removed)
            => new CollectionChangedEventArgs<T>(removed, CollectionChangeAction.Remove, PropertyEventArgs.AllProperties);

        public static CollectionChangedEventArgs<T> PropertyChanged<T>(ReadOnlySpan<T> notes, PropertyEventArgs propertyChangedArgs)
            => new CollectionChangedEventArgs<T>(notes, CollectionChangeAction.PropertyChanged, propertyChangedArgs);
    }
}

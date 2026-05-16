#nullable enable

using System;

namespace Deenote.CoreB.Notification
{
    public delegate void CollectionChangeEventHandler<TSender, TItem>(TSender sender, CollectionChangedEventArgs<TItem> e);

    public enum CollectionChangeAction
    {
        Add, Remove, PropertyChanged
    }

    public readonly ref struct CollectionChangedEventArgs<T>
    {
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
        public static CollectionChangedEventArgs<T> Add<T>(ReadOnlySpan<T> added)
            => new CollectionChangedEventArgs<T>(added, CollectionChangeAction.Add, default);

        public static CollectionChangedEventArgs<T> Remove<T>(ReadOnlySpan<T> removed)
            => new CollectionChangedEventArgs<T>(removed, CollectionChangeAction.Remove, default);

        public static CollectionChangedEventArgs<T> PropertyChanged<T>(ReadOnlySpan<T> notes, PropertyEventArgs propertyChangedArgs)
            => new CollectionChangedEventArgs<T>(notes, CollectionChangeAction.PropertyChanged, propertyChangedArgs);
    }
}

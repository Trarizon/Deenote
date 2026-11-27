namespace Deenote.CoreB.Notification
{
    public interface INotifyCollectionChangedResettable<T>
    {
        CollectionChangedEventArgs<T> GetResetEventArgs();
    }

    public interface INotifyCollectionChanged<T>
    {
        event CollectionChangedEventHandler<INotifyCollectionChanged<T>, T>? CollectionChanged;
    }

    public interface INotifyCollectionChanged<TCollection, T> : INotifyCollectionChanged<T>
        where TCollection : class, INotifyCollectionChanged<TCollection, T>
    {
        new event CollectionChangedEventHandler<TCollection, T>? CollectionChanged;
        event CollectionChangedEventHandler<INotifyCollectionChanged<T>, T>? INotifyCollectionChanged<T>.CollectionChanged
        {
            add => CollectionChanged += value;
            remove => CollectionChanged -= value;
        }
    }

    public interface INotifyCollectionChangedResettable<TCollection, T> : INotifyCollectionChanged<TCollection, T>, INotifyCollectionChangedResettable<T>
        where TCollection : class, INotifyCollectionChanged<TCollection, T>
    { }

    public delegate void CollectionChangedEventHandler<in TCollection, T>(TCollection sender, CollectionChangedEventArgs<T> e);
    public delegate void CollectionChangedNestedEventHandler<in TCollection, T>(TCollection sender, CollectionChangedEventArgs<T> e, ObservableBindingToken t);

    partial class InpcRegistration
    {
        public static CollectionChangedRegistration<TCollection, T> RegisterCollectionChanged<TCollection, T>(this INotifyCollectionChanged<TCollection, T> collection, CollectionChangedEventHandler<INotifyCollectionChanged<T>, T> action)
            where TCollection : class, INotifyCollectionChanged<TCollection, T>
        {
            collection.CollectionChanged += action;
            return new CollectionChangedRegistration<TCollection, T>(collection, action);
        }
    }
}

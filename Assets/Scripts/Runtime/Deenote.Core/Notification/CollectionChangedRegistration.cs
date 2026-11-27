namespace Deenote.CoreB.Notification
{
    public readonly struct CollectionChangedRegistration<TCollection, T> : IPropertyChangedRegistration
        where TCollection : class, INotifyCollectionChanged<TCollection, T>
    {
        internal readonly INotifyCollectionChanged<TCollection, T> _collection;
        internal readonly CollectionChangedEventHandler<TCollection, T> _action;
        internal CollectionChangedRegistration(INotifyCollectionChanged<TCollection, T> collection, CollectionChangedEventHandler<TCollection, T> action)
        {
            _collection = collection;
            _action = action;
        }

        public void Unregister() => _collection.CollectionChanged -= _action;
    }
}
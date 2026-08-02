using System;

namespace Deenote.Core.Notification
{
    public readonly struct PropertyChangedRegistration<T> where T : INotifyPropertyChanged<T>
    {
        private readonly T _self;
        private readonly Action<T, PropertyEventArgs> _handler;

        internal PropertyChangedRegistration(T self, Action<T, PropertyEventArgs> handler)
        {
            _self = self;
            _handler = handler;
        }
    }

    public readonly struct PropertyChangingRegistration<T> where T : INotifyPropertyChanging<T>
    {
        private readonly T _self;
        private readonly Action<T, PropertyEventArgs> _handler;

        internal PropertyChangingRegistration(T self, Action<T, PropertyEventArgs> handler)
        {
            _self = self;
            _handler = handler;
        }
    }
}
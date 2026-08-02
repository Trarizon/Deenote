using System;

namespace Deenote.Core.Notification
{
    public interface INotifyPropertyChanged<T> where T : INotifyPropertyChanged<T>
    {
        event Action<T, PropertyEventArgs>? PropertyChanged;
    }
    
    public interface INotifyPropertyChanging<T> where T : INotifyPropertyChanging<T>
    {
        event Action<T, PropertyEventArgs>? PropertyChanging;
    }

    public static class PropertyChangeNotificationHelpers
    {
        public static PropertyChangedRegistration<T> RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyEventArgs> action)
            where T : INotifyPropertyChanged<T>
        {
            self.PropertyChanged += action;
            action(self, PropertyEventArgs.All);
            return new PropertyChangedRegistration<T>(self, action);
        }

        public static PropertyChangingRegistration<T> RegisterPropertyChanging<T>(this T self, Action<T, PropertyEventArgs> action)
            where T : INotifyPropertyChanging<T>
        {
            self.PropertyChanging += action;
            return new PropertyChangingRegistration<T>(self, action);
        }
    }
}

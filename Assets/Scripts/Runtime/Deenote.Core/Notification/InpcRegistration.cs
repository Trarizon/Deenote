using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Threading;
using UnityEngine;

namespace Deenote.CoreB.Notification
{
    public interface IPropertyChangedRegistration
    {
        void Unregister();
    }

    partial class InpcRegistration
    {
        sealed class Tpl<T1, T2> { public T1 Item1 = default!; public T2 Item2 = default!; }

        public static T UnregisterWhenGameObjectDestroyed<T>(this T registration, GameObject gameObject)
            where T : IPropertyChangedRegistration
        {
            var tuple = new Tpl<T, CancellationTokenRegistration>();
            tuple.Item1 = registration;
            tuple.Item2 = gameObject.GetAsyncDestroyTrigger().CancellationToken.RegisterWithoutCaptureExecutionContext((state) =>
            {
                var tuple = (Tpl<T, CancellationTokenRegistration>)state;
                tuple.Item1.Unregister();
                tuple.Item2.Dispose();
            }, tuple);

            return registration;
        }

        public static T UnregisterWhenGameObjectDestroyed<T>(this T registration, Component component)
            where T : IPropertyChangedRegistration
            => registration.UnregisterWhenGameObjectDestroyed(component.gameObject);

        public static IPropertyChangedRegistration RegisterPropertyChanged<T>(this T self, Action<T, PropertyChangedEventArgs, ObservableBindingToken> action)
            where T : class, INotifyPropertyChanged<T>
        {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
            var binding = new ObservableBinding<T>();
            binding.Update(self, action);
            return binding;
#pragma warning restore CS8631
        }

        public static IPropertyChangedRegistration RegisterPropertyChangedAndInvoke<T>(this T self, Action<T, PropertyChangedEventArgs, ObservableBindingToken> action)
            where T : class, INotifyPropertyChanged<T>
        {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
            var binding = new ObservableBinding<T>();
            binding.Update(self, action);
            action.Invoke(self, PropertyChangedEventArgs.All, new(binding));
            return binding;
#pragma warning restore CS8631
        }

        public static void AsNestedRegisterPropertyChangedAndInvoke<T>(this T self, string propertyName, PropertyChangedEventArgs e, ObservableBindingToken token, Action<T, PropertyChangedEventArgs, ObservableBindingToken> action)
            where T : INotifyPropertyChanged<T?>?
        {
            if (e.Match(propertyName)) {
                token.Binding.AddOrUpdate(propertyName, self, action);
                action.Invoke(self, PropertyChangedEventArgs.All, token);
            }
        }

        public static void AsNestedRegisterCollectionChangedAndInvoke<TCollection, TElement>(this INotifyCollectionChangedResettable<TCollection, TElement> collection, string propertyName, PropertyChangedEventArgs e, ObservableBindingToken token, CollectionChangedNestedEventHandler<TCollection, TElement> action)
            where TCollection : class, INotifyCollectionChanged<TCollection, TElement>
        {
            if (e.Match(propertyName)) {
                token.Binding.AddOrUpdate(propertyName, collection, action);
                action.Invoke((TCollection)collection, CollectionChangedEventArgs<TElement>.Reset(collection), token);
            }
        }
    }
}

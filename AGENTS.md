# AGENTS.md

## INotifyPropertyChanged

This project use `Deenote.Core.Notification.INotifyPropertyChanged<T>` in replace of `System.ComponentModel.INotifyPropertyChanged`. The source code is in `Assets/Scripts/Runtime/Deenote/Core/Notification/INotifyPropertyChanged.cs`.

Use `PropertyChangedEventArgs.Match` or `MatchAny` method to check if the property name matches the expected name. Do not use operator `==` to compare the property name.

``` csharp
if (e.Match(nameof(s.Name))) { ... } // YES
if (e.PropertyName == nameof(s.Name)) { ... } // NO
```

Use methods in `INotifyPropertyChanged.cs` if possible, avoid using `PropertyChanged` event directly.

``` csharp
src.RegisterPropertyChanged((s, e) => ...);
src.RegisterPropertyChangedAndInvoke((s, e) => ...);
```

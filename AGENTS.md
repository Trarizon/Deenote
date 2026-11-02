# AGENTS.md

## INotifyPropertyChanged

This project use `Deenote.Core.Notification.INotifyPropertyChanged<T>` in replace of `System.ComponentModel.INotifyPropertyChanged`. The source code is in directory: `Assets/Scripts/Runtime/Deenote/Core/Notification/`.

A simple example of how to register a property change handler:

``` csharp
inpc.RegisterPropertyChanged((s, e) =>
{
    if (e.Match(nameof(s.Name)))
    {
        txtName.text = s.Name;
    }
    if (e.MatchAny(nameof(s.Gender), nameof(s.Age)))
    {
        txtAge.text = $"{s.Gender} {s.Age}";
    }
}).UnregisterWhenGameObjectDestroyed(this);
```

- Avoid using `e.PropertyName == "xxx"`, use `e.Match(nameof(s.xxx))` and `e.MatchAny(nameof(s.xxx), nameof(s.yyy))` instead.
- Avoid using `PropertyChanged` directly to register handlers, use `RegisterPropertyChanged` method instead.
- Use `RegisterPropertyChangedAndInvoke` method to register handlers that will be invoked immediately after the registration.

### Nested Properties

For nested properties like `ViewModel.Model.Name` while `ViewModel` and `Model` are both `INPC<T>`, use the following syntax:

``` csharp
viewModel.RegisterPropertyChanged((s, e, h) =>
{
    if (e.Match(nameof(s.Model)))
    {
        s.Model.AsNestedRegisterPropertyChanged(nameof(s.Model), h, (s, e, h) =>
        {
            if (e.Match(nameof(s.Name)))
            {
                txtName.text = s.Name;
            }
        });
    }
}).UnregisterWhenGameObjectDestroyed(this);
```

Handler will be invoked when `ViewModel.Model` or `Model.Name` changes.

- The argument `h` is registration handle auto created and use by the framework, caller should just pass it to the next level of registration.
  - `AsNestedRegisterPropertyChanged` receives a `h` and the handler requires a `h`, they are not the same argument.
  - You should only pass the `h` from the previous level of registration.
- Use `AsNestedRegisterPropertyChangedAndInvoke` method to register handlers that will be invoked immediately after the registration.

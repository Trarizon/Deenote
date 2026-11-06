# Property Changed Notification

## INotifyPropertyChanged<T>

项目使用自定义的`INotifyPropertyChanged<T>`代替`System.ComponentModel.INotifyPropertyChanged`。

### Publisher

对于事件发布方，其使用方式类似于`System.ComponentModel.INotifyPropertyChanged`。

```csharp
public class MyObject : INotifyPropertyChanged<MyObject>
{
    public event Action<MyObject, PropertyEventChangedArgs>? PropertyChanged;

    public string Name
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                PropertyChanged?.Invoke(this, nameof(Name));
            }
        }
    }
}
```

上述代码中使用了`PropertyChanged?.Invoke(this, nameof(Name))`触发，这个是代码提供了用于`Action<T, PropertyEventChangedArgs>`的用于简化调用的扩展方法。也可以使用`PropertyChanged?.Invoke(this, nameof(Name), nameof(Desc))`。进行实现多个属性的通知。

### Subscriber

对于基础的单层属性订阅，使用形如下述代码的方式订阅：

``` csharp
obj.RegisterPropertyChangedAndInvoke((s, e) =>
{
    if (e.Match(nameof(Name)))
    {
        // Name属性发生变化时的处理
    }
    if (e.Match(nameof(Desc)))
    {
        // Desc属性发生变化时的处理
    }
    if (e.MatchAny(nameof(Name), nameof(Desc)))
    {
        // Name或Desc属性发生变化时的处理
    }
}).UnregisterWhenGameObjectDestroy(this);
```

- 使用`RegisterPropertyChangedAndInvoke(action)`代替`PropertyChanged += action`事件
  - `RegisterPropertyChanged(action)`是其不会立刻调用一次的版本，按需使用
- 使用`e.Match(name)`、`e.MatchAny(...names)`方法代替`e.PropertyName == name`判断是否是该属性发生变化
- *可以*使用`UnregisterWhenGameObjectDestroy(this)`实现在GameObject销毁时自动取消订阅

对于嵌套属性订阅，使用如下方式

``` csharp
obj.RegisterPropertyChangedAndInvoke((s, e, t) =>
{
    if (e.Match(nameof(Name)))
    {
        // Name属性发生变化时的处理
    }
    // ... 其他单层属性

    s.Child.AsNestedRegisterPropertyChangedAndInvoke(nameof(Child), e, t, (s, e, t) =>
    {
        if (e.Match(nameof(s.Name2)))
        {
            // Child.Name2属性发生变化时的处理
        }

        s.GrandChild.AsNestedRegisterPropertyChangedAndInvoke(nameof(GrandChild), e, t, (s, e, t) =>
        {
            if (e.Match(nameof(s.Name3)))
            {
                // Child.GrandChild.Name3属性发生变化时的处理
            }
        })
    })

    s.Collection.AsNestedRegisterCollectionChangedAndInvoke(nameof(Collection), e, t, (e, t) =>
    {
        // 处理集合变化
    })
}).UnregisterWhenGameObjectDestroy(this);
```

- 直接在方法中在属性上调用`AsNestedRegisterProperty/CollectionChangedAndInvoke`方法。
- `AsNestedRegisterProperty/CollectionChangedAndInvoke`不需要取消订阅，其会在属性发生变化与外部`obj`取消订阅时自动处理
- `AsNestedRegisterProperty/CollectionChangedAndInvoke`的参数需要传入外一层的`e`、`t`参数，不要在内部的方法中使用外部的`e`、`t`参数，目前不确定会发生什么。推荐每一层的lambda使用同样的命名规则，以防误用

## IObservableCollection<T>

项目使用`ObservableCollections`库提供的`IObservableCollection<T>`作为observable collection的接口，使用下述方式订阅：

``` csharp
obj.Collection.RegisterCollectionChanged(e =>
{
    // 处理集合变化
}).UnregisterWhenGameObjectDestroy(this);
```

注：此处的`e`不是原库中的`NotifyCollectionChangedEventArgs<T>`，而是自定义的`CollectionChangedEventArgs<T>`。

集合作为嵌套属性的订阅方式见上文嵌套属性的订阅。

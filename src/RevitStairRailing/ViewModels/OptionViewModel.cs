namespace RevitStairRailing.ViewModels;

/// <summary>
/// Вариант выпадающего списка: значение и его название из локализации.
/// </summary>
internal class OptionViewModel<T> {
    public OptionViewModel(T value, string name) {
        Value = value;
        Name = name;
    }

    public T Value { get; }

    public string Name { get; }
}

using System.Windows;

namespace RevitListOfSchedules.Proxy;

public class BindingProxy : Freezable {
    // Реализация Freezable (требуется для работы в XAML)
    protected override Freezable CreateInstanceCore() {
        return new BindingProxy();
    }

    // Свойство, через которое будет передаваться DataContext
    public object Data {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    // DependencyProperty для Data
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));
}

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace RevitMarkPlacement.Controls;

[ContentProperty(nameof(ItemsSource))]
public partial class GroupControl {
    public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
        nameof(HeaderText),
        typeof(string),
        typeof(GroupControl),
        new PropertyMetadata(default(string)));
    
    public GroupControl() {
        InitializeComponent();
    }

    public string HeaderText {
        get => (string) GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public ItemCollection ItemsSource {
        get => _itemsControl.Items;
    }
}

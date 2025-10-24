using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RevitCreateViewSheet.Resources;
internal class ComboBoxBehavior {
    public static readonly DependencyProperty OpenDropDownOnTextChangedProperty =
          DependencyProperty.RegisterAttached(
              "OpenDropDownOnTextChanged",
              typeof(bool),
              typeof(ComboBoxBehavior),
              new PropertyMetadata(false, OnOpenDropDownOnTextChangedChanged));

    public static bool GetOpenDropDownOnTextChanged(DependencyObject obj) {
        return (bool) obj.GetValue(OpenDropDownOnTextChangedProperty);
    }

    public static void SetOpenDropDownOnTextChanged(DependencyObject obj, bool value) {
        obj.SetValue(OpenDropDownOnTextChangedProperty, value);
    }

    private static void OnOpenDropDownOnTextChangedChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        if(d is ComboBox comboBox) {
            if((bool) e.NewValue) {
                comboBox.AddHandler(
                    TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(OnTextChanged));
            } else {
                comboBox.RemoveHandler(
                    TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(OnTextChanged));
            }
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e) {
        if(sender is ComboBox comboBox && comboBox.IsEditable) {
            if(!string.IsNullOrEmpty(comboBox.Text)) {
                comboBox.IsDropDownOpen = true;
            }
        }
    }
}

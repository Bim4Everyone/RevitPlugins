using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Wpf.Ui.Controls;

namespace RevitBatchPrint.Views;

public partial class SheetsView {
    public SheetsView() {
        InitializeComponent();
    }

    private void _albumParamName_OnLoaded(object sender, RoutedEventArgs e) {
        _albumParamName.SetCurrentValue(ItemsControl.ItemsSourceProperty, _albumParamName.OriginalItemsSource);
    }

    private void _albumParamName_OnGotFocus(object sender, RoutedEventArgs e) {
        if(!_albumParamName.IsSuggestionListOpen) {
            _albumParamName.IsSuggestionListOpen = true;
        }
    }
}


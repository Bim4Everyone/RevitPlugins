using System.Windows;
using System.Windows.Controls;

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
        //_albumParamName.IsSuggestionListOpen = true;
    }

    private void _albumParamName_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
        if(!string.IsNullOrEmpty(_albumParamName.Text)) {
            return;
        }

        _albumParamName.IsSuggestionListOpen = true;
        _albumParamName.SetCurrentValue(ItemsControl.ItemsSourceProperty, _albumParamName.OriginalItemsSource);
    }
}


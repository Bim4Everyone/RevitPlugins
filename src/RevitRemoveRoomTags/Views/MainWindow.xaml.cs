using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using RevitRemoveRoomTags.ViewModels;

namespace RevitRemoveRoomTags.Views;
public partial class MainWindow {

    public MainWindow() {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitRemoveRoomTags);
    public override string ProjectConfigName => nameof(MainWindow);

    public ObservableCollection<ValidationError> ValidationErrors { get; set; } = [];

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }


    private void Window_Error(object sender, ValidationErrorEventArgs e) {
        if(e.Action == ValidationErrorEventAction.Added) {
            ValidationErrors.Add(e.Error);
        } else {
            ValidationErrors.Remove(e.Error);
        }

        MainViewModel mainViewModel = DataContext as MainViewModel;
        if(mainViewModel != null) {
            if(ValidationErrors.Count > 0) {
                mainViewModel.ErrorTextFromGUI = ValidationErrors[0].ErrorContent.ToString();
            } else {
                mainViewModel.ErrorTextFromGUI = "";
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using dosymep.SimpleServices;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace RevitLintelsManager.Services;

internal class SettingsDialogService(IContentDialogService contentDialogService,
    ILocalizationService localizationService) {
    public async Task<string> RequestNameAsync(string title, string initialName, Func<string, bool> validate) {
        var textBox = new Wpf.Ui.Controls.TextBox { Text = initialName, MinWidth = 360, MaxLength = 150 };
        var error = new System.Windows.Controls.TextBlock {
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = Brushes.Orange,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480
        };
        var panel = new StackPanel();
        panel.Children.Add(new System.Windows.Controls.TextBlock {
            Text = localizationService.GetLocalizedString("Dialog.Name"),
            Margin = new Thickness(0, 0, 0, 8)
        });
        panel.Children.Add(textBox);
        panel.Children.Add(error);
        var dialog = new ContentDialog(contentDialogService.GetDialogHost()) {
            Title = title,
            Content = panel,
            PrimaryButtonText = localizationService.GetLocalizedString("Common.Ok"),
            CloseButtonText = localizationService.GetLocalizedString("MainWindow.ButtonCancel"),
            DefaultButton = ContentDialogButton.Primary
        };
        void Validate() {
            bool valid = validate(textBox.Text.Trim());
            dialog.IsPrimaryButtonEnabled = valid;
            error.Text = valid ? null : localizationService.GetLocalizedString("Dialog.InvalidName");
        }
        textBox.TextChanged += (_, _) => Validate();
        textBox.Loaded += (_, _) => {
            textBox.Focus();
            textBox.SelectAll();
        };
        Validate();
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? textBox.Text.Trim() : null;
    }
}

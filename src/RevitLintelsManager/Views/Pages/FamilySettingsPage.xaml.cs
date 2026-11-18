using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Views.Pages;

internal partial class FamilySettingsPage {
    public FamilySettingsPage(SettingsViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.FamilySettingsViewModel;
    }

    private void OnLintelFamiliesLoaded(object sender, RoutedEventArgs e) {
        if(sender is not ListBox list) {
            return;
        }
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () => {
            if(!list.IsLoaded || !list.IsVisible
                || DataContext is not FamilySettingsViewModel viewModel) {
                return;
            }
            var family = viewModel.SelectedLintelFamilyViewModel;
            if(family is null || !list.Items.Contains(family)) {
                return;
            }
            list.ScrollIntoView(family);
            list.UpdateLayout();
            if(list.ItemContainerGenerator.ContainerFromItem(family) is ListBoxItem item) {
                item.BringIntoView();
                FindRadioButton(item)?.Focus();
            }
        });
    }

    private static RadioButton FindRadioButton(DependencyObject parent) {
        for(int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++) {
            var child = VisualTreeHelper.GetChild(parent, index);
            if(child is RadioButton radioButton) {
                return radioButton;
            }
            var nestedRadioButton = FindRadioButton(child);
            if(nestedRadioButton is not null) {
                return nestedRadioButton;
            }
        }
        return null;
    }
}

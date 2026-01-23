using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Views;

/// <summary>
/// Класс главного окна плагина.
/// </summary>
public partial class MainWindow {
    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
    public MainWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
    }

    /// <summary>
    /// Наименование плагина.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string PluginName => nameof(RevitUnmodelingMep);
    
    /// <summary>
    /// Наименование файла конфигурации.
    /// </summary>
    /// <remarks>
    /// Используется для сохранения положения окна.
    /// </remarks>
    public override string ProjectConfigName => nameof(MainWindow);
    
    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void ContentTabs_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if(e.Source is TabControl && DataContext is MainViewModel viewModel) {
            viewModel.RefreshAssignmentsFromConsumableTypes();
        }
    }

    private void FormulaTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.BeginFormulaEdit(item);
        }
    }

    private void FormulaTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.EndFormulaEdit(item);
        }
    }

    private void FullNameTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.BeginNameEdit(item);
        }
    }

    private void FullNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.EndNameEdit(item);
        }
    }

    private void FullNameLabel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is not FrameworkElement element || element.DataContext is not ConsumableTypeItem item) {
            return;
        }

        viewModel.BeginNameEdit(item);

        element.Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() => element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next))));
    }

    private void NoteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.BeginNoteEdit(item);
        }
    }

    private void NoteTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            viewModel.EndNoteEdit(item);
        }
    }
}

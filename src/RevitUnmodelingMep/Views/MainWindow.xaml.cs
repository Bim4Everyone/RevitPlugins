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
    private enum HintTarget {
        None,
        Formula,
        FullName,
        Note
    }

    private TextBox _activeTextBox;
    private HintTarget _activeHintTarget = HintTarget.None;
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
            _activeTextBox = sender as TextBox;
            _activeHintTarget = HintTarget.Formula;
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
            _activeTextBox = sender as TextBox;
            _activeHintTarget = HintTarget.FullName;
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
        _activeHintTarget = HintTarget.FullName;

        element.Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() => element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next))));
    }

    private void NoteTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        if(DataContext is not MainViewModel viewModel) {
            return;
        }

        if(sender is FrameworkElement element && element.DataContext is ConsumableTypeItem item) {
            _activeTextBox = sender as TextBox;
            _activeHintTarget = HintTarget.Note;
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

    private void HintItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(sender is not FrameworkElement element) {
            return;
        }

        string text = element.DataContext as string;
        if(string.IsNullOrWhiteSpace(text)) {
            return;
        }

        string insertText = text;
        if(DataContext is MainViewModel viewModel
           && (viewModel.IsNameEditing || viewModel.IsNoteEditing || _activeHintTarget is HintTarget.FullName or HintTarget.Note)) {
            insertText = "{" + text + "}";
        }

        TextBox targetTextBox = _activeTextBox ?? Keyboard.FocusedElement as TextBox;
        if(targetTextBox == null || targetTextBox.IsReadOnly || !targetTextBox.IsEnabled) {
            return;
        }

        if(targetTextBox.SelectionLength > 0) {
            targetTextBox.SelectedText = insertText;
        } else {
            int caretIndex = targetTextBox.CaretIndex;
            string current = targetTextBox.Text ?? string.Empty;
            if(caretIndex < 0) {
                caretIndex = 0;
            }
            if(caretIndex > current.Length) {
                caretIndex = current.Length;
            }

            targetTextBox.Text = current.Insert(caretIndex, insertText);
            targetTextBox.CaretIndex = caretIndex + insertText.Length;
        }

        targetTextBox.Focus();
        e.Handled = true;
    }
}

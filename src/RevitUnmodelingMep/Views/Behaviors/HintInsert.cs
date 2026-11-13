using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevitUnmodelingMep.Views.Behaviors;

internal enum HintInsertTarget {
    None,
    Formula,
    FullName,
    Note
}

internal static class HintInsert {
    private static TextBox _activeTextBox;

    public static readonly DependencyProperty TrackActiveProperty =
        DependencyProperty.RegisterAttached(
            "TrackActive",
            typeof(bool),
            typeof(HintInsert),
            new PropertyMetadata(false, OnTrackActiveChanged));

    public static void SetTrackActive(DependencyObject element, bool value) => element.SetValue(TrackActiveProperty, value);
    public static bool GetTrackActive(DependencyObject element) => (bool) element.GetValue(TrackActiveProperty);

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.RegisterAttached(
            "Target",
            typeof(HintInsertTarget),
            typeof(HintInsert),
            new PropertyMetadata(HintInsertTarget.None));

    public static void SetTarget(DependencyObject element, HintInsertTarget value) => element.SetValue(TargetProperty, value);
    public static HintInsertTarget GetTarget(DependencyObject element) => (HintInsertTarget) element.GetValue(TargetProperty);

    public static readonly DependencyProperty InsertOnClickProperty =
        DependencyProperty.RegisterAttached(
            "InsertOnClick",
            typeof(bool),
            typeof(HintInsert),
            new PropertyMetadata(false, OnInsertOnClickChanged));

    public static void SetInsertOnClick(DependencyObject element, bool value) => element.SetValue(InsertOnClickProperty, value);
    public static bool GetInsertOnClick(DependencyObject element) => (bool) element.GetValue(InsertOnClickProperty);

    public static readonly DependencyProperty FocusNextOnClickProperty =
        DependencyProperty.RegisterAttached(
            "FocusNextOnClick",
            typeof(bool),
            typeof(HintInsert),
            new PropertyMetadata(false, OnFocusNextOnClickChanged));

    public static void SetFocusNextOnClick(DependencyObject element, bool value) => element.SetValue(FocusNextOnClickProperty, value);
    public static bool GetFocusNextOnClick(DependencyObject element) => (bool) element.GetValue(FocusNextOnClickProperty);

    private static void OnTrackActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is not TextBox textBox) {
            return;
        }

        if((bool) e.NewValue) {
            textBox.GotKeyboardFocus += TextBoxOnGotKeyboardFocus;
        } else {
            textBox.GotKeyboardFocus -= TextBoxOnGotKeyboardFocus;
        }
    }

    private static void TextBoxOnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
        _activeTextBox = sender as TextBox;
    }

    private static void OnInsertOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is not TextBlock textBlock) {
            return;
        }

        if((bool) e.NewValue) {
            textBlock.MouseLeftButtonDown += HintTextBlockOnMouseLeftButtonDown;
        } else {
            textBlock.MouseLeftButtonDown -= HintTextBlockOnMouseLeftButtonDown;
        }
    }

    private static void HintTextBlockOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(sender is not FrameworkElement element) {
            return;
        }

        string token = element.DataContext as string;
        if(string.IsNullOrWhiteSpace(token)) {
            return;
        }

        TextBox target = _activeTextBox ?? Keyboard.FocusedElement as TextBox;
        if(target == null || target.IsReadOnly || !target.IsEnabled) {
            return;
        }

        HintInsertTarget targetKind = GetTarget(target);
        string insertText = targetKind is HintInsertTarget.FullName or HintInsertTarget.Note
            ? "{" + token + "}"
            : token;

        InsertAtCaret(target, insertText);
        target.Focus();
        e.Handled = true;
    }

    private static void InsertAtCaret(TextBox target, string insertText) {
        if(target.SelectionLength > 0) {
            target.SelectedText = insertText;
            return;
        }

        int caretIndex = target.CaretIndex;
        string current = target.Text ?? string.Empty;
        if(caretIndex < 0) {
            caretIndex = 0;
        }
        if(caretIndex > current.Length) {
            caretIndex = current.Length;
        }

        target.Text = current.Insert(caretIndex, insertText);
        target.CaretIndex = caretIndex + insertText.Length;
    }

    private static void OnFocusNextOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is not FrameworkElement element) {
            return;
        }

        if((bool) e.NewValue) {
            element.PreviewMouseLeftButtonDown += ElementOnPreviewMouseLeftButtonDown;
        } else {
            element.PreviewMouseLeftButtonDown -= ElementOnPreviewMouseLeftButtonDown;
        }
    }

    private static void ElementOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if(sender is not FrameworkElement element) {
            return;
        }

        element.Dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() => element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next))));
    }
}

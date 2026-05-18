using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPackageDocumentation.Views.Controls;

public partial class ParamItemControl : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate), typeof(ParamItemControl));

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    // Свойство для режима редактирования
    public static readonly DependencyProperty IsInEditModeProperty =
        DependencyProperty.Register(nameof(IsInEditMode), typeof(bool), typeof(ParamItemControl),
            new PropertyMetadata(false, OnIsInEditModeChanged));

    public bool IsInEditMode {
        get => (bool) GetValue(IsInEditModeProperty);
        set => SetValue(IsInEditModeProperty, value);
    }

    private static void OnIsInEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (ParamItemControl) d;
        var isEditing = (bool) e.NewValue;

        control.UpdateEditModeVisuals(isEditing);
    }

    public ParamItemControl() {
        InitializeComponent();
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        // Включаем режим редактирования
        IsInEditMode = true;

        // Фокусируемся на TextBox после включения режима
        Dispatcher.BeginInvoke(new System.Action(() => {
            ParamNameTextBox.Focus();
            ParamNameTextBox.SelectAll();
        }), System.Windows.Threading.DispatcherPriority.Background);

        e.Handled = true;
    }

    private void OnEditModeEnd(object sender, RoutedEventArgs e) {
        // Выходим из режима редактирования при потере фокуса
        IsInEditMode = false;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Enter) {
            // Сохраняем изменения и выходим из режима
            IsInEditMode = false;

            // Принудительно обновляем привязку
            var binding = ParamNameTextBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            e.Handled = true;
        } else if(e.Key == Key.Escape) {
            // Отменяем изменения (перезагружаем исходное значение)
            var binding = ParamNameTextBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateTarget();

            IsInEditMode = false;
            e.Handled = true;
        }
    }

    private void UpdateEditModeVisuals(bool isEditing) {
        if(ParamNameTextBlock == null || ParamNameTextBox == null || DeleteButton == null)
            return;

        if(isEditing) {
            // Режим редактирования
            ParamNameTextBlock.Visibility = Visibility.Collapsed;
            ParamNameTextBox.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
        } else {
            // Режим просмотра
            ParamNameTextBlock.Visibility = Visibility.Visible;
            ParamNameTextBox.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;

            // Принудительно обновляем TextBlock
            ParamNameTextBlock.Text = ParamNameTextBox.Text;
        }
    }

    // Опционально: обрабатываем случай, когда пользователь MouseDouble за контролом
    protected override void OnLostFocus(RoutedEventArgs e) {
        base.OnLostFocus(e);

        // Проверяем, не перешел ли фокус на дочерние элементы
        if(!IsKeyboardFocusWithin && IsInEditMode) {
            IsInEditMode = false;
        }
    }
}

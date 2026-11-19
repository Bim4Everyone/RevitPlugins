using System.Windows.Input;

namespace RevitLintelsManager.Views;

/// <summary>Компактное немодальное представление снимка, принадлежащее окну менеджера.</summary>
public partial class ManagerDetailsWindow {
    /// <summary>Создаёт временное окно; ресурсами темы владеет ManagerWindow.</summary>
    public ManagerDetailsWindow() {
        InitializeComponent();
    }

    // Escape закрывает только информационное окно, без воздействия на сессию Revit.
    private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Escape) {
            Close();
            e.Handled = true;
        }
    }
}

using System.Windows;
using System.Windows.Input;

namespace RevitClashDetective.Views.Navigator;

internal partial class ReportUserControl {
    public ReportUserControl() {
        InitializeComponent();
    }

    public static readonly DependencyProperty SelectClashCommandProperty = DependencyProperty.Register(
        nameof(SelectClashCommand),
        typeof(ICommand),
        typeof(ReportUserControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty EditCommentsCommandProperty = DependencyProperty.Register(
        nameof(EditCommentsCommand),
        typeof(ICommand),
        typeof(ReportUserControl),
        new PropertyMetadata(null));

    public ICommand SelectClashCommand {
        get => (ICommand) GetValue(SelectClashCommandProperty);
        set => SetValue(SelectClashCommandProperty, value);
    }

    public ICommand EditCommentsCommand {
        get => (ICommand) GetValue(EditCommentsCommandProperty);
        set => SetValue(EditCommentsCommandProperty, value);
    }
}

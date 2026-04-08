using System.Windows;
using System.Windows.Input;

namespace RevitClashDetective.Views.Navigator;

internal partial class ReportsMergeUserControl {
    public ReportsMergeUserControl() {
        InitializeComponent();
    }

    public static readonly DependencyProperty AcceptMergeCommandProperty = DependencyProperty.Register(
        nameof(AcceptMergeCommand),
        typeof(ICommand),
        typeof(ReportsMergeUserControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CancelMergeCommandProperty = DependencyProperty.Register(
        nameof(CancelMergeCommand),
        typeof(ICommand),
        typeof(ReportsMergeUserControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ShowCommentsCommandProperty = DependencyProperty.Register(
        nameof(ShowCommentsCommand),
        typeof(ICommand),
        typeof(ReportsMergeUserControl),
        new PropertyMetadata(null));

    public ICommand AcceptMergeCommand {
        get => (ICommand) GetValue(AcceptMergeCommandProperty);
        set => SetValue(AcceptMergeCommandProperty, value);
    }

    public ICommand CancelMergeCommand {
        get => (ICommand) GetValue(CancelMergeCommandProperty);
        set => SetValue(CancelMergeCommandProperty, value);
    }

    public ICommand ShowCommentsCommand {
        get => (ICommand) GetValue(ShowCommentsCommandProperty);
        set => SetValue(ShowCommentsCommandProperty, value);
    }
}

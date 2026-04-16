using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Views.Navigator;

internal partial class ReportsMergeUserControl {
    public static readonly DependencyProperty SelectClashCommandProperty = DependencyProperty.Register(
        nameof(SelectClashCommand),
        typeof(ICommand),
        typeof(ReportsMergeUserControl),
        new PropertyMetadata(default(ICommand)));

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

    public ICommand SelectClashCommand {
        get => (ICommand) GetValue(SelectClashCommandProperty);
        set => SetValue(SelectClashCommandProperty, value);
    }

    private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        if(sender is TreeView treeView
           && treeView.DataContext is ReportsMergeViewModel viewModel) {
            if(e.NewValue is ClashMergePairViewModel pairViewModel) {
                viewModel.SelectedClashMergePairItem = pairViewModel;
            }
        }
    }
}

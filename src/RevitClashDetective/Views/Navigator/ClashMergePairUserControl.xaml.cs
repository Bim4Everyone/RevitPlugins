using System.Windows;
using System.Windows.Input;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Views.Navigator;

public partial class ClashMergePairUserControl {
    public static readonly DependencyProperty PickElementCommandProperty = DependencyProperty.Register(
        nameof(PickElementCommand),
        typeof(ICommand),
        typeof(ClashMergePairUserControl),
        new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty SelectClashCommandProperty = DependencyProperty.Register(
        nameof(SelectClashCommand),
        typeof(ICommand),
        typeof(ClashMergePairUserControl),
        new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ShowCommentsCommandProperty = DependencyProperty.Register(
        nameof(ShowCommentsCommand),
        typeof(ICommand),
        typeof(ClashMergePairUserControl),
        new PropertyMetadata(default(ICommand)));

    public ClashMergePairUserControl() {
        InitializeComponent();
    }

    public ICommand ShowCommentsCommand {
        get => (ICommand) GetValue(ShowCommentsCommandProperty);
        set => SetValue(ShowCommentsCommandProperty, value);
    }

    public ICommand SelectClashCommand {
        get => (ICommand) GetValue(SelectClashCommandProperty);
        set => SetValue(SelectClashCommandProperty, value);
    }

    public ICommand PickElementCommand {
        get => (ICommand) GetValue(PickElementCommandProperty);
        set => SetValue(PickElementCommandProperty, value);
    }
}

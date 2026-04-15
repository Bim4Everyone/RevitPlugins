using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitClashDetective.Views.Navigator;

public partial class ClashMergePairUserControl : UserControl {
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
}

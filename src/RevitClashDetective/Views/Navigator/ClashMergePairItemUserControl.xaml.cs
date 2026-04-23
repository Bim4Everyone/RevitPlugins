using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Views.Navigator;

public partial class ClashMergePairItemUserControl : UserControl {
    public static readonly DependencyProperty ShowCommentsCommandProperty = DependencyProperty.Register(
        nameof(ShowCommentsCommand),
        typeof(ICommand),
        typeof(ClashMergePairItemUserControl),
        new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ItemCommentsIsSelectedProperty = DependencyProperty.Register(
        nameof(ItemCommentsIsSelected),
        typeof(bool),
        typeof(ClashMergePairItemUserControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ItemStatusIsSelectedProperty = DependencyProperty.Register(
        nameof(ItemStatusIsSelected),
        typeof(bool),
        typeof(ClashMergePairItemUserControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ItemIsSelectedProperty = DependencyProperty.Register(
        nameof(ItemIsSelected),
        typeof(bool),
        typeof(ClashMergePairItemUserControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ItemNameIsSelectedProperty = DependencyProperty.Register(
        nameof(ItemNameIsSelected),
        typeof(bool),
        typeof(ClashMergePairItemUserControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(ClashMergePairItemUserControl),
        new PropertyMetadata(default(string)));

    public ClashMergePairItemUserControl() {
        InitializeComponent();
    }

    public string Title {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ItemNameIsSelected {
        get => (bool) GetValue(ItemNameIsSelectedProperty);
        set => SetValue(ItemNameIsSelectedProperty, value);
    }

    public bool ItemIsSelected {
        get => (bool) GetValue(ItemIsSelectedProperty);
        set => SetValue(ItemIsSelectedProperty, value);
    }

    public bool ItemStatusIsSelected {
        get => (bool) GetValue(ItemStatusIsSelectedProperty);
        set => SetValue(ItemStatusIsSelectedProperty, value);
    }

    public bool ItemCommentsIsSelected {
        get => (bool) GetValue(ItemCommentsIsSelectedProperty);
        set => SetValue(ItemCommentsIsSelectedProperty, value);
    }

    public ICommand ShowCommentsCommand {
        get => (ICommand) GetValue(ShowCommentsCommandProperty);
        set => SetValue(ShowCommentsCommandProperty, value);
    }
}

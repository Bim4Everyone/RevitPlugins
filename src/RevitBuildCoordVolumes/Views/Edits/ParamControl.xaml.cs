using System.Windows;

namespace RevitBuildCoordVolumes.Views.Edits;

public partial class ParamControl {

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DetailDescriptionProperty = DependencyProperty.Register(
        nameof(DetailDescription), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(ParamControl), new PropertyMetadata(true));

    public static readonly DependencyProperty HasSourceParamProperty = DependencyProperty.Register(
        nameof(HasSourceParam), typeof(bool), typeof(ParamControl), new PropertyMetadata(true));

    public static readonly DependencyProperty HasTargetParamProperty = DependencyProperty.Register(
        nameof(HasTargetParam), typeof(bool), typeof(ParamControl), new PropertyMetadata(true));

    public static readonly DependencyProperty SourceParamNameProperty = DependencyProperty.Register(
        nameof(SourceParamName), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty TargetParamNameProperty = DependencyProperty.Register(
        nameof(TargetParamName), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HasWarningProperty = DependencyProperty.Register(
        nameof(HasWarning), typeof(bool), typeof(ParamControl), new PropertyMetadata(true));

    public static readonly DependencyProperty WarningDescriptionProperty = DependencyProperty.Register(
        nameof(WarningDescription), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));


    public ParamControl() {
        InitializeComponent();
    }

    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string DetailDescription {
        get => (string) GetValue(DetailDescriptionProperty);
        set => SetValue(DetailDescriptionProperty, value);
    }

    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public bool HasSourceParam {
        get => (bool) GetValue(HasSourceParamProperty);
        set => SetValue(HasSourceParamProperty, value);
    }

    public bool HasTargetParam {
        get => (bool) GetValue(HasTargetParamProperty);
        set => SetValue(HasTargetParamProperty, value);
    }

    public string SourceParamName {
        get => (string) GetValue(SourceParamNameProperty);
        set => SetValue(SourceParamNameProperty, value);
    }

    public string TargetParamName {
        get => (string) GetValue(TargetParamNameProperty);
        set => SetValue(TargetParamNameProperty, value);
    }

    public bool HasWarning {
        get => (bool) GetValue(HasWarningProperty);
        set => SetValue(HasWarningProperty, value);
    }

    public string WarningDescription {
        get => (string) GetValue(WarningDescriptionProperty);
        set => SetValue(WarningDescriptionProperty, value);
    }
}

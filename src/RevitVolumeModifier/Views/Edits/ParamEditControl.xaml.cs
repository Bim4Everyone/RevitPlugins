using System.Windows;

namespace RevitVolumeModifier.Views.Edits;

public partial class ParamEditControl {

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(ParamEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DetailDescriptionProperty = DependencyProperty.Register(
        nameof(DetailDescription), typeof(string), typeof(ParamEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty RevitParamNameProperty = DependencyProperty.Register(
        nameof(RevitParamName), typeof(string), typeof(ParamEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HasWarningProperty = DependencyProperty.Register(
        nameof(HasWarning), typeof(bool), typeof(ParamEditControl), new PropertyMetadata(true));

    public static readonly DependencyProperty WarningProperty = DependencyProperty.Register(
        nameof(Warning), typeof(string), typeof(ParamEditControl), new PropertyMetadata(default(string)));


    public ParamEditControl() {
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

    public string RevitParamName {
        get => (string) GetValue(RevitParamNameProperty);
        set => SetValue(RevitParamNameProperty, value);
    }

    public bool HasWarning {
        get => (bool) GetValue(HasWarningProperty);
        set => SetValue(HasWarningProperty, value);
    }

    public string Warning {
        get => (string) GetValue(WarningProperty);
        set => SetValue(WarningProperty, value);
    }
}

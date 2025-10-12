using System.Windows;

namespace RevitSetCoordParams.Views.Edits;

public partial class ParamControl {

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(ParamControl), new PropertyMetadata(true));

    public static readonly DependencyProperty SwitchOnContentProperty = DependencyProperty.Register(
        nameof(SwitchOnContent), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty SwitchOffContentProperty = DependencyProperty.Register(
        nameof(SwitchOffContent), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty CommonParamHeaderProperty = DependencyProperty.Register(
        nameof(CommonParamHeader), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty CommonParamProperty = DependencyProperty.Register(
        nameof(CommonParam), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ElementParamHeaderProperty = DependencyProperty.Register(
        nameof(ElementParamHeader), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ElementParamProperty = DependencyProperty.Register(
        nameof(ElementParam), typeof(string), typeof(ParamControl), new PropertyMetadata(default(string)));


    public ParamControl() {
        InitializeComponent();
    }

    public string Header {
        get => (string) GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string SwitchOnContent {
        get => (string) GetValue(SwitchOnContentProperty);
        set => SetValue(SwitchOnContentProperty, value);
    }

    public string SwitchOffContent {
        get => (string) GetValue(SwitchOffContentProperty);
        set => SetValue(SwitchOffContentProperty, value);
    }

    public string CommonParamHeader {
        get => (string) GetValue(CommonParamHeaderProperty);
        set => SetValue(CommonParamHeaderProperty, value);
    }

    public string CommonParam {
        get => (string) GetValue(CommonParamProperty);
        set => SetValue(CommonParamProperty, value);
    }

    public string ElementParamHeader {
        get => (string) GetValue(ElementParamHeaderProperty);
        set => SetValue(ElementParamHeaderProperty, value);
    }

    public string ElementParam {
        get => (string) GetValue(ElementParamProperty);
        set => SetValue(ElementParamProperty, value);
    }
}

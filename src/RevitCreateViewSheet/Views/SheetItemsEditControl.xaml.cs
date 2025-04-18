using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace RevitCreateViewSheet.Views {
    [DefaultProperty(nameof(EditControl))]
    [ContentProperty(nameof(EditControl))]
    public partial class SheetItemsEditControl {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(SheetItemsEditControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
            nameof(EditControl), typeof(object), typeof(SheetItemsEditControl), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
            nameof(ButtonCommand), typeof(ICommand), typeof(SheetItemsEditControl), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
            nameof(ButtonContent), typeof(object), typeof(SheetItemsEditControl), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ButtonToolTipProperty = DependencyProperty.Register(
            nameof(ButtonToolTip), typeof(string), typeof(SheetItemsEditControl), new PropertyMetadata(default(string)));

        public SheetItemsEditControl() {
            InitializeComponent();
        }


        public string Header {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object EditControl {
            get => GetValue(EditControlProperty);
            set => SetValue(EditControlProperty, value);
        }

        public ICommand ButtonCommand {
            get => (ICommand) GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        public object ButtonContent {
            get => GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        public string ButtonToolTip {
            get => (string) GetValue(ButtonToolTipProperty);
            set => SetValue(ButtonToolTipProperty, value);
        }
    }
}

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RevitSetLevelSection.Views {
    [DefaultProperty(nameof(Right))]
    [System.Windows.Markup.ContentProperty(nameof(Right))]
    public partial class SimpleItemView {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(object), 
            typeof(SimpleItemView), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty RightProperty = DependencyProperty.Register(
            nameof(Right), typeof(object), 
            typeof(SimpleItemView), new PropertyMetadata(default(object)));

        public SimpleItemView() {
            InitializeComponent();
        }
        
        public object Title {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public object Right {
            get => GetValue(RightProperty);
            set => SetValue(RightProperty, value);
        }
    }
}
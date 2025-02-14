using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace RevitRefreshLinks.Resources {
    internal class ListViewDoubleClickBehavior : Behavior<ListView> {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(ListViewDoubleClickBehavior));

        public ICommand Command {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }


        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(sender is ListView && e.OriginalSource is FrameworkElement originalSource) {
                var clickedItem = originalSource.DataContext;
                if(clickedItem != null && Command.CanExecute(clickedItem)) {
                    Command.Execute(clickedItem);
                }
            }
        }
    }
}

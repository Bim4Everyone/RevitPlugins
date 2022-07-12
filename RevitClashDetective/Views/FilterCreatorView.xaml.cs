using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using dosymep.WPF.Views;

using RevitClashDetective.ViewModels.FilterCreatorViewModels;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for FilterCreatorView.xaml
    /// </summary>
    public partial class FilterCreatorView {
        public FilterCreatorView() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitClashDetective);
        public override string ProjectConfigName => nameof(FilterCreatorView);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void CriterionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ((ContentControl) sender).Content = new CriterionView() { DataContext = ((ContentControl) sender).DataContext };
        }

        private void CategoryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ((ContentControl) sender).Content = new CategoryView() { DataContext = ((ContentControl) sender).DataContext };
        }
    }
}

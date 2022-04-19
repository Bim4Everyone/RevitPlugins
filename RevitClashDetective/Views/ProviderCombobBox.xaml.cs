using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using RevitClashDetective.ViewModels;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for ProviderCombobBox.xaml
    /// </summary>
    public partial class ProviderCombobBox : UserControl {
        internal static readonly DependencyProperty ProvidersProperty =
            DependencyProperty.Register(nameof(Providers), typeof(ObservableCollection<ProviderViewModel>), typeof(ProviderCombobBox));

        internal static readonly DependencyProperty SelectedProvidersProperty =
            DependencyProperty.Register(nameof(SelectedProviders), typeof(string), typeof(ProviderCombobBox));

        public ProviderCombobBox() {
            InitializeComponent();
        }

        internal string SelectedProviders {
            get { return (string) GetValue(SelectedProvidersProperty); }
            set { SetValue(SelectedProvidersProperty, value); }
        }

        internal ObservableCollection<ProviderViewModel> Providers {
            get { return (ObservableCollection<ProviderViewModel>) GetValue(ProvidersProperty); }
            set { SetValue(ProvidersProperty, value); }
        }

        private void cbMain_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
        }
    }
}

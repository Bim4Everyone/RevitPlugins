using System;
using System.Collections;
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
using System.Windows.Shapes;

using DevExpress.Xpf.Core;

using dosymep.WPF.Commands;
using dosymep.WPF.Views;

using RevitRooms.ViewModels;

namespace RevitRooms.Views {
    /// <summary>
    /// Interaction logic for NumberingOrderSelectWindow.xaml
    /// </summary>
    public partial class NumberingOrderSelectWindow {
        public NumberingOrderSelectWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRooms);
        public override string ProjectConfigName => nameof(NumberingOrderSelectWindow);

        private void ButtonOK_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }

    internal class NumberingOrderSelectViewModel {
        public NumberingOrderSelectViewModel() {
            SelectCommand = new RelayCommand(Select, CanSelect);

            NumberingOrders = new ObservableCollection<NumberingOrderViewModel>();
            SelectedNumberingOrders = new ObservableCollection<NumberingOrderViewModel>();
        }

        public ICommand SelectCommand { get; set; }
        public IList SelectedNumberingOrders { get; set; }
        public ObservableCollection<NumberingOrderViewModel> NumberingOrders { get; set; }

        private void Select(object parameter) {

        }

        private bool CanSelect(object parameter) {
            return SelectedNumberingOrders.Count > 0;
        }
    }
}
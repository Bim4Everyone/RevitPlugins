using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using dosymep.WPF.Commands;

using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Commands {
    internal class AddOrderCommand : BaseCommand {
        private readonly INumberingOrder _numberingOrder;

        public AddOrderCommand(INumberingOrder numberingOrder) {
            _numberingOrder = numberingOrder;
        }

        public override void Execute(object parameter) {
            var window = new NumberingOrderSelectWindow() {
                DataContext = new NumberingOrderSelectViewModel() {
                    NumberingOrders =
                        new ObservableCollection<NumberingOrderViewModel>(
                            _numberingOrder.NumberingOrders.OrderBy(item => item.Name))
                }
            };

            if(window.ShowDialog() == true) {
                var selection = (window.DataContext as NumberingOrderSelectViewModel).SelectedNumberingOrders;
                _numberingOrder.SelectNumberingOrder(selection.OfType<NumberingOrderViewModel>());
            }
        }
    }
}
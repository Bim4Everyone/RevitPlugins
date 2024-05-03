using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitRooms.ViewModels;

namespace RevitRooms.Commands {
    internal class RemoveOrderCommand : BaseCommand {
        private readonly INumberingOrder _numberingOrder;

        public RemoveOrderCommand(INumberingOrder numberingOrder) {
            _numberingOrder = numberingOrder;
        }

        public override void Execute(object parameter) {
            var selection = ((ObservableCollection<object>) parameter)
                .Cast<NumberingOrderViewModel>()
                .ToArray();

            foreach(var selected in selection) {
                _numberingOrder.NumberingOrders.Add(selected);
                _numberingOrder.SelectedNumberingOrders.Remove(selected);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitRooms.ViewModels;

namespace RevitRooms.Commands {
    internal class DownOrderCommand : BaseCommand {
        private readonly INumberingOrder _numberingOrder;

        public DownOrderCommand(INumberingOrder numberingOrder) {
            _numberingOrder = numberingOrder;
        }

        public override void Execute(object parameter) {
            var selection = ((ObservableCollection<object>) parameter)
                 .Cast<NumberingOrderViewModel>()
                 .Select(item => (Index: _numberingOrder.SelectedNumberingOrders.IndexOf(item), Item: item))
                 .OrderByDescending(item => item.Index)
                 .ToArray();

            if(selection.Length == 0) {
                return;
            }

            var max = selection.Max(item => item.Index);
            if(max >= _numberingOrder.SelectedNumberingOrders.Count - 1) {
                return;
            }

            foreach(var selected in selection) {
                if(selected.Index < _numberingOrder.SelectedNumberingOrders.Count - 1) {
                    _numberingOrder.SelectedNumberingOrders.Move(selected.Index, selected.Index + 1);
                }
            }
        }
    }
}

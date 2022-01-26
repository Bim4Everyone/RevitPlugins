using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitRooms.ViewModels;

namespace RevitRooms.Commands {
    internal class UpOrderCommand : BaseCommand {
        private readonly INumberingOrder _numberingOrder;

        public UpOrderCommand(INumberingOrder numberingOrder) {
            _numberingOrder = numberingOrder;
        }

        public override void Execute(object parameter) {
            var selection = ((ObservableCollection<object>) parameter)
                .Cast<NumberingOrderViewModel>()
                .Select(item => (Index: _numberingOrder.SelectedNumberingOrders.IndexOf(item), Item: item))
                .OrderBy(item => item.Index)
                .ToArray();

            if(selection.Length == 0) {
                return;
            }

            var min = selection.Min(item => item.Index);
            if(min <= 0) {
                return;
            }

            foreach(var selected in selection) {
                if(selected.Index > 0) {
                    _numberingOrder.SelectedNumberingOrders.Move(selected.Index, selected.Index - 1);
                }
            }
        }
    }
}

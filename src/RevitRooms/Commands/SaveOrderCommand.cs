using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands {
    internal class SaveOrderCommand : BaseCommand {
        private readonly INumberingOrder _numberingOrder;
        private readonly RevitRepository _revitRepository;

        public SaveOrderCommand(INumberingOrder numberingOrder, RevitRepository revitRepository) {
            _numberingOrder = numberingOrder;
            _revitRepository = revitRepository;
        }

        public override void Execute(object parameter) {
            using(var transaction = _revitRepository.StartTransaction("Изменение приоритета нумерации")) {
                foreach(var order in _numberingOrder.NumberingOrders) {
                    order.Order = 0;
                    order.UpdateOrder();
                }

                int count = 0;
                foreach(var order in _numberingOrder.SelectedNumberingOrders) {
                    order.Order = ++count;
                    order.UpdateOrder();
                }

                transaction.Commit();
            }
        }
    }
}

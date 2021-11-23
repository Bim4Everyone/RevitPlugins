using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitRooms.ViewModels {
    internal interface INumberingOrder {
        ObservableCollection<NumberingOrderViewModel> NumberingOrders { get; }
        ObservableCollection<NumberingOrderViewModel> SelectedNumberingOrders { get; }

        void SelectNumberingOrder(IEnumerable<NumberingOrderViewModel> selection);
    }
}
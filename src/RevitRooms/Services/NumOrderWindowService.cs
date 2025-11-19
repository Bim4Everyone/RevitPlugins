using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;
using Ninject.Syntax;

using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Services;

internal class NumOrderWindowService {
    private readonly IResolutionRoot _resolutionRoot;
    private INumberingOrder _numberingOrder;

    public NumOrderWindowService(IResolutionRoot resolutionRoot) {
        _resolutionRoot = resolutionRoot;
    }

    public INumberingOrder NumberingOrder => _numberingOrder;

    public bool ShowWindow(INumberingOrder numberingOrder) {
        var window = _resolutionRoot.Get<NumberingOrderSelectWindow>();
        window.DataContext = new NumberingOrderSelectViewModel() {
            NumberingOrders =
                [.. numberingOrder.NumberingOrders.OrderBy(item => item.Name)]
        };

        if(window.ShowDialog() == true) {
            var selection = (window.DataContext as NumberingOrderSelectViewModel).SelectedNumberingOrders;
            numberingOrder.SelectNumberingOrder(selection.OfType<NumberingOrderViewModel>());
            _numberingOrder = numberingOrder;
            return true;
        }

        return false;
    }
}

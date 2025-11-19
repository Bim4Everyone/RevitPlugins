using System.Linq;

using RevitRooms.Services;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms.Commands;
internal class AddOrderCommand : BaseCommand {
    private readonly NumOrderWindowService _numberingWindowService;
    private INumberingOrder _numberingOrder;

    public AddOrderCommand(INumberingOrder numberingOrder, NumOrderWindowService numberingWindowService) {
        _numberingWindowService = numberingWindowService;
        _numberingOrder = numberingOrder;
    }

    public override void Execute(object parameter) {
        if(_numberingWindowService.ShowWindow(_numberingOrder)) {
            _numberingOrder = _numberingWindowService.NumberingOrder;
        }
    }
}

using dosymep.SimpleServices;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands;
internal class SaveOrderCommand : BaseCommand {
    private readonly INumberingOrder _numberingOrder;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public SaveOrderCommand(INumberingOrder numberingOrder, RevitRepository revitRepository, ILocalizationService localizationService) {
        _numberingOrder = numberingOrder;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    public override void Execute(object parameter) {
        string transactionName = _localizationService.GetLocalizedString("Transaction.UpdateNumOrder");
        using var transaction = _revitRepository.StartTransaction(transactionName);
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

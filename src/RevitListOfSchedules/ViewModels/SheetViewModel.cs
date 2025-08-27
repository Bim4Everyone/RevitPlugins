using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;
internal class SheetViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly SheetElement _sheetElement;

    public SheetViewModel(
        ILocalizationService localizationService,
        SheetElement sheetElement,
        LinkViewModel linkViewModel = null) {

        _localizationService = localizationService;
        _sheetElement = sheetElement;
        LinkViewModel = linkViewModel;
        ViewSheet = _sheetElement.Sheet;
        Id = ViewSheet.Id;
        Name = _sheetElement.Name;
        Number = _sheetElement.Number;
        RevisionNumber = _sheetElement.RevisionNumber;
    }

    public LinkViewModel LinkViewModel { get; }

    public ViewSheet ViewSheet { get; }

    public RevitParam GroupParameter { get; set; }

    public ElementId Id { get; }

    public string Name { get; }

    public string Number { get; }

    public string RevisionNumber { get; }

    public string AlbumName => GetAlbumName();

    private string GetAlbumName() {
        return GroupParameter != null
            ? _sheetElement.Sheet.GetParamValueOrDefault<string>(GroupParameter)
                ?? _localizationService.GetLocalizedString("GroupValue.NoValue")
            : _localizationService.GetLocalizedString("GroupParameter.NoParameter");
    }
}

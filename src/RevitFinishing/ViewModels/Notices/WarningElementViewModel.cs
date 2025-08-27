using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal class WarningElementViewModel : BaseViewModel, IWarningItemViewModel {
    private readonly Element _element;
    private readonly string _phaseName;
    private readonly string _levelName;
    private readonly ILocalizationService _localizationService;

    public WarningElementViewModel(Element element,
                                  string phaseName,
                                  ILocalizationService localizationService) {
        _element = element;
        _phaseName = phaseName;
        _localizationService = localizationService;

        ElementId levelId = _element.LevelId;
            _levelName = levelId != ElementId.InvalidElementId
            ? _element.Document.GetElement(levelId).Name
            : _localizationService.GetLocalizedString("ErrorsWindow.WithoutLevel");
    }

    public string ElementIdInfo => _element.Id.ToString();
    public string ElementName => _element.Name;
    public string CategoryInfo => _element.Category.Name;
    public string PhaseName => _phaseName;
    public string LevelName => _levelName;
}

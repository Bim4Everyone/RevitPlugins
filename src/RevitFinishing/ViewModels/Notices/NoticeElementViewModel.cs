using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;
internal class NoticeElementViewModel : BaseViewModel {
    private readonly Element _element;
    private readonly string _phaseName;
    private readonly string _levelName;
    private readonly ILocalizationService _localizationService;

    public NoticeElementViewModel(Element element,
                                    string phaseName,
                                    ILocalizationService localizationService) {
        _element = element;
        _phaseName = phaseName;
        _localizationService = localizationService;

        ElementId levelId = _element.LevelId;
            _levelName = levelId != ElementId.InvalidElementId
            ? _element.Document.GetElement(levelId).Name
            : _localizationService.GetLocalizedString("ErrorsWindow.WithoutName");
    }

    public ElementId ElementId => _element.Id;
    public string ElementName => _element.Name;
    public string CategoryName => _element.Category.Name;
    public string PhaseName => _phaseName;
    public string LevelName => _levelName;
}

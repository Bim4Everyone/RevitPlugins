using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels.Notices;

internal class WarningScheduleKeyVM : BaseViewModel, IWarningItemViewModel {
    private readonly Element _element;
    private readonly string _phaseName;
    private readonly string _levelName;
    private readonly string _categoryInfo;
    private readonly ILocalizationService _localizationService;

    public WarningScheduleKeyVM(Element element,
                                string phaseName,
                                string categoryInfo,
                                ILocalizationService localizationService) {
        _element = element;
        _phaseName = phaseName;
        _localizationService = localizationService;
        _categoryInfo = categoryInfo;

        _levelName = _localizationService.GetLocalizedString("ErrorsWindow.WithoutLevel");
    }

    public string ElementIdInfo => _localizationService.GetLocalizedString("ErrorsWindow.WithoutId");
    public string ElementName => _element.Name;
    public string CategoryInfo => _categoryInfo;
    public string PhaseName => _phaseName;
    public string LevelName => _levelName;
}

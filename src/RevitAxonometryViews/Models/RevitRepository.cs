using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitAxonometryViews.Models;
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly string _emptyName;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        AxonometryConfig = new AxonometryConfig(Document);
        _localizationService = localizationService;
        _emptyName = _localizationService.GetLocalizedString("MainWindow.EmptyName");
    }
    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public AxonometryConfig AxonometryConfig { get; }

    /// <summary>
    /// Получаем ФОП_ВИС_Имя системы если оно есть в проекте
    /// </summary>
    public string GetSharedSystemName(MEPSystem system) {
        ElementSet elementSet = null;
        if(system.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
            elementSet = ((MechanicalSystem) system).DuctNetwork;
        }
        if(system.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
            elementSet = ((PipingSystem) system).PipingNetwork;
        }
        // Нужно перебирать элементы пока не встретим заполненный параметр, могут быть не до конца обработаны элементы.
        // Выбрасываем первое встреченное заполненное значение
        if(elementSet != null && !elementSet.IsEmpty) {
            foreach(Element element in elementSet) {
                string result = element.GetSharedParamValueOrDefault<string>(
                    AxonometryConfig.SharedVisSystemName,
                    null);
                if(result != null) {
                    return result;
                }
            }
        }

        return _emptyName;
    }
}

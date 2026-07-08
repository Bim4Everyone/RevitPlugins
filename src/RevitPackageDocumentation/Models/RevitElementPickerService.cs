using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Views;

namespace RevitPackageDocumentation.Models;
public interface IRevitElementPickerService {
    void PickElement(Action<Element> onSelected);
}

public class RevitElementPickerService : IRevitElementPickerService {
    private readonly RevitRepository _revitRepository;
    private readonly MainWindow _mainWindow;
    private readonly ILocalizationService _localizationService;

    private RevitElementPickerService(
        RevitRepository revitRepository,
        MainWindow mainWindow,
        ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _mainWindow = mainWindow;
        _localizationService = localizationService;
    }

    internal static RevitElementPickerService GetRevitElementPickerService(
        RevitRepository revitRepository,
        MainWindow mainWindow,
        ILocalizationService localizationService) {

        return new RevitElementPickerService(revitRepository, mainWindow, localizationService);
    }

    public void PickElement(Action<Element> onSelected) {
        _mainWindow.Hide();
        ISelectionFilter selectFilter = new FloorSelectionFilter();
        var reference = _revitRepository.ActiveUIDocument.Selection.PickObject(
            ObjectType.Element,
            selectFilter,
            _localizationService.GetLocalizedString("MainWindow.PickElement"));
        var element = _revitRepository.Document.GetElement(reference);
        onSelected?.Invoke(element);
        _mainWindow.ShowDialog();
    }
}

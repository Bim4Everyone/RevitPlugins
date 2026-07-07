using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

public interface IFileDialogService {
    string OpenFileDialog();
    string SaveFileDialog(string defaultName = "config.json");
}

public class JsonFileDialogService : IFileDialogService {
    private readonly ILocalizationService _localizationService;

    public JsonFileDialogService(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public string OpenFileDialog() {
        var dialog = new FileOpenDialog("JSON files (*.json)|*.json|All files (*.*)|*.*") {
            Title = $"{_localizationService.GetLocalizedString("MainViewModel.SelectConfigurationFile")}"
        };

        if(dialog.Show() == ItemSelectionDialogResult.Confirmed) {
            ModelPath selectedPath = dialog.GetSelectedModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(selectedPath);
        }
        return null;
    }

    public string SaveFileDialog(string defaultName = "config.json") {
        var dialog = new FileSaveDialog("JSON files (*.json)|*.json") {
            Title = $"{_localizationService.GetLocalizedString("MainViewModel.SaveConfiguration")}",
            InitialFileName = defaultName
        };

        if(dialog.Show() == ItemSelectionDialogResult.Confirmed) {
            ModelPath selectedPath = dialog.GetSelectedModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(selectedPath);
        }
        return null;
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

public interface IFileDialogService {
    string OpenFileDialog();
    string SaveFileDialog(string defaultName = "config.json");
}

public class JsonFileDialogService : IFileDialogService {
    public string OpenFileDialog() {
        var dialog = new FileOpenDialog("JSON files (*.json)|*.json|All files (*.*)|*.*") {
            Title = "Выберите файл конфигурации"
        };

        if(dialog.Show() == ItemSelectionDialogResult.Confirmed) {
            ModelPath selectedPath = dialog.GetSelectedModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(selectedPath);
        }
        return null;
    }

    public string SaveFileDialog(string defaultName = "config.json") {
        var dialog = new FileSaveDialog("JSON files (*.json)|*.json") {
            Title = "Сохранить конфигурацию",
            InitialFileName = defaultName
        };

        if(dialog.Show() == ItemSelectionDialogResult.Confirmed) {
            ModelPath selectedPath = dialog.GetSelectedModelPath();
            return ModelPathUtils.ConvertModelPathToUserVisiblePath(selectedPath);
        }
        return null;
    }
}

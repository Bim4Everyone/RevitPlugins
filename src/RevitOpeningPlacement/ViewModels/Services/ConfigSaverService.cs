using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Services;
internal class ConfigSaverService {
    private readonly ISaveFileDialogService _saveFileDialogService;

    public ConfigSaverService(ISaveFileDialogService saveFileDialogService) {
        _saveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
    }


    public string Save(ProjectConfig config, Document document) {
        if(document is null) { throw new ArgumentNullException(nameof(document)); }

        _saveFileDialogService.AddExtension = true;
        _saveFileDialogService.Filter = "OpeningConfig |*.json";
        _saveFileDialogService.FilterIndex = 1;
        if(!_saveFileDialogService.ShowDialog(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Настройки заданий на отверстия")) {
            throw new OperationCanceledException();
        }
        var configSaver = new ConfigSaver(document);
        configSaver.Save(config, _saveFileDialogService.File.FullName);
        return _saveFileDialogService.File.FullName;
    }
}

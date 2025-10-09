using System;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Services;
internal class ConfigSaverService {
    private readonly RevitRepository _revitRepository;

    public ConfigSaverService(RevitRepository revitRepository, ISaveFileDialogService saveFileDialogService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
    }

    public ISaveFileDialogService SaveFileDialogService { get; }

    public void Save(ProjectConfig config) {
        SaveFileDialogService.AddExtension = true;
        SaveFileDialogService.Filter = "ClashConfig |*.json";
        SaveFileDialogService.FilterIndex = 1;

        if(!SaveFileDialogService.ShowDialog(_revitRepository.GetFileDialogPath(), "config")) {
            throw new OperationCanceledException();
        }
        var configSaver = new ConfigSaver(_revitRepository.Doc);
        configSaver.Save(config, SaveFileDialogService.File.FullName);

        _revitRepository.CommonConfig.LastRunPath = SaveFileDialogService.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();
    }
}

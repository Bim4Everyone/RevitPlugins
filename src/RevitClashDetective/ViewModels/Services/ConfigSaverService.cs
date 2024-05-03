using System;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Services {
    internal class ConfigSaverService {
        private readonly RevitRepository _revitRepository;

        public ConfigSaverService(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void Save(ProjectConfig config) {
            var saveWindow = GetPlatformService<ISaveFileDialogService>();
            saveWindow.AddExtension = true;
            saveWindow.Filter = "ClashConfig |*.json";
            saveWindow.FilterIndex = 1;

            if(!saveWindow.ShowDialog(_revitRepository.GetFileDialogPath(), "config")) {
                throw new OperationCanceledException();
            }
            var configSaver = new ConfigSaver(_revitRepository.Doc);
            configSaver.Save(config, saveWindow.File.FullName);

            _revitRepository.CommonConfig.LastRunPath = saveWindow.File.DirectoryName;
            _revitRepository.CommonConfig.SaveProjectConfig();
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}

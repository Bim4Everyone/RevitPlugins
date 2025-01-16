using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using dosymep.SimpleServices;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class ManyFoldersLinksProvider : ITwoSourceLinksProvider {
        private readonly RevitRepository _revitRepository;
        private readonly IConfigProvider _configProvider;
        private readonly IOpenFolderDialogService _openFolderDialog;

        public ManyFoldersLinksProvider(
            RevitRepository revitRepository,
            IConfigProvider configProvider,
            IOpenFolderDialogService openFolderDialog) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _openFolderDialog = openFolderDialog ?? throw new ArgumentNullException(nameof(openFolderDialog));
        }


        public ICollection<ILink> GetLocalLinks() {
            if(_openFolderDialog.ShowDialog()) {
                var config = _configProvider.GetUpdateLinksConfig();
                var settings = config.GetSettings(_revitRepository.Document)
                    ?? config.AddSettings(_revitRepository.Document);
                settings.InitialFolderPath = _openFolderDialog.Folder.FullName;
                config.SaveProjectConfig();

                const string searchPattern = "*.rvt";

                return _openFolderDialog.Folders
                    .SelectMany(dir => Directory.EnumerateFiles(
                        dir.FullName,
                        searchPattern,
                        SearchOption.AllDirectories))
                    .Select(path => new Link(path))
                    .ToArray();
            } else {
                throw new OperationCanceledException();
            }
        }

        public ICollection<ILink> GetServerLinks() {
            // TODO
            // Временная заглушка для тестов плагина, пока не будет готово нормальное окно выбора папок из RS
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "test\\RevitRefreshLinks\\rs-fullpath-list.txt");
            if(!File.Exists(path)) {
                File.Create(path);
            }
            return File.ReadLines(path).Select(line => new Link(line)).ToArray();
        }
    }
}

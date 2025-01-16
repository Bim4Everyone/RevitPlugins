using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class OneRsFolderLinksProvider : ILinksToAddProvider {
        private readonly RevitRepository _revitRepository;
        private readonly IConfigProvider _configProvider;

        public OneRsFolderLinksProvider(
            RevitRepository revitRepository,
            IConfigProvider configProvider) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        }


        public ICollection<ILink> GetLinks() {
            // TODO
            // Временная заглушка для тестов плагина, пока не будет готово нормальное окно выбора файлов из RS
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

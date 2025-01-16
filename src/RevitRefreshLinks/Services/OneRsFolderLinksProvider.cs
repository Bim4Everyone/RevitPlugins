using System;
using System.Collections.Generic;

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
            throw new NotImplementedException();
        }
    }
}

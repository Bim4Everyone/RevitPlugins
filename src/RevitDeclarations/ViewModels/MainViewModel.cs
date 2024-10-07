using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private protected readonly RevitRepository _revitRepository;
        private protected readonly DeclarationSettings _settings;

        private protected readonly IList<RevitDocumentViewModel> _revitDocuments;

        public MainViewModel(RevitRepository revitRepository, DeclarationSettings settings) {
            _revitRepository = revitRepository;
            _settings = settings;

            _revitDocuments = _revitRepository
                .GetLinks()
                .Select(x => new RevitDocumentViewModel(x, _settings))
                .Where(x => x.HasRooms())
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
    }
}

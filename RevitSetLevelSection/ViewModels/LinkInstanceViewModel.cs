using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkInstanceViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitLinkInstance _revitLinkInstance;
        private readonly LinkInstanceRepository _linkInstanceRepository;
        private bool _isLoaded;

        public LinkInstanceViewModel(RevitLinkType revitLinkType, RevitLinkInstance revitLinkInstance) {
            _revitLinkType = revitLinkType;
            _revitLinkInstance = revitLinkInstance;
            _linkInstanceRepository =
                new LinkInstanceRepository(revitLinkType.Document.Application, revitLinkInstance.GetLinkDocument());

            IsLoaded = _revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
            LoadLinkDocumentCommand = new RelayCommand(LoadLinkDocument, CanLoadLinkDocument);
        }

        public string Name => _revitLinkType.Name;
        
        public ICommand LoadLinkDocumentCommand { get; } 

        public bool IsLoaded {
            get => _isLoaded;
            set => this.RaiseAndSetIfChanged(ref _isLoaded, value);
        }

        public IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            return _linkInstanceRepository.GetDesignOptions()
                .Select(item => new DesignOptionsViewModel(item, _linkInstanceRepository));
        }

        private void LoadLinkDocument(object param) {
            IsLoaded = _revitLinkType.Load().LoadResult == LinkLoadResultType.LinkLoaded;
        }

        private bool CanLoadLinkDocument(object param) {
            return !IsLoaded;
        }
    }
}
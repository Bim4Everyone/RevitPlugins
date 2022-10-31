using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkTypeViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;
        private readonly LinkInstanceRepository _linkInstanceRepository;
        
        private bool _isLoaded;
        private ObservableCollection<DesignOptionsViewModel> _designOptions;
        private ObservableCollection<string> _buildParts;

        public LinkTypeViewModel(RevitLinkType revitLinkType, RevitRepository revitRepository) {
            _revitLinkType = revitLinkType;
            _revitRepository = revitRepository;

            _linkInstanceRepository =
                new LinkInstanceRepository(_revitRepository, _revitLinkType);

            IsLoaded = _linkInstanceRepository.LinkIsLoaded();
            BuildParts = new ObservableCollection<string>(GetPartNames());
            LoadLinkDocumentCommand = new RelayCommand(LoadLinkDocument, CanLoadLinkDocument);
            DesignOptions = IsLoaded
                ? new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions())
                : new ObservableCollection<DesignOptionsViewModel>();
        }

        public int Id => _revitLinkType.Id.IntegerValue;
        public string Name => _revitLinkType.Name;
        public ICommand LoadLinkDocumentCommand { get; }

        public bool IsLoaded {
            get => _isLoaded;
            set => this.RaiseAndSetIfChanged(ref _isLoaded, value);
        }
        
        public ObservableCollection<string> BuildParts {
            get => _buildParts;
            set => this.RaiseAndSetIfChanged(ref _buildParts, value);
        }

        public ObservableCollection<DesignOptionsViewModel> DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        private IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            if(!_linkInstanceRepository.LinkIsLoaded()) {
                return Enumerable.Empty<DesignOptionsViewModel>();
            }

            return _linkInstanceRepository.GetDesignOptions()
                .Select(item => new DesignOptionsViewModel(item, _linkInstanceRepository));
        }

        public IEnumerable<string> GetPartNames() {
            return _linkInstanceRepository.GetPartNames().Distinct();
        }

        private void LoadLinkDocument(object param) {
            IsLoaded = _linkInstanceRepository.LoadLinkDocument();
            BuildParts = new ObservableCollection<string>(GetPartNames());
            DesignOptions = new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions());
        }

        private bool CanLoadLinkDocument(object param) {
            return !IsLoaded;
        }
    }
}
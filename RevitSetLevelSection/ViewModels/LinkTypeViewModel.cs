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

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkTypeViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IDesignOptionFactory _designOptionFactory;
        private readonly LinkInstanceRepository _linkInstanceRepository;

        private readonly Workset _workset;

        private bool _isLinkLoaded;
        private string _linkLoadToolTip;
        private string _buildPart;

        private ObservableCollection<string> _buildParts;
        private ObservableCollection<DesignOptionsViewModel> _designOptions;

        public LinkTypeViewModel(RevitLinkType revitLinkType,
            RevitRepository revitRepository,
            IViewModelFactory viewModelFactory,
            IDesignOptionFactory designOptionFactory) {
            _revitLinkType = revitLinkType;
            _revitRepository = revitRepository;
            _viewModelFactory = viewModelFactory;
            _designOptionFactory = designOptionFactory;

            _linkInstanceRepository =
                new LinkInstanceRepository(_revitRepository, _revitLinkType);
            _workset = _linkInstanceRepository.GetWorkset();

            IsLinkLoaded = _linkInstanceRepository.LinkIsLoaded();

            BuildParts = new ObservableCollection<string>(GetPartNames());
            BuildPart = BuildParts.FirstOrDefault();

            LoadLinkDocumentCommand = new RelayCommand(LoadLinkDocument, CanLoadLinkDocument);
            DesignOptions = IsLinkLoaded
                ? new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions())
                : new ObservableCollection<DesignOptionsViewModel>();
        }

        public ElementId Id => _revitLinkType.Id;
        public string Name => _revitLinkType.Name;
        public ICommand LoadLinkDocumentCommand { get; }

        public bool IsLinkLoaded {
            get => _isLinkLoaded;
            set => this.RaiseAndSetIfChanged(ref _isLinkLoaded, value);
        }

        public string BuildPart {
            get => _buildPart;
            set => this.RaiseAndSetIfChanged(ref _buildPart, value);
        }

        public bool HasAreas => _linkInstanceRepository.GetZones().Any();
        public bool HasAreaScheme => _linkInstanceRepository.GetAreaScheme() != null;

        public ObservableCollection<string> BuildParts {
            get => _buildParts;
            set => this.RaiseAndSetIfChanged(ref _buildParts, value);
        }

        public ObservableCollection<DesignOptionsViewModel> DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        public string LinkLoadToolTip {
            get => _linkLoadToolTip;
            set => this.RaiseAndSetIfChanged(ref _linkLoadToolTip, value);
        }

        private IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            if(!_linkInstanceRepository.LinkIsLoaded()) {
                yield break;
            }

            yield return _viewModelFactory.Create(_designOptionFactory.Create(), _linkInstanceRepository);
            foreach(DesignOption designOption in _linkInstanceRepository.GetDesignOptions()) {
                yield return _viewModelFactory.Create(_designOptionFactory.Create(designOption),
                    _linkInstanceRepository);
            }
        }

        public IEnumerable<string> GetPartNames() {
            return _linkInstanceRepository.GetPartNames().Distinct();
        }

        public IEnumerable<string> GetPartNames(IEnumerable<string> paramNames) {
            return _linkInstanceRepository.GetPartNames(paramNames).Distinct();
        }

        public IZoneRepository GetZonesRepository() {
            return _linkInstanceRepository;
        }
        
        public IMassRepository GetMassRepository() {
            return _linkInstanceRepository;
        }

        private void LoadLinkDocument(object param) {
            IsLinkLoaded = _linkInstanceRepository.LoadLinkDocument();
            DesignOptions = new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions());

            BuildParts = new ObservableCollection<string>(GetPartNames());
            BuildPart = BuildParts.FirstOrDefault();
        }

        private bool CanLoadLinkDocument(object param) {
            if(IsLinkLoaded) {
                LinkLoadToolTip = "Данная связь уже загружена.";
                return false;
            }

            if(!_workset.IsOpen) {
                LinkLoadToolTip = $"Откройте рабочий набор \"{_workset.Name}\"."
                                  + Environment.NewLine
                                  + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!";

                return false;
            }

            LinkLoadToolTip = "Загрузить координационный файл";
            return true;
        }
    }
}
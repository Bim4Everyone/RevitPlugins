using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkTypeViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;

        private bool _isLoaded;
        private ObservableCollection<DesignOptionsViewModel> _designOptions;

        public LinkTypeViewModel(RevitLinkType revitLinkType, RevitRepository revitRepository) {
            _revitLinkType = revitLinkType;
            _revitRepository = revitRepository;

            IsLoaded = _revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
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

        public ObservableCollection<DesignOptionsViewModel> DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        private IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            var linkInstance = _revitRepository.GetLinkInstances()
                .FirstOrDefault(item => item.GetTypeId() == _revitLinkType.Id);

            if(linkInstance == null) {
                return Enumerable.Empty<DesignOptionsViewModel>();
            }

            var linkInstanceRepository =
                new LinkInstanceRepository(_revitRepository.Application, linkInstance.GetLinkDocument());

            return linkInstanceRepository.GetDesignOptions()
                .Select(item => new DesignOptionsViewModel(item, linkInstanceRepository));
        }

        private void LoadLinkDocument(object param) {
            if(_revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.InClosedWorkset) {
                Workset workset = _revitRepository.GetWorkset(_revitLinkType);
                TaskDialog.Show("Предупреждение!", $"Откройте рабочий набор \"{workset.Name}\"." 
                                         + Environment.NewLine
                                         + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!");
                
                return;
            }
            
            var loadResult = _revitLinkType.Load();
            IsLoaded = loadResult.LoadResult == LinkLoadResultType.LinkLoaded;
            if(IsLoaded) {
                DesignOptions = new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions());
            }
        }

        private bool CanLoadLinkDocument(object param) {
            return !IsLoaded;
        }
    }
}
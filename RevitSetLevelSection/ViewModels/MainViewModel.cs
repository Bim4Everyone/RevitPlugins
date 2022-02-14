using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        
        private string _errorText;
        private bool _fromRevitParam;
        private LinkInstanceViewModel _linkInstance;
        private ObservableCollection<DesignOptionsViewModel> _designOptions;

        public MainViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            FillParams = new ObservableCollection<BaseViewModel>(GetFillParams());
            LinkInstances = new ObservableCollection<LinkInstanceViewModel>(GetLinkInstances());

            UpdateElementsCommand = new RelayCommand(UpdateElements, CanUpdateElement);
        }

        public ICommand UpdateElementsCommand { get; set; }
        
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool FromRevitParam {
            get => _fromRevitParam;
            set => this.RaiseAndSetIfChanged(ref _fromRevitParam, value);
        }

        public LinkInstanceViewModel LinkInstance {
            get => _linkInstance;
            set {
                this.RaiseAndSetIfChanged(ref _linkInstance, value);
                DesignOptions = new ObservableCollection<DesignOptionsViewModel>(LinkInstance.GetDesignOptions());
            }
        }

        public ObservableCollection<BaseViewModel> FillParams { get; }
        public ObservableCollection<LinkInstanceViewModel> LinkInstances { get; }

        public ObservableCollection<DesignOptionsViewModel> DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        private IEnumerable<BaseViewModel> GetFillParams() {
            //yield return new FillLevelParamViewModel(_revitRepository);
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomArea };
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomAreaRatio };
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomSectionShortName };
        }

        private IEnumerable<LinkInstanceViewModel> GetLinkInstances() {
            return _revitRepository.GetLinkInstances()
                .Select(item => new LinkInstanceViewModel((RevitLinkType) _revitRepository.GetElements(item.GetTypeId()), item));
        }
        
        private void UpdateElements(object param) {
            
        }

        private bool CanUpdateElement(object param) {
            ErrorText = null;
            return true;
        }
    }
}
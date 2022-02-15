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
        private LinkTypeViewModel _linkType;

        public MainViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            LinkTypes = new ObservableCollection<LinkTypeViewModel>(GetLinkTypes());
            FillParams = new ObservableCollection<FillParamViewModel>(GetFillParams());

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

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; }
        public ObservableCollection<FillParamViewModel> FillParams { get; }

        private IEnumerable<FillParamViewModel> GetFillParams() {
            //yield return new FillLevelParamViewModel(_revitRepository);
            yield return new FillMassParamViewModel(_revitRepository) {
                RevitParam = SharedParamsConfig.Instance.BuildingWorksBlock
            };
            yield return new FillMassParamViewModel(_revitRepository) {
                RevitParam = SharedParamsConfig.Instance.BuildingWorksSection
            };
            yield return new FillMassParamViewModel(_revitRepository) {
                RevitParam = SharedParamsConfig.Instance.EconomicFunction
            };
        }

        private IEnumerable<LinkTypeViewModel> GetLinkTypes() {
            return _revitRepository.GetRevitLinkTypes()
                .Select(item =>
                    new LinkTypeViewModel(item, _revitRepository));
        }

        private void UpdateElements(object param) {
            foreach(FillParamViewModel fillParamViewModel in FillParams.Where(item => item.IsEnabled)) {
                fillParamViewModel.UpdateElements(FromRevitParam);
            }
        }

        private bool CanUpdateElement(object param) {
            if(!FromRevitParam && LinkType == null) {
                ErrorText = "Выберите связанный файл с формообразующими.";
                return false;
            }

            if(!LinkType.IsLoaded) {
                ErrorText = "Загрузите выбранный связанный файл.";
                return false;
            }

            if(!FillParams.Any(item => item.IsEnabled)) {
                ErrorText = "Выберите хотя бы один параметр.";
                return false;
            }

            string errorText = FillParams
                .Select(item => item.GetErrorText(FromRevitParam))
                .FirstOrDefault(item => !string.IsNullOrEmpty(item));

            if(!string.IsNullOrEmpty(errorText)) {
                ErrorText = errorText;
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
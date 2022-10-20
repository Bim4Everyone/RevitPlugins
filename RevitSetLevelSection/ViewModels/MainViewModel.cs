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
            
            SetConfig();
        }

        public ICommand UpdateElementsCommand { get; set; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; }
        public ObservableCollection<FillParamViewModel> FillParams { get; }

        private IEnumerable<FillParamViewModel> GetFillParams() {
            yield return new FillLevelParamViewModel(_revitRepository) {
                RevitParam = SharedParamsConfig.Instance.Level
            };
            
            yield return new FillMassParamViewModel(this, _revitRepository) {
                RevitParam = SharedParamsConfig.Instance.BuildingWorksBlock
            };
            
            yield return new FillMassParamViewModel(this, _revitRepository) {
                RevitParam = SharedParamsConfig.Instance.BuildingWorksSection
            };
        }

        private IEnumerable<LinkTypeViewModel> GetLinkTypes() {
            return _revitRepository.GetRevitLinkTypes()
                .Select(item =>
                    new LinkTypeViewModel(item, _revitRepository));
        }

        private void UpdateElements(object param) {
            SaveConfig();
         
            using(var transactionGroup = 
                  _revitRepository.StartTransactionGroup("Установка уровня/секции")) {
                
                foreach(FillParamViewModel fillParamViewModel in FillParams.Where(item => item.IsEnabled)) {
                    fillParamViewModel.UpdateElements();
                }

                transactionGroup.Assimilate();
            }
        }

        private bool CanUpdateElement(object param) {
            if(LinkType == null) {
                ErrorText = "Выберите связанный файл с формообразующими.";
                return false;
            }

            if(LinkType != null && !LinkType.IsLoaded) {
                ErrorText = "Загрузите выбранный связанный файл.";
                return false;
            }

            if(!FillParams.Any(item => item.IsEnabled)) {
                ErrorText = "Выберите хотя бы один параметр.";
                return false;
            }

            string errorText = FillParams
                .Select(item => item.GetErrorText())
                .FirstOrDefault(item => !string.IsNullOrEmpty(item));

            if(!string.IsNullOrEmpty(errorText)) {
                ErrorText = errorText;
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SetConfig() {
            SetLevelSectionSettings settings =
                SetLevelSectionConfig.GetPrintConfig()
                    .GetSettings(_revitRepository.Document);
            if(settings == null) {
                return;
            }

            LinkType = LinkTypes
                           .FirstOrDefault(item => item.Id == settings.LinkFileId)
                       ?? LinkTypes.FirstOrDefault();

            foreach(FillParamViewModel fillParam in FillParams) {
                ParamSettings paramSettings = settings.ParamSettings
                    .FirstOrDefault(item => item.PropertyName.Equals(fillParam.RevitParam.Id));

                if(paramSettings != null) {
                    fillParam.SetParamSettings(paramSettings);
                }
            }
        }

        private void SaveConfig() {
            SetLevelSectionConfig config = SetLevelSectionConfig.GetPrintConfig();
            SetLevelSectionSettings settings = config.GetSettings(_revitRepository.Document);
            if(settings == null) {
                settings = config.AddSettings(_revitRepository.Document);
            }

            settings.LinkFileId = LinkType.Id;
            settings.ParamSettings.Clear();
            foreach(FillParamViewModel fillParam in FillParams) {
                ParamSettings paramSettings = fillParam.GetParamSettings();
                settings.ParamSettings.Add(paramSettings);
            }
            
            config.SaveProjectConfig();
        }
    }
}
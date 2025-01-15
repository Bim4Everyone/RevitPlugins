using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.Revit.DB.Architecture;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;        
        private readonly ILocalizationService _localizationService;
        
        private readonly FamilyCreator _familyCreator;               

        private string _errorText;
        private string _saveProperty;
        private string _extrusionHeight;
        private string _extrusionFamilyName;                        

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,            
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;              

            _familyCreator = new FamilyCreator(revitRepository);            

            ExtrusionHeight = "2200";
            ExtrusionFamilyName = "Машино-места";

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            SelectedRooms = _revitRepository.GetRooms();
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public List<Room> SelectedRooms { get; set; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }
        public string ExtrusionHeight {
            get => _extrusionHeight;
            set => this.RaiseAndSetIfChanged(ref _extrusionHeight, value);
        }
        public string ExtrusionFamilyName {
            get => _extrusionFamilyName;
            set => this.RaiseAndSetIfChanged(ref _extrusionFamilyName, value);
        }                

        private void LoadView() {
            LoadConfig();
        }
        private void AcceptView() {            
            SaveConfig();            
            _familyCreator.CreatingFamilyes(_extrusionFamilyName, _extrusionHeight, SelectedRooms);
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(_extrusionHeight)) {
                ErrorText = "Задайте высоту выдавливания";
                return false;
            }
            if(!double.TryParse(_extrusionHeight, out double result)) {
                ErrorText = "Высота должна быть числом";
                return false;
            }
            if(string.IsNullOrEmpty(_extrusionFamilyName)) {
                ErrorText = "Задайте имя типа помещений";
                return false;
            }           
            ErrorText = null;
            return true;
        }       

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
            ExtrusionHeight = setting?.ExtrusionHeight ?? "2200";            
            ExtrusionFamilyName = setting?.ExtrusionFamilyName ?? "Машино-места";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            setting.SaveProperty = SaveProperty;
            setting.ExtrusionHeight = ExtrusionHeight;
            setting.ExtrusionFamilyName = ExtrusionFamilyName;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

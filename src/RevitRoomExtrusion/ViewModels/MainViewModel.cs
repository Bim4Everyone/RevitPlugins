using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private const string _defaultHeight = "2200"; //Стандартная высота машино-места

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly FamilyCreator _familyCreator;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _extrusionHeight;
        private string _extrusionFamilyName;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            FamilyCreator familyCreator,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _familyCreator = familyCreator;
            _localizationService = localizationService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public List<Room> SelectedRooms { get; set; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
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
            SelectedRooms = _revitRepository.GetSelectedRooms();
            View3D view3d = _revitRepository.GetView3D(_extrusionFamilyName);
            double extrusionHeightDouble = Convert.ToDouble(_extrusionHeight);
            _familyCreator.CreateFamilies(SelectedRooms, view3d, _extrusionFamilyName, extrusionHeightDouble);
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(_extrusionHeight)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ExtrusionHeight");
                return false;
            }
            if(!double.TryParse(_extrusionHeight, out double result)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightError");
                return false;
            }
            if(string.IsNullOrEmpty(_extrusionFamilyName)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.FamilyName");
                return false;
            }
            if(!NamingUtils.IsValidName(_extrusionFamilyName) ||
                _extrusionFamilyName.Any(c => Path.GetInvalidFileNameChars().Contains(c)) ||
                _extrusionFamilyName.Any(c => Path.GetInvalidPathChars().Contains(c))) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.NameError");
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            ExtrusionHeight = setting?.ExtrusionHeight ?? _defaultHeight;
            ExtrusionFamilyName = setting?.ExtrusionFamilyName
                ?? _localizationService.GetLocalizedString("MainViewModel.DefaultFamilyName");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            setting.ExtrusionHeight = ExtrusionHeight;
            setting.ExtrusionFamilyName = ExtrusionFamilyName;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

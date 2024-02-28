using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;

namespace RevitOpeningSlopes.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly OpeningSlopesPlacement _openingSlopesPlacement;
        private string _errorText;
        private string _saveProperty;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository,
            LinesFromOpening linesFromOpening, OpeningSlopesPlacement openingSlopesPlacement) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _linesFromOpening = linesFromOpening;
            _openingSlopesPlacement = openingSlopesPlacement;
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            _openingSlopesPlacement = openingSlopesPlacement;
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            //_revitRepository.GetWindows();
            //_linesFromOpening.CreateLines(_revitRepository.GetWindows());
            _openingSlopesPlacement.OpeningPlacements(_revitRepository.GetWindows());
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = "Введите значение сохраняемого свойства.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

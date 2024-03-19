using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Enums;
using RevitOpeningSlopes.Models.Services;

namespace RevitOpeningSlopes.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly CreationOpeningSlopes _creationOpeningSlopes;
        private string _errorText;
        private SlopeTypeViewModel _selectedSlopeType;


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository,
            LinesFromOpening linesFromOpening, CreationOpeningSlopes creationOpeningSlopes) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository;
            _linesFromOpening = linesFromOpening;
            _creationOpeningSlopes = creationOpeningSlopes;
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            WindowGetterModes = new ObservableCollection<WindowsGetterMode>(
                Enum.GetValues(typeof(WindowsGetterMode)).Cast<WindowsGetterMode>());

            SlopeTypes = new ObservableCollection<SlopeTypeViewModel>(
                _revitRepository.GetSlopeTypes()
                .Select(fs => new SlopeTypeViewModel(fs))
                .OrderBy(fs => fs.Name));
            SelectedSlopeType = SlopeTypes.FirstOrDefault();


            SelectedWindows = _revitRepository.GetSelectedWindows();

        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool _isCheckedSelect;
        public bool IsCheckedSelect {
            get => _isCheckedSelect;
            set => RaiseAndSetIfChanged(ref _isCheckedSelect, value);
        }

        private bool _isCheckedOnView;
        public bool IsCheckedOnView {
            get => _isCheckedOnView;
            set => RaiseAndSetIfChanged(ref _isCheckedOnView, value);
        }

        private bool _isCheckedSelected;
        public bool IsCheckedSelected {
            get => _isCheckedSelected;
            set => RaiseAndSetIfChanged(ref _isCheckedSelected, value);
        }
        public bool IsEnabledAlreadySelected => SelectedWindows?.Count > 0;
        public ObservableCollection<WindowsGetterMode> WindowGetterModes { get; }
        public WindowsGetterMode SelectedWindowGetterMode { get; set; }


        public ICollection<FamilyInstance> SelectedWindows { get; }

        public ObservableCollection<SlopeTypeViewModel> SlopeTypes { get; }

        public SlopeTypeViewModel SelectedSlopeType {
            get => _selectedSlopeType;
            set {
                RaiseAndSetIfChanged(ref _selectedSlopeType, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }


        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
            _creationOpeningSlopes.CreateSlopes(_pluginConfig, out string error);
        }

        private bool CanAcceptView() {
            return string.IsNullOrWhiteSpace(ErrorText);
        }

        private void LoadConfig() {
            SelectedWindowGetterMode = _pluginConfig.WindowsGetterMode;
            if(SelectedWindows.Count > 0) {
                switch(SelectedWindowGetterMode) {
                    case WindowsGetterMode.AlreadySelectedWindows:
                        IsCheckedSelected = true;
                        break;
                    case WindowsGetterMode.ManuallySelectedWindows:
                        IsCheckedSelect = true;
                        break;
                    case WindowsGetterMode.WindowsOnActiveView:
                        IsCheckedOnView = true;
                        break;
                    default:
                        IsCheckedSelect = true;
                        break;
                }
            } else {
                switch(SelectedWindowGetterMode) {
                    case WindowsGetterMode.AlreadySelectedWindows:
                        IsCheckedSelect = true;
                        break;
                    case WindowsGetterMode.ManuallySelectedWindows:
                        IsCheckedSelect = true;
                        break;
                    case WindowsGetterMode.WindowsOnActiveView:
                        IsCheckedOnView = true;
                        break;
                    default:
                        IsCheckedSelect = true;
                        break;
                }
            }
            SelectedSlopeType = SlopeTypes.FirstOrDefault(slwm => slwm.SlopeTypeId == _pluginConfig.SlopeTypeId);
        }

        private void SaveConfig() {
            if(IsCheckedSelected) {
                SelectedWindowGetterMode = WindowsGetterMode.AlreadySelectedWindows;
            } else if(IsCheckedSelect) {
                SelectedWindowGetterMode = WindowsGetterMode.ManuallySelectedWindows;
            } else {
                SelectedWindowGetterMode = WindowsGetterMode.WindowsOnActiveView;
            }
            _pluginConfig.SlopeTypeId = SelectedSlopeType.SlopeTypeId;
            _pluginConfig.WindowsGetterMode = SelectedWindowGetterMode;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

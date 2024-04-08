using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Enums;
using RevitOpeningSlopes.Models.Services;

namespace RevitOpeningSlopes.ViewModels {
    internal class MainViewModel : BaseViewModel, IDataErrorInfo {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly CreationOpeningSlopes _creationOpeningSlopes;
        private readonly IMessageBoxService _messageBoxService;


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository,
            CreationOpeningSlopes creationOpeningSlopes, IMessageBoxService messageBoxService) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository;
            _creationOpeningSlopes = creationOpeningSlopes;
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            WindowGetterModes = new ObservableCollection<WindowsGetterMode>(
                Enum.GetValues(typeof(WindowsGetterMode)).Cast<WindowsGetterMode>());

            SlopeTypes = new ObservableCollection<SlopeTypeViewModel>(
                _revitRepository.GetSlopeTypes()
                .Select(fs => new SlopeTypeViewModel(fs))
                .OrderBy(fs => fs.Name));
            //SelectedSlopeType = SlopeTypes.FirstOrDefault();


            SelectedWindows = _revitRepository.GetSelectedWindows();
            WindowsOnActiveView = _revitRepository.GetWindowsOnActiveView();
            //SlopeFrontOffset = "0";
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public IMessageBoxService MessageBoxService => _messageBoxService;
        public string ErrorText => Error;
        public string Error => GetType()
            .GetProperties()
            .Select(prop => this[prop.Name])
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error)) ?? string.Empty;

        public string this[string columnName] {
            get {
                var error = string.Empty;
                switch(columnName) {
                    case nameof(SelectedSlopeType): {
                        if(SelectedSlopeType is null) {
                            error = "Задайте тип создаваемого откоса";
                        }
                        break;
                    }
                    case nameof(SlopeFrontOffset): {
                        if(!double.TryParse(SlopeFrontOffset, out double result)) {
                            error = "Смещение не должно содержать символов или букв";
                        }
                        break;
                    }
                }
                return error;
            }
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
        public ICollection<FamilyInstance> WindowsOnActiveView { get; }
        public ObservableCollection<SlopeTypeViewModel> SlopeTypes { get; }

        private string _slopeFrontOffset;
        public string SlopeFrontOffset {
            get => _slopeFrontOffset;
            set {
                RaiseAndSetIfChanged(ref _slopeFrontOffset, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }


        private SlopeTypeViewModel _selectedSlopeType;
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
            if(!string.IsNullOrEmpty(error)) {
                error = $"Не удалось обработать следующие окна:\n\n" + error;
                _messageBoxService.Show(
                    error, "BIM", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
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
            if(string.IsNullOrEmpty(_pluginConfig.SlopeFrontOffset)) {
                SlopeFrontOffset = "0";
            } else {
                SlopeFrontOffset = _pluginConfig.SlopeFrontOffset;
            }

            if(_pluginConfig.SlopeTypeId == ElementId.InvalidElementId) {
                SelectedSlopeType = SlopeTypes.FirstOrDefault();
            } else {
                SelectedSlopeType = SlopeTypes.FirstOrDefault(slwp => slwp.SlopeTypeId == _pluginConfig.SlopeTypeId);
            }
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
            _pluginConfig.SlopeFrontOffset = SlopeFrontOffset;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

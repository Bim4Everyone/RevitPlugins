using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Services;

namespace RevitOpeningSlopes.ViewModels {
    internal class MainViewModel : BaseViewModel, IDataErrorInfo {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly CreationOpeningSlopes _creationOpeningSlopes;
        private readonly IMessageBoxService _messageBoxService;

        public MainViewModel(PluginConfig pluginConfig,
            RevitRepository revitRepository,
            CreationOpeningSlopes creationOpeningSlopes,
            AlreadySelectedWindowsGetter alreadySelectedWindowsGetter,
            ManuallySelectedWindowsGetter manuallySelectedWindowsGetter,
            OnActiveViewWindowsGetter onActiveViewWindowsGetter,
            IMessageBoxService messageBoxService
            ) {
            _revitRepository = revitRepository;
            _creationOpeningSlopes = creationOpeningSlopes;
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
            _messageBoxService = messageBoxService
                ?? throw new ArgumentNullException(nameof(messageBoxService));

            SlopeTypes = new ObservableCollection<SlopeTypeViewModel>(
                _revitRepository.GetSlopeTypes()
                .Select(fs => new SlopeTypeViewModel(fs))
                .OrderBy(fs => fs.Name));

            WindowsGetters = new ObservableCollection<IWindowsGetter>() {
                alreadySelectedWindowsGetter,
                manuallySelectedWindowsGetter,
                onActiveViewWindowsGetter
            };

            SelectedWindowsGetter = WindowsGetters.First();

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadViewCommand = RelayCommand.Create(LoadView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public IMessageBoxService MessageBoxService => _messageBoxService;
        public ObservableCollection<IWindowsGetter> WindowsGetters { get; }

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

        private IWindowsGetter _selectedWindowsGetter;
        public IWindowsGetter SelectedWindowsGetter {
            get => _selectedWindowsGetter;
            set => RaiseAndSetIfChanged(ref _selectedWindowsGetter, value);
        }

        public ICollection<FamilyInstance> SelectedOpenings { get; }
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
            string errorMsg;
            ICollection<FamilyInstance> openings = SelectedWindowsGetter.GetOpenings();
            using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
                progressDialogService.MaxValue = openings.Count;
                progressDialogService.StepValue = progressDialogService.MaxValue / 10;
                progressDialogService.DisplayTitleFormat = "Обработка оконных проемов... [{0}]\\[{1}]";
                var progress = progressDialogService.CreateProgress();
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                _creationOpeningSlopes.CreateSlopes(_pluginConfig, openings, out string error, progress, ct);
                errorMsg = error;
            }
            ShowMessageBoxError(errorMsg);
        }

        private bool CanAcceptView() {
            return string.IsNullOrWhiteSpace(ErrorText);
        }

        private void LoadConfig() {

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

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.SlopeTypeId = SelectedSlopeType.SlopeTypeId;
            _pluginConfig.SlopeFrontOffset = SlopeFrontOffset;
            _pluginConfig.SaveProjectConfig();
        }

        private void ShowMessageBoxError(string error) {
            if(!string.IsNullOrEmpty(error)) {
                error = $"Не удалось обработать следующие окна:\n\n" + error;
                _messageBoxService.Show(
                    error, "BIM", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }
    }
}

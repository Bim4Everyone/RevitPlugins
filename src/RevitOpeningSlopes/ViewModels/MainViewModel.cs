using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Services;

namespace RevitOpeningSlopes.ViewModels {
    internal class MainViewModel : BaseViewModel {
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

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _creationOpeningSlopes = creationOpeningSlopes
                ?? throw new ArgumentNullException(nameof(creationOpeningSlopes));
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
            _messageBoxService = messageBoxService
                ?? throw new ArgumentNullException(nameof(messageBoxService));

            WindowsGetters = new ObservableCollection<IWindowsGetter>() {
                alreadySelectedWindowsGetter,
                manuallySelectedWindowsGetter,
                onActiveViewWindowsGetter
            };
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadViewCommand = RelayCommand.Create(LoadView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public IMessageBoxService MessageBoxService => _messageBoxService;
        public ObservableCollection<IWindowsGetter> WindowsGetters { get; }

        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private IWindowsGetter _selectedWindowsGetter;
        public IWindowsGetter SelectedWindowsGetter {
            get => _selectedWindowsGetter;
            set => RaiseAndSetIfChanged(ref _selectedWindowsGetter, value);
        }

        private string _slopeFrontOffset;
        public string SlopeFrontOffset {
            get => _slopeFrontOffset;
            set {
                RaiseAndSetIfChanged(ref _slopeFrontOffset, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }

        private ObservableCollection<SlopeTypeViewModel> _slopeTypes;
        public ObservableCollection<SlopeTypeViewModel> SlopeTypes {
            get => _slopeTypes;
            set => this.RaiseAndSetIfChanged(ref _slopeTypes, value);
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
            if(SelectedSlopeType is null) {
                ErrorText = "Задайте тип создаваемого откоса";
                return false;
            }
            if(!double.TryParse(SlopeFrontOffset, out double result)) {
                ErrorText = "Смещение не должно содержать символов или букв";
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            SelectedWindowsGetter = WindowsGetters.First();
            SlopeTypes = new ObservableCollection<SlopeTypeViewModel>(
                _revitRepository.GetSlopeTypes()
                .Select(fs => new SlopeTypeViewModel(fs))
                .OrderBy(fs => fs.Name));

            SlopeFrontOffset = _pluginConfig.SlopeFrontOffset.GetValueOrDefault(0).ToString();

            if(_pluginConfig.SlopeTypeId == ElementId.InvalidElementId) {
                SelectedSlopeType = SlopeTypes.FirstOrDefault();
            } else {
                SelectedSlopeType = SlopeTypes.FirstOrDefault(slwp => slwp.SlopeTypeId == _pluginConfig.SlopeTypeId);
            }

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.SlopeTypeId = SelectedSlopeType.SlopeTypeId;
            _pluginConfig.SlopeFrontOffset = double.Parse(SlopeFrontOffset);
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

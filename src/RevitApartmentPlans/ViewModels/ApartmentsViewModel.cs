using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels;
internal class ApartmentsViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;

    public ApartmentsViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        IMessageBoxService messageBoxService,
        ILocalizationService localization) {

        _pluginConfig = pluginConfig ?? throw new System.ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        ShowApartmentCommand = RelayCommand.Create<ApartmentViewModel>(ShowApartment, CanShowApartment);
        ShowWarningCommand = RelayCommand.Create(ShowWarning);
        Apartments = [];
        Parameters = new ObservableCollection<ParamViewModel>(_revitRepository
            .GetRoomGroupingParameters()
            .OrderBy(x => x.Name)
            .Select(p => new ParamViewModel(p)));
        LoadConfig();
    }


    public ICommand ShowApartmentCommand { get; }

    public ICommand ShowWarningCommand { get; }

    public IMessageBoxService MessageBoxService { get; }

    private ParamViewModel _selectedParam;
    public ParamViewModel SelectedParam {
        get => _selectedParam;
        set {
            RaiseAndSetIfChanged(ref _selectedParam, value);
            UpdateApartments(value?.Name, ProcessLinks);
        }
    }


    public ObservableCollection<ApartmentViewModel> Apartments { get; }

    public ObservableCollection<ParamViewModel> Parameters { get; }


    private bool _processLinks;
    public bool ProcessLinks {
        get => _processLinks;
        set {
            RaiseAndSetIfChanged(ref _processLinks, value);
            OnPropertyChanged(nameof(ShowWarningButton));
            UpdateApartments(SelectedParam?.Name, value);
        }
    }

#if REVIT_2023_OR_LESS
    /// <summary>
    /// Кнопка предупреждений о неполном/нестабильном функционале в плагине в версиях ниже 2024
    /// </summary>
    public bool ShowWarningButton => ProcessLinks;
#else
    public bool ShowWarningButton => false;
#endif


    private void UpdateApartments(string paramName, bool processLinks) {
        Apartments.Clear();

        if(!string.IsNullOrWhiteSpace(paramName)) {
            var apartments = _revitRepository.GetApartments(paramName, processLinks);
            foreach(var item in apartments) {
                Apartments.Add(new ApartmentViewModel(item));
            }
        }
    }

    private void LoadConfig() {
        if(_pluginConfig == null) { throw new ArgumentNullException(nameof(_pluginConfig)); }
        if(_revitRepository == null) { throw new ArgumentNullException(nameof(_revitRepository)); }
        if(Parameters == null) { throw new ArgumentNullException(nameof(Parameters)); }

        var settings = _pluginConfig.GetSettings(_revitRepository.Document);
        string paramName = settings?.ParamName ?? string.Empty;
        SelectedParam = Parameters.FirstOrDefault(p => p.Name == paramName);
        ProcessLinks = settings?.ProcessLinks ?? false;
    }

    private void ShowApartment(ApartmentViewModel apartmentViewModel) {
        _revitRepository.ShowApartment(apartmentViewModel.GetApartment());
    }

    private bool CanShowApartment(ApartmentViewModel apartmentViewModel) {
        return apartmentViewModel != null;
    }


    private void ShowWarning() {
        string msg;
#if REVIT_2022_OR_LESS
        msg = _localization.GetLocalizedString("Warnings.Revit2022");
#elif REVIT_2023
        msg = _localization.GetLocalizedString("Warnings.Revit2023");
#else
        msg = string.Empty;
#endif
        if(!string.IsNullOrWhiteSpace(msg)) {
            MessageBoxService.Show(msg,
                _localization.GetLocalizedString("Warning"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }
}

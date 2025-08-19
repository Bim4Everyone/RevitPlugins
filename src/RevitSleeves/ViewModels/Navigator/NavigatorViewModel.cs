using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Navigator;

namespace RevitSleeves.ViewModels.Navigator;
internal class NavigatorViewModel : BaseViewModel {
    private readonly ISleeveStatusFinder _sleeveStatusFinder;
    private readonly ILocalizationService _localizationService;
    private readonly SleevePlacementSettingsConfig _config;
    private readonly RevitRepository _revitRepository;

    public NavigatorViewModel(
        IProgressDialogFactory progressFactory,
        ISleeveStatusFinder sleeveStatusFinder,
        IMessageBoxService messageBoxService,
        ILocalizationService localizationService,
        SleevePlacementSettingsConfig config,
        RevitRepository revitRepository) {

        ProgressDialogFactory = progressFactory
            ?? throw new System.ArgumentNullException(nameof(progressFactory));
        _sleeveStatusFinder = sleeveStatusFinder
            ?? throw new System.ArgumentNullException(nameof(sleeveStatusFinder));
        MessageBoxService = messageBoxService
            ?? throw new System.ArgumentNullException(nameof(messageBoxService));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _config = config
            ?? throw new System.ArgumentNullException(nameof(config));
        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));

        SettingsNameHeader = string.Format(
            _localizationService.GetLocalizedString("Navigator.SettingsNamePrefix"), _config.Name);
        Sleeves = [];
        LoadViewCommand = RelayCommand.Create(LoadView);
        SelectSleeveCommand = RelayCommand.Create<SleeveViewModel>(SelectSleeve, CanSelectSleeve);
    }


    public IProgressDialogFactory ProgressDialogFactory { get; }

    public IMessageBoxService MessageBoxService { get; }

    public ICommand SelectSleeveCommand { get; }

    public ICommand LoadViewCommand { get; }

    public string SettingsNameHeader { get; }

    public ObservableCollection<SleeveViewModel> Sleeves { get; }


    private void SelectSleeve(SleeveViewModel sleeve) {
        _revitRepository
            .GetClashRevitRepository()
            .SelectAndShowElement(
            [new ElementModel(sleeve.GetSleeve().GetFamilyInstance())], 2, _revitRepository.GetSleeve3dView());
    }

    private bool CanSelectSleeve(SleeveViewModel sleeve) {
        return sleeve is not null;
    }

    private void LoadView() {
        var sleeves = _revitRepository.GetSleeves();
        using var progressService = ProgressDialogFactory.CreateDialog();
        progressService.MaxValue = sleeves.Count;
        progressService.StepValue = 50;
        var progress = progressService.CreateProgress();
        var ct = progressService.CreateCancellationToken();
        progressService.Show();

        int i = 0;
        foreach(var sleeve in sleeves) {
            ct.ThrowIfCancellationRequested();
            progress.Report(++i);
            sleeve.Status = _sleeveStatusFinder.GetStatus(sleeve);
            Sleeves.Add(new SleeveViewModel(_revitRepository, _localizationService, sleeve));
        }
    }
}

using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Services;

namespace RevitApartmentPlans.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IViewPlanCreationService _viewPlanCreationService;
    private readonly ILengthConverter _lengthConverter;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Минимальное значение отступа подрезки контура квартиры в мм
    /// </summary>
    private const double _minOffsetMm = 0;
    /// <summary>
    /// Максимальное значение отступа подрезки контура квартиры в мм.
    /// Принято с целью разумно ограничить ввод пользователя.
    /// </summary>
    private const double _maxOffsetMm = 2000;
    /// <summary>
    /// Значение отступа подрезки контура квартиры в мм по умолчанию. 
    /// Принято исходя из наиболее часто встречающейся толщины межквартирных стен в 200 мм.
    /// </summary>
    private const double _defaultOffsetMm = 200;


    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ViewTemplatesViewModel viewTemplatesViewModel,
        ApartmentsViewModel apartmentsViewModel,
        IViewPlanCreationService viewPlanCreationService,
        ILengthConverter lengthConverter,
        IProgressDialogFactory progressDialogFactory,
        ILocalizationService localization) {

        _pluginConfig = pluginConfig
            ?? throw new System.ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        ViewTemplatesViewModel = viewTemplatesViewModel
            ?? throw new System.ArgumentNullException(nameof(viewTemplatesViewModel));
        ApartmentsViewModel = apartmentsViewModel
            ?? throw new System.ArgumentNullException(nameof(apartmentsViewModel));
        _viewPlanCreationService = viewPlanCreationService
            ?? throw new System.ArgumentNullException(nameof(viewPlanCreationService));
        _lengthConverter = lengthConverter
            ?? throw new System.ArgumentNullException(nameof(lengthConverter));
        ProgressDialogFactory = progressDialogFactory
            ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
        _localization = localization
            ?? throw new System.ArgumentNullException(nameof(localization));
        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    private string _errorText;
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    public ViewTemplatesViewModel ViewTemplatesViewModel { get; }

    public ApartmentsViewModel ApartmentsViewModel { get; }

    private double _offsetMm;
    public double OffsetMm {
        get => _offsetMm;
        set => RaiseAndSetIfChanged(ref _offsetMm, value);
    }

    private bool _copyDetail;
    public bool CopyDetail {
        get => _copyDetail;
        set => RaiseAndSetIfChanged(ref _copyDetail, value);
    }


    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
        var selectedApartments = ApartmentsViewModel.Apartments
                .Where(a => a.IsSelected)
                .Select(vm => vm.GetApartment())
                .ToArray();
        var selectedTemplates = ViewTemplatesViewModel.ViewTemplates
                .Select(t => t.GetTemplate())
                .ToArray();
        using var progressDialogService = ProgressDialogFactory.CreateDialog();
        progressDialogService.StepValue = 1;
        progressDialogService.DisplayTitleFormat = _localization.GetLocalizedString("Progress.Title");
        var progress = progressDialogService.CreateProgress();
        progressDialogService.MaxValue = selectedApartments.Length;
        var ct = progressDialogService.CreateCancellationToken();
        progressDialogService.Show();

        if(CopyDetail) {
            _viewPlanCreationService.CreateViews(
                selectedApartments,
                selectedTemplates,
                _lengthConverter.ConvertToInternal(OffsetMm),
                _revitRepository.GetActiveViewPlan(),
                progress,
                ct);
        } else {
            _viewPlanCreationService.CreateViews(
                selectedApartments,
                selectedTemplates,
                _lengthConverter.ConvertToInternal(OffsetMm),
                progress,
                ct);
        }

    }

    private bool CanAcceptView() {
        if(OffsetMm < _minOffsetMm) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.OffsetLessThan", _minOffsetMm);
            return false;
        }
        if(OffsetMm > _maxOffsetMm) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.OffsetGreaterThan", _maxOffsetMm);
            return false;
        }
        if(ApartmentsViewModel?.SelectedParam is null) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.ParameterNotSet");
            return false;
        }
        if(ViewTemplatesViewModel?.ViewTemplates?.Count() == 0) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.AddViewTemplate");
            return false;
        }
        if(ApartmentsViewModel?.Apartments?.Where(a => a.IsSelected).Count() == 0) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.SelectApartments");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        OffsetMm = setting?.OffsetMm ?? _defaultOffsetMm;
        CopyDetail = setting?.CopyDetail ?? false;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.OffsetMm = OffsetMm;
        setting.ParamName = ApartmentsViewModel?.SelectedParam?.Name ?? string.Empty;
        setting.ViewTemplates = ViewTemplatesViewModel?.ViewTemplates?.Select(t => t.GetTemplate().Id).ToArray();
        setting.CopyDetail = CopyDetail;
        setting.ProcessLinks = ApartmentsViewModel?.ProcessLinks ?? false;
        _pluginConfig.SaveProjectConfig();
    }
}

using System.ComponentModel.DataAnnotations;
using System.Linq;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserDispatcherSettingsVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
    private string _dispatcherGroupingSecondTemp = "_Группа видов 2";

    public UserDispatcherSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                                 ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _revitRepository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public string DispatcherGroupingFirst { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string DispatcherGroupingFirstTemp {
        get => _dispatcherGroupingFirstTemp;
        set {
            RaiseAndSetIfChanged(ref _dispatcherGroupingFirstTemp, value);
            ValidateProperty(value);
        }
    }
    public string DispatcherGroupingSecond { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string DispatcherGroupingSecondTemp {
        get => _dispatcherGroupingSecondTemp;
        set {
            RaiseAndSetIfChanged(ref _dispatcherGroupingSecondTemp, value);
            ValidateProperty(value);
        }
    }


    public bool CheckSettings() {
        // Пытаемся проверить виды
        if(_revitRepository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            SetError(_localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid"));
            return false;
        }
        if(_revitRepository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            SetError(_localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid"));
            return false;
        }

        // Пытаемся проверить спеки
        if(_revitRepository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            SetError(_localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid"));
            return false;
        }
        if(_revitRepository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            SetError(_localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid"));
            return false;
        }
        return true;
    }


    /// <summary>
    /// Записывает ошибку для отображения в GUI, указывая наименование вкладки, на которой произошла ошибка
    /// </summary>
    /// <param name="error"></param>
    private void SetError(string error) {
        _viewModel.ErrorText = string.Format(
            "{0} - {1}",
            _localizationService.GetLocalizedString("MainWindow.ProjectParameters"),
            error);
    }
}

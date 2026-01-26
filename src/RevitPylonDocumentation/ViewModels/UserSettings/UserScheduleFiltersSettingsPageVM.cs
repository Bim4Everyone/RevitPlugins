using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserScheduleFiltersSettingsPageVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<ScheduleFilterParamHelper> _paramsForScheduleFiltersTemp = [
        new ScheduleFilterParamHelper("обр_ФОП_Форма_номер", ""),
        new ScheduleFilterParamHelper("обр_ФОП_Изделие_Марка", ""),
        new ScheduleFilterParamHelper("обр_ФОП_Фильтрация 1", ""),
        new ScheduleFilterParamHelper("обр_ФОП_Группа КР", ""),
        new ScheduleFilterParamHelper("обр_ФОП_Раздел проекта", "обр_ФОП_Раздел проекта"),
        new ScheduleFilterParamHelper("обр_ФОП_Орг. уровень", "обр_ФОП_Орг. уровень"),
        new ScheduleFilterParamHelper("обр_Метка основы_универсальная", "Марка"),
        new ScheduleFilterParamHelper("обр_Метка основы", "Марка"),
        new ScheduleFilterParamHelper("Марка", "Марка")
    ];

    public UserScheduleFiltersSettingsPageVM(MainViewModel mainViewModel, ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }


    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; } = [];
    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFiltersTemp {
        get => _paramsForScheduleFiltersTemp;
        set {
            RaiseAndSetIfChanged(ref _paramsForScheduleFiltersTemp, value);
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Добавляет новое имя параметра фильтра спецификаций в настройках плагина
    /// </summary>
    public void AddScheduleFilterParam() {
        ParamsForScheduleFilters.Add(
            new ScheduleFilterParamHelper(
                _localizationService.GetLocalizedString("VM.WriteName"),
                _localizationService.GetLocalizedString("VM.WriteName")));
        _viewModel.SettingsChanged();
    }

    /// <summary>
    /// Удаляет выбранное имя параметра фильтра спецификаций в настройках плагина
    /// </summary>
    public void DeleteScheduleFilterParam() {
        List<ScheduleFilterParamHelper> forDel = [];

        foreach(ScheduleFilterParamHelper param in ParamsForScheduleFilters) {
            if(param.IsCheck) {
                forDel.Add(param);
            }
        }

        foreach(ScheduleFilterParamHelper param in forDel) {
            ParamsForScheduleFilters.Remove(param);
        }
        _viewModel.SettingsChanged();
    }

    /// <summary>
    /// Определяет можно ли удалить выбранное имя параметра фильтра спецификаций в настройках плагина
    /// True, если выбрана штриховка в списке штриховок в настройках плагина
    /// </summary> 
    public bool CanChangeScheduleFilterParam() {
        foreach(var param in ParamsForScheduleFilters) {
            if(param.IsCheck) { return true; }
        }
        return false;
    }
}

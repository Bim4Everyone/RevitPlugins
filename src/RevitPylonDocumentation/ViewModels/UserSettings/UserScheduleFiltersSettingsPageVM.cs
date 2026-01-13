using System.Collections.ObjectModel;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserScheduleFiltersSettingsPageVM : ValidatableViewModel {

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

    public UserScheduleFiltersSettingsPageVM(MainViewModel mainViewModel) {
        ViewModel = mainViewModel;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }

    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; } = [];
    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFiltersTemp {
        get => _paramsForScheduleFiltersTemp;
        set {
            RaiseAndSetIfChanged(ref _paramsForScheduleFiltersTemp, value);
            ValidateProperty(value);
        }
    }

    public void ApplyScheduleFiltersSettings() {
        ParamsForScheduleFilters = ParamsForScheduleFiltersTemp;
    }

    public UserScheduleFiltersSettings GetSettings() {
        var settings = new UserScheduleFiltersSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserScheduleFiltersSettings);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null && vmProp.CanRead && prop.CanWrite) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }
}

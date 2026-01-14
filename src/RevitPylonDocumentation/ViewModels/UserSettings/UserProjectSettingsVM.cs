using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserProjectSettingsVM : ValidatableViewModel {
    private readonly ILocalizationService _localizationService;

    private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
    private string _dispatcherGroupingSecondTemp = "_Группа видов 2";

    private string _dimensionTypeNameTemp = "я_Основной_Плагин_2.5 мм";
    private string _spotDimensionTypeNameTemp = "Стрелка_Проектная_Верх";

    private string _skeletonTagTypeNameTemp = "Изделие_Марка - Полка 7";
    private string _rebarTagTypeWithSerifNameTemp = "Поз., Диаметр / Шаг - Полка 10, Засечка";
    private string _rebarTagTypeWithStepNameTemp = "Поз., Диаметр / Шаг - Полка 10";
    private string _rebarTagTypeWithCommentNameTemp = "Поз., Диаметр / Комментарий - Полка 10";
    private string _universalTagTypeNameTemp = "Без засечки";

    private string _breakLineTypeNameTemp = "Линейный обрыв";
    private string _concretingJointTypeNameTemp = "3 мм_М 20";

    private DimensionType _selectedDimensionType;
    private FamilySymbol _selectedSkeletonTagType;
    private FamilySymbol _selectedRebarTagTypeWithSerif;
    private FamilySymbol _selectedRebarTagTypeWithStep;
    private FamilySymbol _selectedRebarTagTypeWithComment;
    private FamilySymbol _selectedUniversalTagType;

    private FamilySymbol _selectedBreakLineType;
    private FamilySymbol _selectedConcretingJointType;

    private SpotDimensionType _selectedSpotDimensionType;

    public UserProjectSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                                 ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        Repository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }


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

    public string DimensionTypeName { get; set; }
    public string DimensionTypeNameTemp {
        get => _dimensionTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _dimensionTypeNameTemp, value);
    }

    public string SpotDimensionTypeName { get; set; }
    public string SpotDimensionTypeNameTemp {
        get => _spotDimensionTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _spotDimensionTypeNameTemp, value);
    }

    public string SkeletonTagTypeName { get; set; }
    public string SkeletonTagTypeNameTemp {
        get => _skeletonTagTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _skeletonTagTypeNameTemp, value);
    }

    public string RebarTagTypeWithSerifName { get; set; }
    public string RebarTagTypeWithSerifNameTemp {
        get => _rebarTagTypeWithSerifNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithSerifNameTemp, value);
    }

    public string RebarTagTypeWithStepName { get; set; }
    public string RebarTagTypeWithStepNameTemp {
        get => _rebarTagTypeWithStepNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithStepNameTemp, value);
    }

    public string RebarTagTypeWithCommentName { get; set; }
    public string RebarTagTypeWithCommentNameTemp {
        get => _rebarTagTypeWithCommentNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithCommentNameTemp, value);
    }

    public string UniversalTagTypeName { get; set; }
    public string UniversalTagTypeNameTemp {
        get => _universalTagTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _universalTagTypeNameTemp, value);
    }

    public string BreakLineTypeName { get; set; }
    public string BreakLineTypeNameTemp {
        get => _breakLineTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _breakLineTypeNameTemp, value);
    }

    public string ConcretingJointTypeName { get; set; }
    public string ConcretingJointTypeNameTemp {
        get => _concretingJointTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _concretingJointTypeNameTemp, value);
    }

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    [Required]
    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedDimensionType, value);
            ViewModel.ProjectSettings.DimensionTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки для обозначения каркаса
    /// </summary>
    [Required]
    public FamilySymbol SelectedSkeletonTagType {
        get => _selectedSkeletonTagType;
        set {
            RaiseAndSetIfChanged(ref _selectedSkeletonTagType, value);
            ViewModel.ProjectSettings.SkeletonTagTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с засечкой
    /// </summary>
    [Required]
    public FamilySymbol SelectedRebarTagTypeWithSerif {
        get => _selectedRebarTagTypeWithSerif;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithSerif, value);
            ViewModel.ProjectSettings.RebarTagTypeWithSerifNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с шагом
    /// </summary>
    [Required]
    public FamilySymbol SelectedRebarTagTypeWithStep {
        get => _selectedRebarTagTypeWithStep;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithStep, value);
            ViewModel.ProjectSettings.RebarTagTypeWithStepNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с количеством
    /// </summary>
    [Required]
    public FamilySymbol SelectedRebarTagTypeWithComment {
        get => _selectedRebarTagTypeWithComment;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithComment, value);
            ViewModel.ProjectSettings.RebarTagTypeWithCommentNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер аннотации рабочего шва бетонирования
    /// </summary>
    [Required]
    public FamilySymbol SelectedUniversalTagType {
        get => _selectedUniversalTagType;
        set {
            RaiseAndSetIfChanged(ref _selectedUniversalTagType, value);
            ViewModel.ProjectSettings.UniversalTagTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с количеством
    /// </summary>
    [Required]
    public FamilySymbol SelectedBreakLineType {
        get => _selectedBreakLineType;
        set {
            RaiseAndSetIfChanged(ref _selectedBreakLineType, value);
            ViewModel.ProjectSettings.BreakLineTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер аннотации рабочего шва бетонирования
    /// </summary>
    [Required]
    public FamilySymbol SelectedConcretingJointType {
        get => _selectedConcretingJointType;
        set {
            RaiseAndSetIfChanged(ref _selectedConcretingJointType, value);
            ViewModel.ProjectSettings.ConcretingJointTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    [Required]
    public SpotDimensionType SelectedSpotDimensionType {
        get => _selectedSpotDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedSpotDimensionType, value);
            ViewModel.ProjectSettings.SpotDimensionTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }


    public void ApplyProjectSettings() {
        DispatcherGroupingFirst = DispatcherGroupingFirstTemp;
        DispatcherGroupingSecond = DispatcherGroupingSecondTemp;

        DimensionTypeName = DimensionTypeNameTemp;
        SpotDimensionTypeName = SpotDimensionTypeNameTemp;

        SkeletonTagTypeName = SkeletonTagTypeNameTemp;
        RebarTagTypeWithSerifName = RebarTagTypeWithSerifNameTemp;
        RebarTagTypeWithStepName = RebarTagTypeWithStepNameTemp;
        RebarTagTypeWithCommentName = RebarTagTypeWithCommentNameTemp;
        UniversalTagTypeName = UniversalTagTypeNameTemp;
        BreakLineTypeName = BreakLineTypeNameTemp;
        ConcretingJointTypeName = ConcretingJointTypeNameTemp;
    }

    public void CheckProjectSettings() {
        // Пытаемся проверить виды
        if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
        }
        if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid");
        }

        // Пытаемся проверить спеки
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
        }
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid");
        }
    }

    public UserProjectSettings GetSettings() {
        var settings = new UserProjectSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserProjectSettings);

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

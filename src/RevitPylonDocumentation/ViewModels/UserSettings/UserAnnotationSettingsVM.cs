using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserAnnotationSettingsVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _dimensionTypeNameTemp = "я_Основной_Плагин_2.5 мм";
    private string _spotDimensionTypeNameTemp = "Стрелка_Проектная_Верх";

    private string _skeletonTagTypeNameTemp = "Изделие_Марка - Полка 7";
    private string _rebarTagTypeWithSerifNameTemp = "Поз., Диаметр / Шаг - Полка 10, Засечка";
    private string _rebarTagTypeWithStepNameTemp = "Поз., Диаметр / Шаг - Полка 10";
    private string _rebarTagTypeWithCommentNameTemp = "Поз., Диаметр / Комментарий - Полка 10";
    private string _universalTagTypeNameTemp = "Без засечки";

    private string _breakLineTypeNameTemp = "Линейный обрыв";
    private string _concretingJointTypeNameTemp = "3 мм_М 20";

    private bool _dimensionGrouping = true;

    private DimensionType _selectedDimensionType;
    private FamilySymbol _selectedSkeletonTagType;
    private FamilySymbol _selectedRebarTagTypeWithSerif;
    private FamilySymbol _selectedRebarTagTypeWithStep;
    private FamilySymbol _selectedRebarTagTypeWithComment;
    private FamilySymbol _selectedUniversalTagType;

    private FamilySymbol _selectedBreakLineType;
    private FamilySymbol _selectedConcretingJointType;

    private SpotDimensionType _selectedSpotDimensionType;

    public UserAnnotationSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                                    ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _revitRepository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
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

    public bool DimensionGrouping {
        get => _dimensionGrouping;
        set => RaiseAndSetIfChanged(ref _dimensionGrouping, value);
    }

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    [Required]
    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedDimensionType, value);
            _viewModel.AnnotationSettings.DimensionTypeNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.SkeletonTagTypeNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.RebarTagTypeWithSerifNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.RebarTagTypeWithStepNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.RebarTagTypeWithCommentNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.UniversalTagTypeNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.BreakLineTypeNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.ConcretingJointTypeNameTemp = value?.Name;
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
            _viewModel.AnnotationSettings.SpotDimensionTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }


    public bool CheckSettings() {
        if(SelectedDimensionType is null) {
            SetError(_localizationService.GetLocalizedString("VM.DimensionTypeNotSelected"));
            return false;
        }
        if(SelectedSpotDimensionType is null) {
            SetError(_localizationService.GetLocalizedString("VM.SpotDimensionTypeNotSelected"));
            return false;
        }
        if(SelectedSkeletonTagType is null) {
            SetError(_localizationService.GetLocalizedString("VM.SkeletonTagTypeNotSelected"));
            return false;
        }
        if(SelectedRebarTagTypeWithSerif is null) {
            SetError(_localizationService.GetLocalizedString("VM.RebarTagWithSerifTypeNotSelected"));
            return false;
        }
        if(SelectedRebarTagTypeWithStep is null) {
            SetError(_localizationService.GetLocalizedString("VM.RebarTagWithoutSerifTypeNotSelected"));
            return false;
        }
        if(SelectedRebarTagTypeWithComment is null) {
            SetError(_localizationService.GetLocalizedString("VM.RebarTagWithCommentTypeNotSelected"));
            return false;
        }
        if(SelectedUniversalTagType is null) {
            SetError(_localizationService.GetLocalizedString("VM.UniversalTagTypeNotSelected"));
            return false;
        }
        if(SelectedBreakLineType is null) {
            SetError(_localizationService.GetLocalizedString("VM.BreakLineTypeNotSelected"));
            return false;
        }
        if(SelectedConcretingJointType is null) {
            SetError(_localizationService.GetLocalizedString("VM.ConcretingJointTypeNotSelected"));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получает типоразмер для расстановки размеров
    /// </summary>
    public void FindDimensionType() {
        if(!String.IsNullOrEmpty(DimensionTypeName)) {
            SelectedDimensionType = _viewModel.DimensionTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(DimensionTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер высотной отметки
    /// </summary>
    public void FindSpotDimensionType() {
        if(!String.IsNullOrEmpty(SpotDimensionTypeName)) {
            SelectedSpotDimensionType = _viewModel.SpotDimensionTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(SpotDimensionTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматурного каркаса
    /// </summary>
    public void FindSkeletonTagType() {
        if(!String.IsNullOrEmpty(SkeletonTagTypeName)) {
            SelectedSkeletonTagType = _viewModel.RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(SkeletonTagTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с засечкой
    /// </summary>
    public void FindRebarTagTypeWithSerif() {
        if(!String.IsNullOrEmpty(RebarTagTypeWithSerifName)) {
            SelectedRebarTagTypeWithSerif = _viewModel.RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(RebarTagTypeWithSerifName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с шагом
    /// </summary>
    public void FindRebarTagTypeWithStep() {
        if(!String.IsNullOrEmpty(RebarTagTypeWithStepName)) {
            SelectedRebarTagTypeWithStep = _viewModel.RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(RebarTagTypeWithStepName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с количеством
    /// </summary>
    public void FindRebarTagTypeWithComment() {
        if(!String.IsNullOrEmpty(RebarTagTypeWithCommentName)) {
            SelectedRebarTagTypeWithComment = _viewModel.RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(RebarTagTypeWithCommentName));
        }
    }

    /// <summary>
    /// Получает типоразмер универсальной марки
    /// </summary>
    public void FindUniversalTagType() {
        if(!String.IsNullOrEmpty(UniversalTagTypeName)) {
            SelectedUniversalTagType = _viewModel.TypicalAnnotationsTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(UniversalTagTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер аннотацию линии обрыва
    /// </summary>
    public void FindBreakLineType() {
        if(!String.IsNullOrEmpty(BreakLineTypeName)) {
            SelectedBreakLineType = _viewModel.DetailComponentsTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(BreakLineTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер аннотацию рабочего шва бетонирования
    /// </summary>
    public void FindConcretingJointType() {
        if(!String.IsNullOrEmpty(ConcretingJointTypeName)) {
            SelectedConcretingJointType = _viewModel.DetailComponentsTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ConcretingJointTypeName));
        }
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

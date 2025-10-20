using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserTypesSettingsVM : BaseViewModel {
    private ViewFamilyType _selectedViewFamilyType;
    private DimensionType _selectedDimensionType;
    private FamilySymbol _selectedSkeletonTagType;
    private FamilySymbol _selectedRebarTagTypeWithSerif;
    private FamilySymbol _selectedRebarTagTypeWithStep;
    private FamilySymbol _selectedRebarTagTypeWithComment;
    private FamilySymbol _selectedUniversalTagType;

    private FamilySymbol _selectedBreakLineType;
    private FamilySymbol _selectedConcretingJointType;

    private SpotDimensionType _selectedSpotDimensionType;
    private View _selectedGeneralViewTemplate;
    private View _selectedGeneralRebarViewTemplate;
    private View _selectedTransverseViewTemplate;
    private View _selectedTransverseRebarViewTemplate;
    private View _selectedLegend;
    private FamilySymbol _selectedTitleBlock;

    public UserTypesSettingsVM(MainViewModel mainViewModel) {
        ViewModel = mainViewModel;
    }

    public MainViewModel ViewModel { get; set; }

    /// <summary>
    /// Выбранная пользователем рамка листа
    /// </summary>
    public FamilySymbol SelectedTitleBlock {
        get => _selectedTitleBlock;
        set {
            RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
            ViewModel.ProjectSettings.TitleBlockNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранная пользователем легенда
    /// </summary>
    public View SelectedLegend {
        get => _selectedLegend;
        set {
            RaiseAndSetIfChanged(ref _selectedLegend, value);
            ViewModel.ProjectSettings.LegendNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер вида для создания новых видов
    /// </summary>
    public ViewFamilyType SelectedViewFamilyType {
        get => _selectedViewFamilyType;
        set {
            RaiseAndSetIfChanged(ref _selectedViewFamilyType, value);
            ViewModel.ViewSectionSettings.ViewFamilyTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedDimensionType, value);
            ViewModel.ProjectSettings.DimensionTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки для обозначения каркаса
    /// </summary>
    public FamilySymbol SelectedSkeletonTagType {
        get => _selectedSkeletonTagType;
        set {
            RaiseAndSetIfChanged(ref _selectedSkeletonTagType, value);
            ViewModel.ProjectSettings.SkeletonTagTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с засечкой
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithSerif {
        get => _selectedRebarTagTypeWithSerif;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithSerif, value);
            ViewModel.ProjectSettings.RebarTagTypeWithSerifNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с шагом
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithStep {
        get => _selectedRebarTagTypeWithStep;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithStep, value);
            ViewModel.ProjectSettings.RebarTagTypeWithStepNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с количеством
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithComment {
        get => _selectedRebarTagTypeWithComment;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithComment, value);
            ViewModel.ProjectSettings.RebarTagTypeWithCommentNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер аннотации рабочего шва бетонирования
    /// </summary>
    public FamilySymbol SelectedUniversalTagType {
        get => _selectedUniversalTagType;
        set {
            RaiseAndSetIfChanged(ref _selectedUniversalTagType, value);
            ViewModel.ProjectSettings.UniversalTagTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с количеством
    /// </summary>
    public FamilySymbol SelectedBreakLineType {
        get => _selectedBreakLineType;
        set {
            RaiseAndSetIfChanged(ref _selectedBreakLineType, value);
            ViewModel.ProjectSettings.BreakLineTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер аннотации рабочего шва бетонирования
    /// </summary>
    public FamilySymbol SelectedConcretingJointType {
        get => _selectedConcretingJointType;
        set {
            RaiseAndSetIfChanged(ref _selectedConcretingJointType, value);
            ViewModel.ProjectSettings.ConcretingJointTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    public SpotDimensionType SelectedSpotDimensionType {
        get => _selectedSpotDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedSpotDimensionType, value);
            ViewModel.ProjectSettings.SpotDimensionTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов
    /// </summary>
    public View SelectedGeneralViewTemplate {
        get => _selectedGeneralViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralViewTemplate, value);
            ViewModel.ViewSectionSettings.GeneralViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов армирования
    /// </summary>
    public View SelectedGeneralRebarViewTemplate {
        get => _selectedGeneralRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralRebarViewTemplate, value);
            ViewModel.ViewSectionSettings.GeneralRebarViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов
    /// </summary>
    public View SelectedTransverseViewTemplate {
        get => _selectedTransverseViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseViewTemplate, value);
            ViewModel.ViewSectionSettings.TransverseViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов армирования
    /// </summary>
    public View SelectedTransverseRebarViewTemplate {
        get => _selectedTransverseRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseRebarViewTemplate, value);
            ViewModel.ViewSectionSettings.TransverseRebarViewTemplateNameTemp = value?.Name;
        }
    }


    public UserTypesSettings GetSettings() {
        var settings = new UserTypesSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserTypesSettings);

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

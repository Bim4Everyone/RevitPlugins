using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserTypesSettingsVM : ValidatableViewModel {
    private DimensionType _selectedDimensionType;
    private FamilySymbol _selectedSkeletonTagType;
    private FamilySymbol _selectedRebarTagTypeWithSerif;
    private FamilySymbol _selectedRebarTagTypeWithStep;
    private FamilySymbol _selectedRebarTagTypeWithComment;
    private FamilySymbol _selectedUniversalTagType;

    private FamilySymbol _selectedBreakLineType;
    private FamilySymbol _selectedConcretingJointType;

    private SpotDimensionType _selectedSpotDimensionType;

    public UserTypesSettingsVM(MainViewModel mainViewModel) {
        ViewModel = mainViewModel;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }


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

using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;

internal class UserVerticalViewSettingsPageVM : ValidatableViewModel {
    private readonly ILocalizationService _localizationService;

    private string _generalViewPrefixTemp = "";
    private string _generalViewSuffixTemp = "";
    private string _generalRebarViewPrefixTemp = "Каркас ";
    private string _generalRebarViewSuffixTemp = "";
    private string _generalViewPerpendicularPrefixTemp = "";
    private string _generalViewPerpendicularSuffixTemp = "_Боковой";
    private string _generalRebarViewPerpendicularPrefixTemp = "Каркас ";
    private string _generalRebarViewPerpendicularSuffixTemp = "_Боковой";

    private string _generalViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ВЕРТ_Пилоны";
    private string _generalRebarViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ВЕРТ_Пилоны_Каркас";
    private string _generalViewXOffsetTemp = "100";

    private string _generalViewYTopOffsetTemp = "300";
    private string _generalViewYBottomOffsetTemp = "500";

    private string _generalViewPerpXOffsetTemp = "240";
    private string _generalViewPerpYTopOffsetTemp = "300";
    private string _generalViewPerpYBottomOffsetTemp = "500";

    private string _generalViewFamilyTypeNameTemp = "РАЗРЕЗ_Номер вида без номера листа";
    private ViewFamilyType _selectedGeneralViewFamilyType;

    private View _selectedGeneralRebarViewTemplate;
    private View _selectedGeneralViewTemplate;


    public UserVerticalViewSettingsPageVM(MainViewModel mainViewModel, ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }

    public string GeneralViewPrefix { get; set; }
    public string GeneralViewPrefixTemp {
        get => _generalViewPrefixTemp;
        set => RaiseAndSetIfChanged(ref _generalViewPrefixTemp, value);
    }

    public string GeneralViewSuffix { get; set; }
    public string GeneralViewSuffixTemp {
        get => _generalViewSuffixTemp;
        set => RaiseAndSetIfChanged(ref _generalViewSuffixTemp, value);
    }

    public string GeneralViewPerpendicularPrefix { get; set; }
    public string GeneralViewPerpendicularPrefixTemp {
        get => _generalViewPerpendicularPrefixTemp;
        set => RaiseAndSetIfChanged(ref _generalViewPerpendicularPrefixTemp, value);
    }

    public string GeneralViewPerpendicularSuffix { get; set; }
    public string GeneralViewPerpendicularSuffixTemp {
        get => _generalViewPerpendicularSuffixTemp;
        set => RaiseAndSetIfChanged(ref _generalViewPerpendicularSuffixTemp, value);
    }

    public string GeneralRebarViewPrefix { get; set; }
    public string GeneralRebarViewPrefixTemp {
        get => _generalRebarViewPrefixTemp;
        set => RaiseAndSetIfChanged(ref _generalRebarViewPrefixTemp, value);
    }

    public string GeneralRebarViewSuffix { get; set; }
    public string GeneralRebarViewSuffixTemp {
        get => _generalRebarViewSuffixTemp;
        set => RaiseAndSetIfChanged(ref _generalRebarViewSuffixTemp, value);
    }

    public string GeneralRebarViewPerpendicularPrefix { get; set; }
    public string GeneralRebarViewPerpendicularPrefixTemp {
        get => _generalRebarViewPerpendicularPrefixTemp;
        set => RaiseAndSetIfChanged(ref _generalRebarViewPerpendicularPrefixTemp, value);
    }

    public string GeneralRebarViewPerpendicularSuffix { get; set; }
    public string GeneralRebarViewPerpendicularSuffixTemp {
        get => _generalRebarViewPerpendicularSuffixTemp;
        set => RaiseAndSetIfChanged(ref _generalRebarViewPerpendicularSuffixTemp, value);
    }

    public string GeneralViewTemplateName { get; set; }
    public string GeneralViewTemplateNameTemp {
        get => _generalViewTemplateNameTemp;
        set => RaiseAndSetIfChanged(ref _generalViewTemplateNameTemp, value);
    }

    public string GeneralRebarViewTemplateName { get; set; }
    public string GeneralRebarViewTemplateNameTemp {
        get => _generalRebarViewTemplateNameTemp;
        set => RaiseAndSetIfChanged(ref _generalRebarViewTemplateNameTemp, value);
    }

    public string GeneralViewXOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewXOffsetTemp {
        get => _generalViewXOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewYTopOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewYTopOffsetTemp {
        get => _generalViewYTopOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewYBottomOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewYBottomOffsetTemp {
        get => _generalViewYBottomOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewPerpXOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewPerpXOffsetTemp {
        get => _generalViewPerpXOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewPerpXOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewPerpYTopOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewPerpYTopOffsetTemp {
        get => _generalViewPerpYTopOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewPerpYTopOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewPerpYBottomOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string GeneralViewPerpYBottomOffsetTemp {
        get => _generalViewPerpYBottomOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _generalViewPerpYBottomOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string GeneralViewFamilyTypeName { get; set; }
    public string GeneralViewFamilyTypeNameTemp {
        get => _generalViewFamilyTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _generalViewFamilyTypeNameTemp, value);
    }

    /// <summary>
    /// Выбранный пользователем типоразмер вида для создания новых видов
    /// </summary>
    [Required]
    public ViewFamilyType SelectedGeneralViewFamilyType {
        get => _selectedGeneralViewFamilyType;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralViewFamilyType, value);
            GeneralViewFamilyTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов
    /// </summary>
    [Required]
    public View SelectedGeneralViewTemplate {
        get => _selectedGeneralViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralViewTemplate, value);
            GeneralViewTemplateNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов армирования
    /// </summary>
    [Required]
    public View SelectedGeneralRebarViewTemplate {
        get => _selectedGeneralRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralRebarViewTemplate, value);
            GeneralRebarViewTemplateNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    public void ApplyViewSectionsSettings() {
        GeneralViewPrefix = GeneralViewPrefixTemp;
        GeneralViewSuffix = GeneralViewSuffixTemp;
        GeneralViewPerpendicularPrefix = GeneralViewPerpendicularPrefixTemp;
        GeneralViewPerpendicularSuffix = GeneralViewPerpendicularSuffixTemp;

        GeneralRebarViewPrefix = GeneralRebarViewPrefixTemp;
        GeneralRebarViewSuffix = GeneralRebarViewSuffixTemp;
        GeneralRebarViewPerpendicularPrefix = GeneralRebarViewPerpendicularPrefixTemp;
        GeneralRebarViewPerpendicularSuffix = GeneralRebarViewPerpendicularSuffixTemp;

        GeneralViewTemplateName = GeneralViewTemplateNameTemp;
        GeneralRebarViewTemplateName = GeneralRebarViewTemplateNameTemp;
        GeneralViewXOffset = GeneralViewXOffsetTemp;
        GeneralViewYTopOffset = GeneralViewYTopOffsetTemp;
        GeneralViewYBottomOffset = GeneralViewYBottomOffsetTemp;
        GeneralViewPerpXOffset = GeneralViewPerpXOffsetTemp;
        GeneralViewPerpYTopOffset = GeneralViewPerpYTopOffsetTemp;
        GeneralViewPerpYBottomOffset = GeneralViewPerpYBottomOffsetTemp;

        GeneralViewFamilyTypeName = GeneralViewFamilyTypeNameTemp;
    }

    public void CheckViewSectionsSettings() {
        if(!int.TryParse(GeneralViewXOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.MainViewXOffsetInvalid");
        }
        if(!int.TryParse(GeneralViewYTopOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.MainViewYTopOffsetInvalid");
        }
        if(!int.TryParse(GeneralViewYBottomOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.MainViewYBottomOffsetInvalid");
        }
        if(!int.TryParse(GeneralViewPerpXOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.SideViewXOffsetInvalid");
        }
        if(!int.TryParse(GeneralViewPerpYTopOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.SideViewYTopOffsetInvalid");
        }
        if(!int.TryParse(GeneralViewPerpYBottomOffset, out _)) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.SideViewYBottomOffsetInvalid");
        }
    }

    public UserVerticalViewSettings GetSettings() {
        var settings = new UserVerticalViewSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserVerticalViewSettings);

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

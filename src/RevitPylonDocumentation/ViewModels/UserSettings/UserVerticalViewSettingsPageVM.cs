using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitPylonDocumentation.ViewModels.UserSettings;

internal class UserVerticalViewSettingsPageVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
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
        _viewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }


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

    public bool CheckSettings() {
        if(!int.TryParse(GeneralViewXOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.MainViewXOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(GeneralViewYTopOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.MainViewYTopOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(GeneralViewYBottomOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.MainViewYBottomOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(GeneralViewPerpXOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.SideViewXOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(GeneralViewPerpYTopOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.SideViewYTopOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(GeneralViewPerpYBottomOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.SideViewYBottomOffsetInvalid"));
            return false;
        }

        if(SelectedGeneralViewTemplate is null) {
            SetError(_localizationService.GetLocalizedString("VM.MainViewsTemplateNotSelected"));
            return false;
        }
        if(SelectedGeneralRebarViewTemplate is null) {
            SetError(_localizationService.GetLocalizedString("VM.MainRebarViewsTemplateNotSelected"));
            return false;
        }
        if(SelectedGeneralViewFamilyType is null) {
            SetError(_localizationService.GetLocalizedString("VM.ViewTypeNotSelected"));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получает шаблон для основных видов по имени
    /// </summary>
    public void FindGeneralViewTemplate() {
        if(!String.IsNullOrEmpty(GeneralViewTemplateName)) {
            SelectedGeneralViewTemplate = _viewModel.ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(GeneralViewTemplateName));
        }
    }

    /// <summary>
    /// Получает шаблон для основных видов армирования по имени
    /// </summary>
    public void FindGeneralRebarViewTemplate() {
        if(!String.IsNullOrEmpty(GeneralRebarViewTemplateName)) {
            SelectedGeneralRebarViewTemplate = _viewModel.ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(GeneralRebarViewTemplateName));
        }
    }

    /// <summary>
    /// Получает типоразмер вида для создаваемых видов
    /// </summary> 
    public void FindVerticalViewFamilyType() {
        if(!String.IsNullOrEmpty(GeneralViewFamilyTypeName)) {
            SelectedGeneralViewFamilyType = _viewModel.ViewFamilyTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(GeneralViewFamilyTypeName));
        }
    }

    /// <summary>
    /// Записывает ошибку для отображения в GUI, указывая наименование вкладки, на которой произошла ошибка
    /// </summary>
    /// <param name="error"></param>
    private void SetError(string error) {
        _viewModel.ErrorText = string.Format(
            "{0} - {1}",
            _localizationService.GetLocalizedString("MainWindow.VerticalViews"),
            error);
    }
}

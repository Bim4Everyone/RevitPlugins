using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitPylonDocumentation.ViewModels.UserSettings;

internal class UserTransverseViewSettingsPageVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly ILocalizationService _localizationService;

    private string _transverseViewDepthTemp = "900";
    private string _transverseViewFirstPrefixTemp = "";
    private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
    private string _transverseViewFirstElevationTemp = "800";
    private string _transverseViewSecondPrefixTemp = "";
    private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
    private string _transverseViewSecondElevationTemp = "2000";
    private string _transverseViewThirdPrefixTemp = "";
    private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";
    private string _transverseViewThirdElevationTemp = "600";

    private string _transverseRebarViewDepthTemp = "1200";
    private string _transverseRebarViewFirstPrefixTemp = "Каркас ";
    private string _transverseRebarViewFirstSuffixTemp = "_Сеч.а-а";
    private string _transverseRebarViewSecondPrefixTemp = "Каркас ";
    private string _transverseRebarViewSecondSuffixTemp = "_Сеч.б-б";
    private string _transverseRebarViewThirdPrefixTemp = "Каркас ";
    private string _transverseRebarViewThirdSuffixTemp = "_Сеч.в-в";

    private string _transverseViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ГОР_Пилоны";
    private string _transverseRebarViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ГОР_Пилоны_Каркас";
    private string _transverseViewXOffsetTemp = "50";
    private string _transverseViewYOffsetTemp = "50";

    private string _transverseViewFamilyTypeNameTemp = "РАЗРЕЗ_Номер вида без номера листа";
    private ViewFamilyType _selectedTransverseViewFamilyType;

    private View _selectedTransverseViewTemplate;
    private View _selectedTransverseRebarViewTemplate;

    public UserTransverseViewSettingsPageVM(MainViewModel mainViewModel, ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }


    public string TransverseViewDepth { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewDepthTemp {
        get => _transverseViewDepthTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewDepthTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseViewFirstPrefix { get; set; }
    public string TransverseViewFirstPrefixTemp {
        get => _transverseViewFirstPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewFirstPrefixTemp, value);
    }

    public string TransverseViewFirstSuffix { get; set; }
    public string TransverseViewFirstSuffixTemp {
        get => _transverseViewFirstSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewFirstSuffixTemp, value);
    }

    public string TransverseViewFirstElevation { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewFirstElevationTemp {
        get => _transverseViewFirstElevationTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewFirstElevationTemp, value);
            ValidateProperty(value);
        }
    }


    public string TransverseViewSecondPrefix { get; set; }
    public string TransverseViewSecondPrefixTemp {
        get => _transverseViewSecondPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewSecondPrefixTemp, value);
    }

    public string TransverseViewSecondSuffix { get; set; }
    public string TransverseViewSecondSuffixTemp {
        get => _transverseViewSecondSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewSecondSuffixTemp, value);
    }

    public string TransverseViewSecondElevation { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewSecondElevationTemp {
        get => _transverseViewSecondElevationTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewSecondElevationTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseViewThirdPrefix { get; set; }
    public string TransverseViewThirdPrefixTemp {
        get => _transverseViewThirdPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewThirdPrefixTemp, value);
    }

    public string TransverseViewThirdSuffix { get; set; }
    public string TransverseViewThirdSuffixTemp {
        get => _transverseViewThirdSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewThirdSuffixTemp, value);
    }

    public string TransverseViewThirdElevation { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewThirdElevationTemp {
        get => _transverseViewThirdElevationTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewThirdElevationTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseRebarViewDepth { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseRebarViewDepthTemp {
        get => _transverseRebarViewDepthTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseRebarViewDepthTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseRebarViewFirstPrefix { get; set; }
    public string TransverseRebarViewFirstPrefixTemp {
        get => _transverseRebarViewFirstPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewFirstPrefixTemp, value);
    }

    public string TransverseRebarViewFirstSuffix { get; set; }
    public string TransverseRebarViewFirstSuffixTemp {
        get => _transverseRebarViewFirstSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewFirstSuffixTemp, value);
    }

    public string TransverseRebarViewSecondPrefix { get; set; }
    public string TransverseRebarViewSecondPrefixTemp {
        get => _transverseRebarViewSecondPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewSecondPrefixTemp, value);
    }

    public string TransverseRebarViewSecondSuffix { get; set; }
    public string TransverseRebarViewSecondSuffixTemp {
        get => _transverseRebarViewSecondSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewSecondSuffixTemp, value);
    }

    public string TransverseRebarViewThirdPrefix { get; set; }
    public string TransverseRebarViewThirdPrefixTemp {
        get => _transverseRebarViewThirdPrefixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewThirdPrefixTemp, value);
    }

    public string TransverseRebarViewThirdSuffix { get; set; }
    public string TransverseRebarViewThirdSuffixTemp {
        get => _transverseRebarViewThirdSuffixTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewThirdSuffixTemp, value);
    }

    public string TransverseViewTemplateName { get; set; }
    public string TransverseViewTemplateNameTemp {
        get => _transverseViewTemplateNameTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewTemplateNameTemp, value);
    }

    public string TransverseRebarViewTemplateName { get; set; }
    public string TransverseRebarViewTemplateNameTemp {
        get => _transverseRebarViewTemplateNameTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewTemplateNameTemp, value);
    }

    public string TransverseViewXOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewXOffsetTemp {
        get => _transverseViewXOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseViewYOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string TransverseViewYOffsetTemp {
        get => _transverseViewYOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string TransverseViewFamilyTypeName { get; set; }
    public string TransverseViewFamilyTypeNameTemp {
        get => _transverseViewFamilyTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewFamilyTypeNameTemp, value);
    }


    /// <summary>
    /// Выбранный пользователем типоразмер вида для создания новых видов
    /// </summary>
    [Required]
    public ViewFamilyType SelectedTransverseViewFamilyType {
        get => _selectedTransverseViewFamilyType;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseViewFamilyType, value);
            TransverseViewFamilyTypeNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов
    /// </summary>
    [Required]
    public View SelectedTransverseViewTemplate {
        get => _selectedTransverseViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseViewTemplate, value);
            TransverseViewTemplateNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов армирования
    /// </summary>
    [Required]
    public View SelectedTransverseRebarViewTemplate {
        get => _selectedTransverseRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseRebarViewTemplate, value);
            TransverseRebarViewTemplateNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    public bool CheckSettings() {
        if(!int.TryParse(TransverseViewXOffset, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.TransverseViewXOffsetInvalid"));
            return false;
        }
        if(!int.TryParse(TransverseViewYOffsetTemp, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.TransverseViewYOffsetInvalid"));
            return false;
        }

        if(!double.TryParse(TransverseViewFirstElevationTemp, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.FirstTransverseViewElevationInvalid"));
            return false;
        }
        if(!double.TryParse(TransverseViewSecondElevationTemp, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.SecondTransverseViewElevationInvalid"));
            return false;
        }
        if(!double.TryParse(TransverseViewThirdElevationTemp, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.ThirdTransverseViewElevationInvalid"));
            return false;
        }

        if(SelectedTransverseViewTemplate is null) {
            SetError(_localizationService.GetLocalizedString("VM.TransverseViewsTemplateNotSelected"));
            return false;
        }
        if(SelectedTransverseRebarViewTemplate is null) {
            SetError(_localizationService.GetLocalizedString("VM.TransverseRebarViewsTemplateNotSelected"));
            return false;
        }
        if(SelectedTransverseViewFamilyType is null) {
            SetError(_localizationService.GetLocalizedString("VM.ViewTypeNotSelected"));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получает шаблон для поперечных видов по имени
    /// </summary>
    public void FindTransverseViewTemplate() {
        if(!String.IsNullOrEmpty(TransverseViewTemplateName)) {
            SelectedTransverseViewTemplate = _viewModel.ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(TransverseViewTemplateName));
        }
    }

    /// <summary>
    /// Получает шаблон для поперечных видов армирования по имени
    /// </summary>
    public void FindTransverseRebarViewTemplate() {
        if(!String.IsNullOrEmpty(TransverseRebarViewTemplateName)) {
            SelectedTransverseRebarViewTemplate = _viewModel.ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(TransverseRebarViewTemplateName));
        }
    }


    /// <summary>
    /// Получает типоразмер вида для создаваемых видов
    /// </summary>
    public void FindTransverseViewFamilyType() {
        if(!String.IsNullOrEmpty(TransverseViewFamilyTypeName)) {
            SelectedTransverseViewFamilyType = _viewModel.ViewFamilyTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(TransverseViewFamilyTypeName));
        }
    }

    /// <summary>
    /// Записывает ошибку для отображения в GUI, указывая наименование вкладки, на которой произошла ошибка
    /// </summary>
    /// <param name="error"></param>
    private void SetError(string error) {
        _viewModel.ErrorText = string.Format(
            "{0} - {1}",
            _localizationService.GetLocalizedString("MainWindow.TransverseViews"),
            error);
    }
}

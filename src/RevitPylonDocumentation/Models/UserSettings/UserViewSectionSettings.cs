using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserViewSectionSettings : BaseViewModel {
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
    private string _generalViewXOffsetTemp = "250";
    private string _generalViewYTopOffsetTemp = "1300";
    private string _generalViewYBottomOffsetTemp = "500";

    private string _transverseViewFirstPrefixTemp = "";
    private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
    private string _transverseViewFirstElevationTemp = "0,25";
    private string _transverseViewSecondPrefixTemp = "";
    private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
    private string _transverseViewSecondElevationTemp = "0,45";
    private string _transverseViewThirdPrefixTemp = "";
    private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";
    private string _transverseViewThirdElevationTemp = "1,1";

    private string _transverseRebarViewFirstPrefixTemp = "Каркас ";
    private string _transverseRebarViewFirstSuffixTemp = "_Сеч.а-а";
    private string _transverseRebarViewFirstElevationTemp = "0,3";
    private string _transverseRebarViewSecondPrefixTemp = "Каркас ";
    private string _transverseRebarViewSecondSuffixTemp = "_Сеч.б-б";
    private string _transverseRebarViewSecondElevationTemp = "1,1";

    private string _transverseViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ГОР_Пилоны";
    private string _transverseRebarViewTemplateNameTemp = "я_КЖ1.1_АРМ_РЗ_ГОР_Пилоны_Каркас";
    private string _transverseViewXOffsetTemp = "200";
    private string _transverseViewYOffsetTemp = "600";

    private string _viewFamilyTypeNameTemp = "РАЗРЕЗ_Номер вида без номера листа";

    public UserViewSectionSettings(MainViewModel mainViewModel) {

        ViewModel = mainViewModel;
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
    public string GeneralViewXOffsetTemp {
        get => _generalViewXOffsetTemp;
        set => RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
    }

    public string GeneralViewYTopOffset { get; set; }
    public string GeneralViewYTopOffsetTemp {
        get => _generalViewYTopOffsetTemp;
        set => RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
    }

    public string GeneralViewYBottomOffset { get; set; }
    public string GeneralViewYBottomOffsetTemp {
        get => _generalViewYBottomOffsetTemp;
        set => RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
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
    public string TransverseViewFirstElevationTemp {
        get => _transverseViewFirstElevationTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewFirstElevationTemp, value);
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
    public string TransverseViewSecondElevationTemp {
        get => _transverseViewSecondElevationTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewSecondElevationTemp, value);
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
    public string TransverseViewThirdElevationTemp {
        get => _transverseViewThirdElevationTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewThirdElevationTemp, value);
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

    public string TransverseRebarViewFirstElevation { get; set; }
    public string TransverseRebarViewFirstElevationTemp {
        get => _transverseRebarViewFirstElevationTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewFirstElevationTemp, value);
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

    public string TransverseRebarViewSecondElevation { get; set; }
    public string TransverseRebarViewSecondElevationTemp {
        get => _transverseRebarViewSecondElevationTemp;
        set => RaiseAndSetIfChanged(ref _transverseRebarViewSecondElevationTemp, value);
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
    public string TransverseViewXOffsetTemp {
        get => _transverseViewXOffsetTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
    }

    public string TransverseViewYOffset { get; set; }
    public string TransverseViewYOffsetTemp {
        get => _transverseViewYOffsetTemp;
        set => RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
    }

    public string ViewFamilyTypeName { get; set; }
    public string ViewFamilyTypeNameTemp {
        get => _viewFamilyTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _viewFamilyTypeNameTemp, value);
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

        TransverseViewFirstPrefix = TransverseViewFirstPrefixTemp;
        TransverseViewFirstSuffix = TransverseViewFirstSuffixTemp;
        TransverseViewFirstElevation = TransverseViewFirstElevationTemp;
        TransverseViewSecondPrefix = TransverseViewSecondPrefixTemp;
        TransverseViewSecondSuffix = TransverseViewSecondSuffixTemp;
        TransverseViewSecondElevation = TransverseViewSecondElevationTemp;
        TransverseViewThirdPrefix = TransverseViewThirdPrefixTemp;
        TransverseViewThirdSuffix = TransverseViewThirdSuffixTemp;
        TransverseViewThirdElevation = TransverseViewThirdElevationTemp;

        TransverseRebarViewFirstPrefix = TransverseRebarViewFirstPrefixTemp;
        TransverseRebarViewFirstSuffix = TransverseRebarViewFirstSuffixTemp;
        TransverseRebarViewFirstElevation = TransverseRebarViewFirstElevationTemp;
        TransverseRebarViewSecondPrefix = TransverseRebarViewSecondPrefixTemp;
        TransverseRebarViewSecondSuffix = TransverseRebarViewSecondSuffixTemp;
        TransverseRebarViewSecondElevation = TransverseRebarViewSecondElevationTemp;

        TransverseViewTemplateName = TransverseViewTemplateNameTemp;
        TransverseRebarViewTemplateName = TransverseRebarViewTemplateNameTemp;
        TransverseViewXOffset = TransverseViewXOffsetTemp;
        TransverseViewYOffset = TransverseViewYOffsetTemp;

        ViewFamilyTypeName = ViewFamilyTypeNameTemp;
    }

    public void CheckViewSectionsSettings() {
        if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewXOffset, out _)) {
            ViewModel.ErrorText = "Значение отступа основного вида по X некорректно";
        }
        if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset, out _)) {
            ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
        }
        if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset, out _)) {
            ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
        }

        if(!int.TryParse(ViewModel.ViewSectionSettings.TransverseViewXOffset, out _)) {
            ViewModel.ErrorText = "Значение отступа поперечного вида по X некорректно";
        }
        if(!int.TryParse(ViewModel.ViewSectionSettings.TransverseViewYOffsetTemp, out _)) {
            ViewModel.ErrorText = "Значение отступа поперечного вида по Y некорректно";
        }

        if(!double.TryParse(ViewModel.ViewSectionSettings.TransverseViewFirstElevationTemp, out _)) {
            ViewModel.ErrorText = "Значение возвышения первого горизонтального вида некорректно";
        }
        if(!double.TryParse(ViewModel.ViewSectionSettings.TransverseViewSecondElevationTemp, out _)) {
            ViewModel.ErrorText = "Значение возвышения второго горизонтального вида некорректно";
        }
        if(!double.TryParse(ViewModel.ViewSectionSettings.TransverseViewThirdElevationTemp, out _)) {
            ViewModel.ErrorText = "Значение возвышения третьего горизонтального вида некорректно";
        }
        if(!double.TryParse(ViewModel.ViewSectionSettings.TransverseRebarViewFirstElevationTemp, out _)) {
            ViewModel.ErrorText = "Значение возвышения первого горизонтального вида армирования некорректно";
        }
        if(!double.TryParse(ViewModel.ViewSectionSettings.TransverseRebarViewSecondElevationTemp, out _)) {
            ViewModel.ErrorText = "Значение возвышения второго горизонтального вида армирования некорректно";
        }
    }
}

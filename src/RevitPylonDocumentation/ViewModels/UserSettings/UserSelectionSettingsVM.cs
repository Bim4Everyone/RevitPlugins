using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserSelectionSettingsVM : BaseViewModel {
    private bool _needWorkWithGeneralView = false;
    private bool _needWorkWithGeneralPerpendicularView = false;
    private bool _needWorkWithTransverseViewFirst = false;
    private bool _needWorkWithTransverseViewSecond = false;
    private bool _needWorkWithTransverseViewThird = false;

    private bool _needWorkWithGeneralRebarView = false;
    private bool _needWorkWithGeneralPerpendicularRebarView = false;
    private bool _needWorkWithTransverseRebarViewFirst = false;
    private bool _needWorkWithTransverseRebarViewSecond = false;
    private bool _needWorkWithTransverseRebarViewThird = false;

    private bool _needWorkWithSkeletonSchedule = false;
    private bool _needWorkWithSkeletonByElemsSchedule = false;
    private bool _needWorkWithMaterialSchedule = false;
    private bool _needWorkWithSystemPartsSchedule = false;
    private bool _needWorkWithIfcPartsSchedule = false;
    private bool _needWorkWithLegend = false;

    private string _selectedProjectSection = string.Empty;

    public bool NeedWorkWithGeneralView {
        get => _needWorkWithGeneralView;
        set => RaiseAndSetIfChanged(ref _needWorkWithGeneralView, value);
    }
    public bool NeedWorkWithGeneralPerpendicularView {
        get => _needWorkWithGeneralPerpendicularView;
        set => RaiseAndSetIfChanged(ref _needWorkWithGeneralPerpendicularView, value);
    }

    public bool NeedWorkWithTransverseViewFirst {
        get => _needWorkWithTransverseViewFirst;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseViewFirst, value);
    }

    public bool NeedWorkWithTransverseViewSecond {
        get => _needWorkWithTransverseViewSecond;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseViewSecond, value);
    }

    public bool NeedWorkWithTransverseViewThird {
        get => _needWorkWithTransverseViewThird;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseViewThird, value);
    }

    public bool NeedWorkWithGeneralRebarView {
        get => _needWorkWithGeneralRebarView;
        set => RaiseAndSetIfChanged(ref _needWorkWithGeneralRebarView, value);
    }

    public bool NeedWorkWithGeneralPerpendicularRebarView {
        get => _needWorkWithGeneralPerpendicularRebarView;
        set => RaiseAndSetIfChanged(ref _needWorkWithGeneralPerpendicularRebarView, value);
    }

    public bool NeedWorkWithTransverseRebarViewFirst {
        get => _needWorkWithTransverseRebarViewFirst;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseRebarViewFirst, value);
    }

    public bool NeedWorkWithTransverseRebarViewSecond {
        get => _needWorkWithTransverseRebarViewSecond;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseRebarViewSecond, value);
    }

    public bool NeedWorkWithTransverseRebarViewThird {
        get => _needWorkWithTransverseRebarViewThird;
        set => RaiseAndSetIfChanged(ref _needWorkWithTransverseRebarViewThird, value);
    }

    public bool NeedWorkWithSkeletonSchedule {
        get => _needWorkWithSkeletonSchedule;
        set => RaiseAndSetIfChanged(ref _needWorkWithSkeletonSchedule, value);
    }

    public bool NeedWorkWithSkeletonByElemsSchedule {
        get => _needWorkWithSkeletonByElemsSchedule;
        set => RaiseAndSetIfChanged(ref _needWorkWithSkeletonByElemsSchedule, value);
    }

    public bool NeedWorkWithMaterialSchedule {
        get => _needWorkWithMaterialSchedule;
        set => RaiseAndSetIfChanged(ref _needWorkWithMaterialSchedule, value);
    }

    public bool NeedWorkWithSystemPartsSchedule {
        get => _needWorkWithSystemPartsSchedule;
        set => RaiseAndSetIfChanged(ref _needWorkWithSystemPartsSchedule, value);
    }

    public bool NeedWorkWithIfcPartsSchedule {
        get => _needWorkWithIfcPartsSchedule;
        set => RaiseAndSetIfChanged(ref _needWorkWithIfcPartsSchedule, value);
    }

    public bool NeedWorkWithLegend {
        get => _needWorkWithLegend;
        set => RaiseAndSetIfChanged(ref _needWorkWithLegend, value);
    }


    /// <summary>
    /// Выбранный пользователем комплект документации
    /// </summary>
    public string SelectedProjectSection {
        get => _selectedProjectSection;
        set => RaiseAndSetIfChanged(ref _selectedProjectSection, value);
    }

    public UserSelectionSettings GetSettings() {
        var settings = new UserSelectionSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserSelectionSettings);

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

using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserLegendsAndAnnotationsSettingsVM : ValidatableViewModel {
    private readonly ILocalizationService _localizationService;

    private string _legendXOffsetTemp = "-100";
    private string _legendYOffsetTemp = "130";
    private string _legendNameTemp = "Указания для пилонов корпуса";
    private View _selectedLegend;

    public UserLegendsAndAnnotationsSettingsVM(MainViewModel mainViewModel, ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }

    public string LegendXOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string LegendXOffsetTemp {
        get => _legendXOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _legendXOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string LegendYOffset { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string LegendYOffsetTemp {
        get => _legendYOffsetTemp;
        set {
            RaiseAndSetIfChanged(ref _legendYOffsetTemp, value);
            ValidateProperty(value);
        }
    }

    public string LegendName { get; set; }
    public string LegendNameTemp {
        get => _legendNameTemp;
        set => RaiseAndSetIfChanged(ref _legendNameTemp, value);
    }

    /// <summary>
    /// Выбранная пользователем легенда
    /// </summary>
    [Required]
    public View SelectedLegend {
        get => _selectedLegend;
        set {
            RaiseAndSetIfChanged(ref _selectedLegend, value);
            LegendNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    public void ApplyLegendsAndAnnotationsSettings() {
        LegendName = LegendNameTemp;
        LegendXOffset = LegendXOffsetTemp;
        LegendYOffset = LegendYOffsetTemp;
    }

    public void CheckLegendsAndAnnotationsSettings() {
        // Проверяем, чтоб были заданы офсеты видового экрана легенды
        if(LegendXOffset is null || LegendYOffset is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.LegendOffsetsNotSet");
        }
    }

    public UserLegendsAndAnnotationsSettings GetSettings() {
        var settings = new UserLegendsAndAnnotationsSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserLegendsAndAnnotationsSettings);

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

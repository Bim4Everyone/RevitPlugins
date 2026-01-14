using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserLegendsAndAnnotationsSettingsVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly ILocalizationService _localizationService;

    private string _legendXOffsetTemp = "-100";
    private string _legendYOffsetTemp = "130";
    private string _legendNameTemp = "Указания для пилонов корпуса";
    private View _selectedLegend;

    public UserLegendsAndAnnotationsSettingsVM(MainViewModel mainViewModel, ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _localizationService = localizationService;
        ValidateAllProperties();
    }


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

    public bool CheckSettings() {
        // Проверяем, чтоб были заданы офсеты видового экрана легенды
        if(LegendXOffset is null || LegendYOffset is null) {
            _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.LegendOffsetsNotSet");
            return false;
        }
        if(SelectedLegend is null) {
            _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.LegendNotSelected");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получает легенду примечания по имени
    /// </summary>
    public void FindLegend() {
        if(!String.IsNullOrEmpty(LegendName)) {
            SelectedLegend = _viewModel.Legends
                .FirstOrDefault(view => view.Name.Contains(LegendName));
        }
    }
}

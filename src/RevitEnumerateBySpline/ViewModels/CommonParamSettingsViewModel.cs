using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Services;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.ViewModels;

internal class CommonParamSettingsViewModel : BaseViewModel {
    private readonly ParamService _paramService;
    private readonly RevitRepository _revitRepository;
    private ObservableCollection<ParamViewModel>? _numberParams;
    private ParamViewModel? _selectedNumberParam;
    private string? _startNumberText;
    private string? _prefix;
    private string? _suffix;

    public CommonParamSettingsViewModel(
        ParamService paramService,
        RevitRepository revitRepository) {
        _paramService = paramService;
        _revitRepository = revitRepository;
    }

    public ObservableCollection<ParamViewModel>? NumberParams {
        get => _numberParams;
        set => RaiseAndSetIfChanged(ref _numberParams, value);
    }
    
    public ParamViewModel? SelectedNumberParam {
        get => _selectedNumberParam;
        set => RaiseAndSetIfChanged(ref _selectedNumberParam, value);
    }
    
    public string? StartNumberText {
        get => _startNumberText;
        set => RaiseAndSetIfChanged(ref _startNumberText, value);
    }

    public double? StartNumber =>
        double.TryParse(
            StartNumberText,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture,
            out double value)
        && !double.IsNaN(value)
        && !double.IsInfinity(value)
            ? value
            : null;
    
    public string? Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }
    
    public string? Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    public void LoadView(ConfigSettings? settings) {
        NumberParams = new ObservableCollection<ParamViewModel>(GetNumberParamViewModels());
        SelectedNumberParam = NumberParams
            .FirstOrDefault(param => param?.RevitParam?.Name == settings?.NumberParam?.Name)
            ?? NumberParams.FirstOrDefault();
        
        StartNumberText = settings?.StartNumber?.ToString(CultureInfo.CurrentCulture);
        Prefix = settings?.Prefix;
        Suffix = settings?.Suffix;
    } 
    
    private IEnumerable<ParamViewModel> GetNumberParamViewModels() {
        var spatialElements = _revitRepository.AllSpatialModels;

        return _paramService.AllSpatialRevitParams
            .Where(revitParam => {
                if (!spatialElements.Any()) {
                    return true;
                }

                return spatialElements.All(spatialElement =>
                    _paramService.TryGetParam(
                        revitParam,
                        spatialElement.SpatialElement,
                        out var param)
                    && param is {
                        IsReadOnly: false,
                        StorageType: StorageType.String
                    });
            })
            .Select(param => new ParamViewModel {
                Name = param.Name,
                RevitParam = param
            });
    }
}

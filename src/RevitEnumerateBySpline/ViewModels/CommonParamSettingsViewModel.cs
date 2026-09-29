using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitEnumerateBySpline.ViewModels;

internal class CommonParamSettingsViewModel : BaseViewModel {
    
    private ObservableCollection<ParamViewModel>? _numberParams;
    private ParamViewModel? _selectedNumberParam;
    private double _startNumber = 1.0;
    private string _prefix = string.Empty;
    private string _suffix = string.Empty;
    
    public CommonParamSettingsViewModel() {
        LoadView();
    }

    public ObservableCollection<ParamViewModel>? NumberParams {
        get => _numberParams;
        set => RaiseAndSetIfChanged(ref _numberParams, value);
    }
    
    public ParamViewModel? SelectedNumberParam {
        get => _selectedNumberParam;
        set => RaiseAndSetIfChanged(ref _selectedNumberParam, value);
    }
    
    public double StartNumber {
        get => _startNumber;
        set => RaiseAndSetIfChanged(ref _startNumber, value);
    }
    
    public string Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }
    
    public string Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    private void LoadView() {
       
    } 
}

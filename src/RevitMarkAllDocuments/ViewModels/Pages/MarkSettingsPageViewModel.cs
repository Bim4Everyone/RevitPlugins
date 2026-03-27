using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkSettingsPageViewModel : BaseViewModel {
    private ObservableCollection<ParameterViewModel> _paramsForMark;
    private ParameterViewModel _selectedParam;
    private string _startNumber;
    private string _prefix;
    private string _suffix;
    private bool _isSuffixEnable;

    public MarkSettingsPageViewModel(RevitRepository revitRepository, Category category) {
        _paramsForMark = [.. revitRepository.GetFilterableParams(category)
            .Where(x => x.StorageType == StorageType.String 
                || x.StorageType == StorageType.Integer 
                || x.StorageType == StorageType.Double)
            .Select(x => new ParameterViewModel(x))];

    }

    public ObservableCollection<ParameterViewModel> ParamsForMark {
        get => _paramsForMark;
        set => RaiseAndSetIfChanged(ref _paramsForMark, value);
    }

    public ParameterViewModel SelectedParam {
        get => _selectedParam;
        set {
            RaiseAndSetIfChanged(ref _selectedParam, value);

            _isSuffixEnable =
                value?.RevitParam.StorageType == StorageType.String;

            RaisePropertyChanged(nameof(IsSuffixPrefixEnable));
        }
    }

    public string StartNumber {
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

    public bool IsSuffixPrefixEnable {
        get => _isSuffixEnable;
        set => RaiseAndSetIfChanged(ref _isSuffixEnable, value);
    }
}

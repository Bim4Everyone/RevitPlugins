using System.Globalization;

using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

internal class LintelRuleViewModel : BaseViewModel {
    private string _minWidthText;
    private string _maxWidthText;
    private string _wallSupportText;
    private string _lintelType;
    private string _errorText;

    public LintelRuleViewModel(LintelRule rule) {
        MinWidthText = rule.MinWidth.ToString(CultureInfo.InvariantCulture);
        MaxWidthText = rule.MaxWidth.ToString(CultureInfo.InvariantCulture);
        WallSupportText = rule.WallSupport.ToString(CultureInfo.InvariantCulture);
        LintelType = rule.LintelType;
    }

    public string MinWidthText {
        get => _minWidthText;
        set => RaiseAndSetIfChanged(ref _minWidthText, value);
    }

    public string MaxWidthText {
        get => _maxWidthText;
        set => RaiseAndSetIfChanged(ref _maxWidthText, value);
    }

    public string WallSupportText {
        get => _wallSupportText;
        set => RaiseAndSetIfChanged(ref _wallSupportText, value);
    }

    public string LintelType {
        get => _lintelType;
        set => RaiseAndSetIfChanged(ref _lintelType, value);
    }

    public string ErrorText {
        get => _errorText;
        set {
            RaiseAndSetIfChanged(ref _errorText, value);
            RaisePropertyChanged(nameof(HasErrors));
        }
    }

    public bool HasErrors => !string.IsNullOrEmpty(ErrorText);

    public bool TryGetRule(out LintelRule rule) {
        rule = null;
        if(!int.TryParse(MinWidthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minWidth)
            || !int.TryParse(MaxWidthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxWidth)
            || !int.TryParse(WallSupportText, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out int wallSupport)) {
            return false;
        }
        rule = new LintelRule {
            MinWidth = minWidth,
            MaxWidth = maxWidth,
            WallSupport = wallSupport,
            LintelType = LintelType
        };
        return true;
    }
}

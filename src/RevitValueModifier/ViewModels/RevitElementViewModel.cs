using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitValueModifier.Models;

namespace RevitValueModifier.ViewModels;
internal class RevitElementViewModel : BaseViewModel {
    private string _paramValue = string.Empty;
    private bool _isParamValueValid = true;
    private string _paramValueError = string.Empty;
    private string _elemName;
    private ElementId _elemId;
    private readonly RevitElement _revitElem;

    public RevitElementViewModel(RevitElement revitElement) {
        _revitElem = revitElement;
        ElemName = revitElement.ElemName;
        ElemId = revitElement.ElemId;
    }

    public string ElemName {
        get => _elemName;
        set => RaiseAndSetIfChanged(ref _elemName, value);
    }

    public ElementId ElemId {
        get => _elemId;
        set => RaiseAndSetIfChanged(ref _elemId, value);
    }

    public string ParamValue {
        get => _paramValue;
        set => RaiseAndSetIfChanged(ref _paramValue, value);
    }

    public bool IsParamValueValid {
        get => _isParamValueValid;
        set => RaiseAndSetIfChanged(ref _isParamValueValid, value);
    }

    public string ParamValueError {
        get => _paramValueError;
        set => RaiseAndSetIfChanged(ref _paramValueError, value);
    }

    public void WriteParamValue(RevitParameter revitParameter) {
        _revitElem.WriteParamValue(revitParameter);
    }

    public void SetParamValue(string paramValueMask, RevitParameter targetParameter, ParamValueParser paramValueParser) {
        _revitElem.SetParamValue(paramValueMask, targetParameter, paramValueParser);
        ParamValue = _revitElem.ParamValue;
        IsParamValueValid = _revitElem.IsParamValueValid;
        ParamValueError = _revitElem.ParamValueError;
    }
}

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitValueModifier.Models;

namespace RevitValueModifier.ViewModels;
internal class RevitElementViewModel : BaseViewModel {
    private string _paramValue = string.Empty;
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

    public void WriteParamValue(RevitParameter revitParameter) {
        _revitElem.WriteParamValue(revitParameter);
    }

    public void SetParamValue(string paramValueMask) {
        _revitElem.SetParamValue(paramValueMask);
        ParamValue = _revitElem.ParamValue;
    }
}

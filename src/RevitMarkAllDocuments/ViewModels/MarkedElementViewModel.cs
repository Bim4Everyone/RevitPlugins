using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkedElementViewModel : BaseViewModel{
    private readonly MarkedElement _element;
    private readonly string _id;
    private readonly string _name;
    private readonly string _markValue;
    private readonly string _status;
    private readonly bool _hasError;

    public MarkedElementViewModel(MarkedElement element, Document document, ILocalizationService localizationService) {
        _id = element.Id.ToString();
        _element = element;
        _markValue = element.MarkValue;

        var revitElement = document.GetElement(new ElementId(element.Id));
        if(revitElement != null) {
            _name = revitElement.Name;
            _status = "";
            _hasError = false;
        } else {
            _name = "???";
            _status = localizationService.GetLocalizedString("MarkList.NoElementInDoc");
            _hasError = true;
        }
    }

    public MarkedElement MarkedElement => _element;
    public string Id => _id;
    public string Name => _name;
    public string MarkValue => _markValue;
    public string Status => _status;
    public bool HasError => _hasError;
}

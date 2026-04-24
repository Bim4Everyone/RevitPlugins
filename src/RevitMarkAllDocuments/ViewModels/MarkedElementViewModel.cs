using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

    public MarkedElementViewModel(MarkedElement element, Document document, ILocalizationService localizationService) {
#if REVIT_2023_OR_LESS
        _id = element.Id.ToString();
#else
        _id = element.Id.ToString();
#endif
        _element = element;
        _markValue = element.MarkValue;

        var revitElement = document.GetElement(new ElementId(element.Id));
        if(revitElement != null) {
            _name = revitElement.Name;
            _status = "";
        } else {
            _name = "";
            _status = localizationService.GetLocalizedString("WarningsWindow.NoElementInDoc");
        }
    }

    public MarkedElement MarkedElement => _element;
    public string Id => _id;
    public string Name => _name;
    public string MarkValue => _markValue;
    public string Status => _status;
}

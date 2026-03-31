using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkedElementViewModel : BaseViewModel{
    private readonly string _id;
    private readonly string _name;
    private readonly string _markValue;
    private readonly string _status;

    public MarkedElementViewModel(MarkedElement element, Document document) {
#if REVIT_2023_OR_LESS
        _id = element.Id.IntegerValue.ToString();
#else
        _id = element.Id.Value.ToString();
#endif
        _markValue = element.MarkValue;

        var revitElement = document.GetElement(element.Id);
        if(revitElement != null) {
            _name = revitElement.Name;
            _status = "";
        } else {
            _name = "";
            _status = "элемент не найден в проекте";
        }
    }

    public string Id => _id;
    public string Name => _name;
    public string MarkValue => _markValue;
    public string Status => _status;
}

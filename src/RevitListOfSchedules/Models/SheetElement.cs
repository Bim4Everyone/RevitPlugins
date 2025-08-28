using System.Text;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitListOfSchedules.Models;
internal class SheetElement {
    private const string _revisionStartString = "Изм.1";
    private readonly ParamFactory _paramFactory;

    public SheetElement(ParamFactory paramFactory, ViewSheet viewSheet) {
        _paramFactory = paramFactory;
        Sheet = viewSheet;
        Name = Sheet.Name;
        Number = GetNumberParam();
        RevisionNumber = GetRevisionString();
    }

    public ViewSheet Sheet { get; }
    public string Name { get; }
    public string Number { get; }
    public string RevisionNumber { get; }

    private string GetNumberParam() {
        return Sheet.GetParamValueOrDefault<string>(_paramFactory.SharedParamNumber);
    }

    private string GetRevisionString() {
        var sb = new StringBuilder();
        for(int i = 0; i < _paramFactory.SharedParamsRevision.Count; i++) {
            if(Sheet.IsExistsParamValue(_paramFactory.SharedParamsRevision[i])) {
                string paramValue = Sheet.GetParamValue<string>(_paramFactory.SharedParamsRevision[i]);
                string paramValueRevision = Sheet.GetParamValue<string>(_paramFactory.SharedParamsRevisionValue[i]);

                sb.Append(_revisionStartString)
                  .Append(paramValue)
                  .Append($"({paramValueRevision}); ");
            }
        }
        return sb.ToString();
    }
}

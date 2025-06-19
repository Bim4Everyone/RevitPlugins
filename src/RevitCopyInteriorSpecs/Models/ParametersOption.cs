using Autodesk.Revit.DB;

namespace RevitCopyInteriorSpecs.Models;

internal class ParametersOption {

    public ParametersOption() { }

    public string FirstParamName { get; set; } = string.Empty;
    public string FirstParamValue { get; set; } = string.Empty;
    public string SecondParamName { get; set; } = string.Empty;
    public string SecondParamValue { get; set; } = string.Empty;
    public string ThirdParamName { get; set; } = string.Empty;
    public string ThirdParamValue { get; set; } = string.Empty;
    public string FourthParamName { get; set; } = string.Empty;
    public ElementId FourthParamValue { get; set; }
}

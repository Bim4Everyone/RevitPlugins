using Autodesk.Revit.DB;

namespace RevitKrChecker.Models;
public class CheckInfo {

    public CheckInfo(string checkName, string targetParamName, Element element, string tooltip) {
        Elem = element;
        TargetParamName = targetParamName;
        СheckName = checkName;
        ElementErrorTooltip = tooltip;
    }

    public Element Elem { get; }
    public string TargetParamName { get; }
    public string СheckName { get; }
    public string ElementErrorTooltip { get; }
}

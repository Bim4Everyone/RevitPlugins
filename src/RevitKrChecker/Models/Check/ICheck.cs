using Autodesk.Revit.DB;

namespace RevitKrChecker.Models.Check {
    public interface ICheck {
        string CheckName { get; }
        string TargetParamName { get; }

        string GetTooltip();
        bool Check(Element element, out CheckInfo info);
    }
}

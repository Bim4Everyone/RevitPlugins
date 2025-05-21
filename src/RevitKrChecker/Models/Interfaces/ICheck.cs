using Autodesk.Revit.DB;

namespace RevitKrChecker.Models.Interfaces {
    public interface ICheck {
        string CheckName { get; }
        string TargetParamName { get; }
        ParamLevel TargetParamLevel { get; }

        string GetTooltip();
        bool Check(Element element, out CheckInfo info);
    }
}

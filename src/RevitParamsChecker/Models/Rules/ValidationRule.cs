using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Rules;

internal abstract class ValidationRule {
    public abstract bool Evaluate(Element element);
}

using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Rules;

internal abstract class ValidationRule {
    /// <summary>
    /// Проверяет элемент и возвращает результат вместе с условиями, которые элемент не прошел
    /// </summary>
    public abstract EvaluationResult Evaluate(Element element);

    public abstract ValidationRule Copy();
}

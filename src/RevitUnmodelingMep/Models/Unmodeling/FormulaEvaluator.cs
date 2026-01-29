using System;
using System.Globalization;
using System.Text;

using Autodesk.Revit.DB;

using CodingSeb.ExpressionEvaluator;

using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class FormulaEvaluator {
    private readonly ILocalizationService _localizationService;

    public FormulaEvaluator(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public double Evaluate(string formula, CalculationElementBase calcElement) {
        try {
            // При необходимости проверок расчета включаем LogEvaluation и подаем туда айди элемента который проверяем
            // LogEvaluation(formula, calcElement, 5375862);
            return EvaluateInternal(formula, calcElement);
        } catch(Exception) {
            BuildDebugLog(calcElement, formula);
            throw;
        }
    }

    public void LogEvaluation(string formula, CalculationElementBase calcElement, int? elementIdInt = null) {
        if(calcElement == null) {
            return;
        }

        if(elementIdInt.HasValue && elementIdInt.Value != 0) {
            ElementId elementId;
#if REVIT_2024_OR_GREATER
            elementId = new ElementId((long) elementIdInt.Value);
#else
            elementId = new ElementId(elementIdInt.Value);
#endif

            if(calcElement.Element.Id != elementId) {
                return;
            }
        }

        double result = EvaluateInternal(formula, calcElement);
        string log = BuildDebugLogMessage(calcElement, formula, result);
        Console.WriteLine(log);
    }

    public void LogEvaluation(string formula, CalculationElementBase calcElement, string elementIdText) {
        int? elementId = null;
        if(!string.IsNullOrWhiteSpace(elementIdText) && int.TryParse(elementIdText, out int parsed)) {
            elementId = parsed;
        }

        LogEvaluation(formula, calcElement, elementId);
    }

    private double EvaluateInternal(string formula, CalculationElementBase calcElement) {
        var evaluator = CreateEvaluator(calcElement);
        object result = evaluator.Evaluate(formula);
        return Convert.ToDouble(result);
    }

    private static ExpressionEvaluator CreateEvaluator(CalculationElementBase calcElement) {
        var evaluator = new ExpressionEvaluator();
        foreach(var property in calcElement.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calcElement);
            evaluator.Variables[property.Name] = value;
        }

        return evaluator;
    }

    private void BuildDebugLog(CalculationElementBase calcElement, string formula) {
        string errorMessage = _localizationService.GetLocalizedString("UnmodelingCalculator.Error");
        string log = BuildDebugLogMessage(calcElement, formula, null, errorMessage);
        Console.WriteLine(log);
        throw new OperationCanceledException(errorMessage);
    }

    private string BuildDebugLogMessage(
        CalculationElementBase calcElement,
        string formula,
        double? result,
        string header = null) {

        StringBuilder logBuilder = new StringBuilder();
        if(!string.IsNullOrWhiteSpace(header)) {
            logBuilder.AppendLine(header);
        }

        logBuilder.AppendLine($"ElementId: {calcElement.Element.Id}");
        logBuilder.AppendLine($"Formula: {formula}");
        if(result.HasValue) {
            logBuilder.AppendLine($"Result: {result.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        foreach(var property in calcElement.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calcElement);
            string valueText = value switch {
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                null => "null",
                _ => value.ToString()
            };
            logBuilder.AppendLine($"{property.Name} = {valueText}");
        }

        logBuilder.AppendLine("__________________________________");
        return logBuilder.ToString();
    }
}

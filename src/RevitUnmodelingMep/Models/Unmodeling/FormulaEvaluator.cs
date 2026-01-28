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
        var evaluator = new ExpressionEvaluator();

        foreach(var property in calcElement.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calcElement);
            evaluator.Variables[property.Name] = value;
        }

        try {
            object result = evaluator.Evaluate(formula);
            return Convert.ToDouble(result);
        } catch(Exception) {
            BuildDebugLog(calcElement, formula);
            throw;
        }
    }

    private void BuildDebugLog(CalculationElementBase calcElement, string formula) {
        StringBuilder logBuilder = new StringBuilder();
        string errorMessage = _localizationService.GetLocalizedString("UnmodelingCalculator.Error");
        logBuilder.AppendLine(errorMessage);
        logBuilder.AppendLine($"ElementId: {calcElement.Element.Id}");
        logBuilder.AppendLine($"Formula: {formula}");

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

        Console.WriteLine(logBuilder.ToString());
        throw new OperationCanceledException(errorMessage);
    }
}


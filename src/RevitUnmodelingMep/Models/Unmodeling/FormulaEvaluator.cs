using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

using Autodesk.Revit.DB;

using CodingSeb.ExpressionEvaluator;

using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class FormulaEvaluator {
    private readonly ILocalizationService _localizationService;
    public IMessageBoxService MessageBoxService { get; }
    private readonly Dictionary<Type, ExpressionEvaluator> _evaluators = new();
    private readonly Dictionary<Type, PropertyInfo[]> _propertiesByType = new();

    public FormulaEvaluator(ILocalizationService localizationService, IMessageBoxService messageBoxService) {
        _localizationService = localizationService;
        MessageBoxService = messageBoxService;
    }

    public double Evaluate(string formula, CalculationElementBase calcElement) {
        try {
            // При необходимости проверок расчета включаем LogEvaluation и подаем туда айди элемента который проверяем
            // LogEvaluation(formula, calcElement);
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
        var evaluator = GetEvaluator(calcElement);
        object result = evaluator.Evaluate(formula);
        return Convert.ToDouble(result);
    }

    private ExpressionEvaluator GetEvaluator(CalculationElementBase calcElement) {
        Type elementType = calcElement.GetType();
        if(!_evaluators.TryGetValue(elementType, out ExpressionEvaluator evaluator)) {
            evaluator = new ExpressionEvaluator();
            _evaluators[elementType] = evaluator;
        }

        PropertyInfo[] properties = GetCachedProperties(elementType);
        foreach(var property in properties) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calcElement);
            evaluator.Variables[property.Name] = value;
        }

        return evaluator;
    }

    private PropertyInfo[] GetCachedProperties(Type elementType) {
        if(!_propertiesByType.TryGetValue(elementType, out PropertyInfo[] properties)) {
            properties = elementType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            _propertiesByType[elementType] = properties;
        }

        return properties;
    }

    private void BuildDebugLog(CalculationElementBase calcElement, string formula) {
        string errorMessage = _localizationService.GetLocalizedString("UnmodelingCalculator.Error");
        string log = BuildDebugLogMessage(calcElement, formula, null, errorMessage);
        UserMessageException.Throw(MessageBoxService, errorMessage, log);
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

        return logBuilder.ToString();
    }
}

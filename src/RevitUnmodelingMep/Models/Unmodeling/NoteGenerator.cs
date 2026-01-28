using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class NoteGenerator {
    public string Create(string noteTemplate, IReadOnlyCollection<CalculationElementBase> calculationElements) {
        string template = noteTemplate ?? string.Empty;
        if(string.IsNullOrEmpty(template)) {
            return string.Empty;
        }

        double sumArea = calculationElements.Any()
            ? calculationElements.Sum(r => GetDoublePropertyValue(r, "Area_m2") ?? 0)
            : 0;
        double sumLengthMm = calculationElements.Any()
            ? calculationElements.Sum(r => GetDoublePropertyValue(r, "Length_mm") ?? 0)
            : 0;
        double sumLengthM = calculationElements.Any()
            ? calculationElements.Sum(r => GetDoublePropertyValue(r, "Length_m") ?? 0)
            : 0;
        double count = calculationElements.Count;

        string FormatValue(double value) {
            double rounded = Math.Round(value, 2);
            return rounded.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var tokens = new Dictionary<string, string> {
            { FormulaValidator.NoteTokenSumArea_m2, FormatValue(sumArea) },
            { FormulaValidator.NoteTokenSumLength_mm, FormatValue(sumLengthMm) },
            { FormulaValidator.NoteTokenSumLength_m, FormatValue(sumLengthM) },
            { FormulaValidator.NoteTokenCount, FormatValue(count) }
        };

        string result = template;
        foreach(var token in tokens) {
            string placeholder = "{" + token.Key + "}";
            if(result.Contains(placeholder)) {
                result = result.Replace(placeholder, token.Value);
            }
        }

        return result;
    }

    private static double? GetDoublePropertyValue(object instance, string propertyName) {
        if(instance == null || string.IsNullOrWhiteSpace(propertyName)) {
            return null;
        }

        var property = instance.GetType().GetProperty(propertyName);
        if(property == null || !property.CanRead) {
            return null;
        }

        object value = property.GetValue(instance);
        return value switch {
            null => null,
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            _ => null
        };
    }
}


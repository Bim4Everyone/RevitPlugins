using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class NoteGenerator {
    public string Create(string noteTemplate, IReadOnlyCollection<CalculationElementBase> calculationElements) {
        string template = noteTemplate ?? string.Empty;
        if(string.IsNullOrEmpty(template)) {
            return string.Empty;
        }
        
        string FormatValue(double value) {
            double rounded = Math.Round(value, 2);
            return rounded.ToString("0.##", CultureInfo.InvariantCulture);
        }

        NoteElement noteElement = BuildNoteElement(calculationElements);
        IReadOnlyList<string> tokenNames = NoteElement.GetTokenNames();
        Dictionary<string, string> tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach(string tokenName in tokenNames) {
            if(string.IsNullOrWhiteSpace(tokenName)) {
                continue;
            }

            double? value = GetDoublePropertyValue(noteElement, tokenName);
            tokens[tokenName] = FormatValue(value ?? 0);
        }

        string result = template;
        foreach(var token in tokens) {
            string placeholder = "{" + token.Key + "}";
            if(result.Contains(placeholder)) {
                result = result.Replace(placeholder, token.Value);
            }
        }

        return result;
    }

    private static NoteElement BuildNoteElement(IReadOnlyCollection<CalculationElementBase> calculationElements) {
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

        double stock = ResolveProjectStock(calculationElements);

        return new NoteElement {
            SumArea_m2 = sumArea,
            SumLength_mm = sumLengthMm,
            SumLength_m = sumLengthM,
            Count = count,
            SumAreaWithStock_m2 = sumArea * stock,
            SumLengthWithStock_mm = sumLengthMm * stock,
            SumLengthWithStock_m = sumLengthM * stock
        };
    }

    private static double ResolveProjectStock(IReadOnlyCollection<CalculationElementBase> calculationElements) {
        if(calculationElements == null || calculationElements.Count == 0) {
            return 1;
        }

        CalculationElementBase first = calculationElements.FirstOrDefault();
        double? stock = GetDoublePropertyValue(first, "ProjectStock");
        if(!stock.HasValue || stock.Value <= 0) {
            return 1;
        }

        return stock.Value;
    }

    private static double? GetDoublePropertyValue(object instance, string propertyName) {
        if(instance == null || string.IsNullOrWhiteSpace(propertyName)) {
            return null;
        }

        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.ViewModels;

internal static class FormulaValidator {

    public static string NormalizeFormula(string formula) {
        if(string.IsNullOrEmpty(formula))
            return formula;

        return formula.Replace(',', '.');
    }

    public static bool ValidateFormulas(
        IEnumerable<ConsumableTypeItem> consumableTypes,
        string saveProperty,
        string emptySavePropertyMessage,
        ILocalizationService localizationService,
        Func<ConsumableTypeItem, int?> resolveCategoryId,
        out string errorText) {

        errorText = null;

        if(string.IsNullOrEmpty(saveProperty)) {
            errorText = !string.IsNullOrWhiteSpace(emptySavePropertyMessage)
                ? emptySavePropertyMessage
                : "Validation error.";
            return false;
        }

        ConsumableTypeItem missingFormula = consumableTypes?
            .FirstOrDefault(item => string.IsNullOrWhiteSpace(item?.Formula));

        if(missingFormula != null) {
            string name = GetName(missingFormula);
            string message = localizationService?.GetLocalizedString("FormulaValidator.FormulaError");
            errorText = AppendDetail(message, name);
            return false;
        }

        ConsumableTypeItem formulaWithComma = consumableTypes?
            .FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item?.Formula) &&
                item.Formula.Contains(","));

        if(formulaWithComma != null) {
            string name = GetName(formulaWithComma);
            string message = localizationService?.GetLocalizedString("FormulaValidator.DotError");
            errorText = AppendDetail(message, name);
            return false;
        }

        foreach(ConsumableTypeItem item in consumableTypes ?? Enumerable.Empty<ConsumableTypeItem>()) {
            if(!IsFormulaAllowed(item, resolveCategoryId, out string invalidToken)) {
                string name = GetName(item);
                string message = localizationService?.GetLocalizedString("FormulaValidator.VariableError");
                errorText = AppendDetail(AppendDetail(message, name), invalidToken);
                return false;
            }
        }

        return true;
    }

    private static bool IsFormulaAllowed(
        ConsumableTypeItem item,
        Func<ConsumableTypeItem, int?> resolveCategoryId,
        out string invalidToken) {

        invalidToken = null;
        if(item == null || string.IsNullOrWhiteSpace(item.Formula)) {
            return false;
        }

        int categoryId = resolveCategoryId?.Invoke(item) ?? 0;

        var allowed = GetAllowedPropertyNames(categoryId)
            ?? Enumerable.Empty<string>();

        HashSet<string> allowedSet = new HashSet<string>(
            allowed.Select(a => a?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a)),
            StringComparer.OrdinalIgnoreCase);


        string formulaNoStrings = Regex.Replace(item.Formula, "\"[^\"]*\"", " ");

        foreach(Match match in Regex.Matches(formulaNoStrings, @"[\p{L}_][\p{L}\p{Nd}_]*")) {
            string token = match.Value;
            if(allowedSet.Contains(token)) {
                continue;
            }

            invalidToken = token;
            return false;
        }

        return true;
    }

    private static string GetName(ConsumableTypeItem item) {
        return item.ConsumableTypeName
               ?? item.Title
               ?? item.ConfigKey
               ?? "Unexpected Name";
    }

    private static string AppendDetail(string message, string detail) {
        if(string.IsNullOrWhiteSpace(detail)) {
            return message;
        }

        if(string.IsNullOrWhiteSpace(message)) {
            return detail;
        }

        return $"{message}: {detail}";
    }

    public static IEnumerable<string> GetAllowedPropertyNames(int categoryId) {
        BuiltInCategory category = (BuiltInCategory) categoryId;
        Type elementType = category switch {
            BuiltInCategory.OST_DuctCurves => typeof(CalculationElementDuct),
            BuiltInCategory.OST_PipeCurves => typeof(CalculationElementPipe),
            BuiltInCategory.OST_PipeInsulations => typeof(CalculationElementPipeIns),
            BuiltInCategory.OST_DuctInsulations => typeof(CalculationElementDuctIns),
            BuiltInCategory.OST_PipingSystem => typeof(CalculationElementPipeSystem),
            BuiltInCategory.OST_DuctSystem => typeof(CalculationElementDuctSystem),
            _ => null
        };

        if(elementType == null) {
            return Enumerable.Empty<string>();
        }

        return elementType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead
                        && p.GetIndexParameters().Length == 0
                        && p.Name != nameof(CalculationElementBase.Element)
                        && IsAllowedPropertyType(p.PropertyType))
            .Select(p => p.Name);
    }

    private static bool IsAllowedPropertyType(Type type) {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string)
               || type == typeof(bool)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double)
               || type.IsEnum;
    }
}

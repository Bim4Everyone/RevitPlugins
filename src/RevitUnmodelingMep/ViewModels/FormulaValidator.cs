using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

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
        Func<ConsumableTypeItem, int?> resolveCategoryId,
        out string errorText) {

        errorText = null;

        if(string.IsNullOrEmpty(saveProperty)) {
            errorText = emptySavePropertyMessage;
            return false;
        }

        ConsumableTypeItem missingFormula = consumableTypes?
            .FirstOrDefault(item => string.IsNullOrWhiteSpace(item?.Formula));

        if(missingFormula != null) {
            string name = GetName(missingFormula);
            errorText = $"Заполните формулу: {name}.";
            return false;
        }

        ConsumableTypeItem formulaWithComma = consumableTypes?
            .FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item?.Formula) &&
                item.Formula.Contains(","));

        if(formulaWithComma != null) {
            string name = GetName(formulaWithComma);
            errorText = $"В формуле используется запятая вместо точки: {name}.";
            return false;
        }

        foreach(ConsumableTypeItem item in consumableTypes ?? Enumerable.Empty<ConsumableTypeItem>()) {
            if(!IsFormulaAllowed(item, resolveCategoryId, out string invalidToken)) {
                string name = GetName(item);
                errorText = $"Недопустимая переменная в формуле {name}: {invalidToken}.";
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

        var allowed = GetFormulaPropWhiteList(categoryId)
            ?? Enumerable.Empty<string>();

        HashSet<string> allowedSet = new HashSet<string>(allowed);
        HashSet<string> builtins = new HashSet<string> {
            "PI", "E", "TRUE", "FALSE", "NAN", "INFINITY"
        };

        // Исключаем строковые литералы, чтобы имена внутри кавычек не валидировались как идентификаторы.
        string formulaNoStrings = Regex.Replace(item.Formula, "\"[^\"]*\"", " ");

        foreach(Match match in Regex.Matches(formulaNoStrings, @"[A-Za-z_][A-Za-z0-9_]*")) {
            string token = match.Value;
            if(allowedSet.Contains(token) || builtins.Contains(token)) {
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
               ?? "Неизвестный элемент";
    }

    public static List<string> GetFormulaPropWhiteList(int categoryId) {
        BuiltInCategory category = (BuiltInCategory) categoryId;

        List<string> allowedNames = [];
        if (category == BuiltInCategory.OST_DuctCurves) {
            allowedNames = ["SystemSharedName", "SystemTypeName", "IsRound", "Diameter", "Width", "Height", "Perimeter",
                "Volume", "Area", "Length", "InsulationThikness", "IsInsulated", "InsulationArea"];
            return allowedNames;
        }
        if (category == BuiltInCategory.OST_PipeCurves) {
            allowedNames = ["SystemSharedName", "SystemTypeName", "IsRound", "Diameter", "OutDiameter", "Area",
                "Volume", "Perimeter", "Length", "InsulationThikness", "IsInsulated", "InsulationArea"];
            return allowedNames;
        }
        if(category == BuiltInCategory.OST_PipeInsulations) {
            allowedNames = ["SystemSharedName", "SystemTypeName", "InsulationThikness", "IsRound", "Diameter", 
                "OutDiameter", "Area", "Perimeter", "Length"];
            return allowedNames;
        }
        if(category == BuiltInCategory.OST_DuctInsulations) {
            allowedNames = ["SystemSharedName", "SystemTypeName", "IsRound", "Diameter", "Width", "Height", "Perimeter", 
                "Area", "Length"];
            return allowedNames;
        }
        if(category == BuiltInCategory.OST_PipingSystem || category == BuiltInCategory.OST_DuctSystem) {
            allowedNames = ["SystemTypeName", "SystemSharedName "];
        }

        return allowedNames;
    }
}

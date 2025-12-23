using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.ViewModels;

internal static class FormulaValidator {

    public static string NormalizeFormula(string formula) {
        if(string.IsNullOrEmpty(formula))
            return formula;

        return formula.Replace(',', '.');
    }

    private static List<string> GetFormulaPropWhiteList(int categoryId) {
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
            allowedNames = ["IsRound", "Diameter", "Width", "Height", "Perimeter"];
            return allowedNames;
        }

        return allowedNames;
    }
}

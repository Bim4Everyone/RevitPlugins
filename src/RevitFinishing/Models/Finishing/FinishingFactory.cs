using System;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingFactory {
    public static FinishingElement Create(string name, Element element, FinishingCalculator calculator) {
        var paramService = new ParamCalculationService();

        switch(name) {
            case "Стены":
                return new FinishingWall(element, calculator, paramService);
            case "Перекрытия":
                return new FinishingFloor(element, calculator, paramService);
            case "Потолки":
                return new FinishingCeiling(element, calculator, paramService);
            case "Плинтусы":
                return new FinishingBaseboard(element, calculator, paramService);
            default:
                throw new ArgumentException($"Wrong finishing type {name}");
        }
    }
}

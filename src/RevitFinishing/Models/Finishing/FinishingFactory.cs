using System;

using Autodesk.Revit.DB;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
internal class FinishingFactory {
    public static FinishingElement Create(string name, Element element) {
        var paramService = new ParamCalculationService();

        switch(name) {
            case "Стены":
                return new FinishingWall(element, paramService);
            case "Перекрытия":
                return new FinishingFloor(element, paramService);
            case "Потолки":
                return new FinishingCeiling(element, paramService);
            case "Плинтусы":
                return new FinishingBaseboard(element, paramService);
            default:
                throw new ArgumentException($"Wrong finishing type {name}");
        }
    }
}

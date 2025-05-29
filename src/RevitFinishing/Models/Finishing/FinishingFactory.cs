using System;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFactory {
        public static FinishingElement Create(string name, Element element, FinishingCalculator calculator) {
            ParamCalculationService paramService = new ParamCalculationService();

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
}

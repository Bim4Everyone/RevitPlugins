using System;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    internal class FinishingFactory {
        public static FinishingElement Create(string name, Element element, FinishingCalculator calculator) {
            switch(name) {
                case "Стены":
                    return new FinishingWall(element, calculator);
                case "Перекрытия":
                    return new FinishingFloor(element, calculator);
                case "Потолки":
                    return new FinishingCeiling(element, calculator);
                case "Плинтусы":
                    return new FinishingBaseboard(element, calculator);
                default:
                    throw new ArgumentException($"Wrong finishing type {name}");
            }
        }
    }
}

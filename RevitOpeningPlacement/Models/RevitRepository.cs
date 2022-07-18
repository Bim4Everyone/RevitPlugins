using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitOpeningPlacement.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public static Dictionary<MepCategoriesEnum, string> CategoryNames => new Dictionary<MepCategoriesEnum, string> {
            {MepCategoriesEnum.Pipe, "Трубы" },
            {MepCategoriesEnum.RectangleDuct, "Воздуховоды (прямоугольное сечение)" },
            {MepCategoriesEnum.RoundDuct, "Воздуховоды (круглое сечение)" },
            {MepCategoriesEnum.Tray, "Лотки" }
        };

        public static Dictionary<Parameters, string> ParameterNames => new Dictionary<Parameters, string>() {
            {Parameters.Diameter, "Диаметр" },
            {Parameters.Height, "Высота" },
            {Parameters.Width, "Ширина" }
        };
    }

    internal enum MepCategoriesEnum {
        Pipe,
        RectangleDuct,
        RoundDuct,
        Tray
    }

    internal enum Parameters {
        Height,
        Width,
        Diameter
    }
}
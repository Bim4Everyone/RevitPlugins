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

        public static Dictionary<BuiltInCategory, string> CategoryNames => new Dictionary<BuiltInCategory, string> {
            {BuiltInCategory.OST_PipeCurves, "Трубы" },
            {BuiltInCategory.OST_DuctCurves, "Воздуховоды (прямоугольное сечение)" },
            {BuiltInCategory.OST_DuctCurves, "Воздуховоды (круглое сечение)" },
            {BuiltInCategory.OST_CableTray, "Лотки" },
            {BuiltInCategory.OST_Walls, "Стены" },
            {BuiltInCategory.OST_Floors, "Перекрытия" }
        };

        public static Dictionary<Parameters, string> ParameterNames => new Dictionary<Parameters, string>() {
            {Parameters.Diameter, "Диаметр" },
            {Parameters.Height, "Высота" },
            {Parameters.Width, "Ширина" }
        };
    }


    internal enum Parameters {
        Height,
        Width,
        Diameter
    }
}
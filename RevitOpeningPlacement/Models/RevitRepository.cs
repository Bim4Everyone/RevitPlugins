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

        public static Dictionary<Categories, string> CategoryNames => new Dictionary<Categories, string> {
            {Categories.Pipe, "Трубы" },
            {Categories.RectangleDuct, "Воздуховоды (прямоугольное сечение)" },
            {Categories.RoundDuct, "Воздуховоды (круглое сечение)" },
            {Categories.Tray, "Лотки" }
        };
    }

    internal enum Categories {
        Pipe,
        RectangleDuct,
        RoundDuct,
        Tray
    }
}
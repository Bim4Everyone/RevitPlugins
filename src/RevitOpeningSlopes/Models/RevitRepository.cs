using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitOpeningSlopes.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        public View ActiveView => ActiveUIDocument.ActiveView;
        public XYZ GetOpeningLocation(Element opening) {
            if(opening.Location is LocationPoint locationPoint) {
                return locationPoint.Point;

            } else {
                throw new ArgumentException("Расположение не является точкой");
            };
        }
        public XYZ GetOpeningVector(FamilyInstance opening) {
            return opening.FacingOrientation.Normalize();
        }
        public IList<FamilyInstance> GetWindows() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .Where(el => el is FamilyInstance)
                .Cast<FamilyInstance>()
                .ToList();
        }
        public IList<FamilySymbol> GetFamilySymbols() {
            string familyName = "Откос_V2";
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Where(el => el is FamilySymbol fs && fs.Family.Name == familyName)
                .Cast<FamilySymbol>()
                .ToList();
        }
        public double ConvertToFeet(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }
        public double ConvertToMillimeters(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }
    }
}

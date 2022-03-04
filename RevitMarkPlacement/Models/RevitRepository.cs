using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.ViewModels;

namespace RevitMarkPlacement.Models {
    internal class RevitRepository : BaseViewModel {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public IEnumerable<string> GetSpotDimentionTypeNames(ISelectionMode selectionMode) {
            return selectionMode.GetSpotDimentions(_document)
                .Select(s => s.SpotDimensionType.Name)
                .Distinct();
        }

        public IEnumerable<GlobalParameterViewModel> GetGlobalParameters() {
            return GlobalParametersManager
                .GetGlobalParametersOrdered(_document)
                .Select(id=>_document.GetElement(id))
                .Cast<GlobalParameter>()
                .Where(p=>p.GetValue() is DoubleParameterValue)
                .Select(p => new GlobalParameterViewModel(p.Name, GetValue(p)));
        }

        public FamilyInstance CreateAnnotation(FamilySymbol symbol, XYZ point, View view) {
            return _document.Create.NewFamilyInstance(point, symbol, view);
        }

        public Element GetElement(ElementId elementId) {
            return _document.GetElement(elementId);
        }

        public FamilySymbol GetAnnotationSymbolType(string typeName, string familyName) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(item => IsNeededAnnotationSymbol(item, typeName, familyName));
        }

        public IEnumerable<SpotDimension> GetSpotDimensions(ISelectionMode selectionMode) {
            return selectionMode.GetSpotDimentions(_document);
        }

        public IEnumerable<AnnotationSymbol> GetAnnotations() {
            var familyNames = new List<string>(){
                "ТипАн_Отметка_ТипЭт_Разрез_Вверх",
                "ТипАн_Отметка_ТипЭт_Разрез_Вниз"
            };
            return new FilteredElementCollector(_document)
                .OfClass(typeof(FamilyInstance))
                .OfType<AnnotationSymbol>()
                .Where(item => IsNeededAnnotationInstance(item, familyNames));
        }

        public Transaction StartTransaction(string text) {
            var t = new Transaction(_document);
            t.BIMStart(text);
            return t;
        }

        private bool IsNeededAnnotationSymbol(FamilySymbol symbol, string typeName, string familyName) {
            return symbol.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase)
                    && symbol.Family.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool IsNeededAnnotationInstance(AnnotationSymbol instance, IEnumerable<string> familyNames) {
            return instance.Symbol != null
                    && instance.Symbol.Family != null
                    && familyNames.Any(n => n.Equals(instance.Symbol.Family.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public SpotOrientation GetSpotOrientation(SpotDimension spot) {
            var bb = spot.get_BoundingBox(spot.View);
            if(spot.Origin.Z < bb.Max.Z) {
                if(spot.Origin.X < bb.Max.X) {
                    return SpotOrientation.RightTop;
                } else {
                    return SpotOrientation.LeftTop;
                }
            } else {
                if(spot.Origin.X < bb.Max.X) {
                    return SpotOrientation.RightBottom;
                } else {
                    return SpotOrientation.LeftBottom;
                }
            }
        }

        private double GetValue(GlobalParameter parameter) {
#if D2020 || R2020
            return Math.Round(UnitUtils.ConvertFromInternalUnits((parameter.GetValue() as DoubleParameterValue).Value, DisplayUnitType.DUT_MILLIMETERS));
#else
            return Math.Round(UnitUtils.ConvertFromInternalUnits((parameter.GetValue() as DoubleParameterValue).Value, UnitTypeId.Millimeters));
#endif
        }
    }

    internal enum SpotOrientation {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    }
}

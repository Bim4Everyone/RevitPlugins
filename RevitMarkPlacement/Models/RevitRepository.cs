using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Revit.Comparators;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.ViewModels;

namespace RevitMarkPlacement.Models {
    internal class RevitRepository : BaseViewModel {
        public const string FamilyTop = "ТипАн_Отметка_ТипЭт_Разрез_Вверх";
        public const string FamilyBottom = "ТипАн_Отметка_ТипЭт_Разрез_Вниз";
        public const string TypeTop = "Вверх";
        public const string TypeBottom = "Вниз";
        public const string LevelCountParam = "Количество типовых этажей";
        public const string FirstLevelOnParam = "Вкл_Уровень_1";
        public const string SpotDimensionIdParam = "Id высотной отметки";
        public const string TemplateLevelHeightParam = "Высота типового этажа";
        public const string FirstLevelParam = "Уровень_1";
        public const string ElevSymbolWidth = "Длина полки";
        public const string ElevSymbolHeight = "Высота полки";

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

        public IEnumerable<SpotDimensionType> GetSpotDimentionTypeNames(ISelectionMode selectionMode) {
            return selectionMode.GetSpotDimentions(_document)
                .Select(s => s.SpotDimensionType)
                .Distinct(new ElementNameEquatable<SpotDimensionType>());
        }

        public IEnumerable<GlobalParameter> GetDoubleGlobalParameters() {
            return GlobalParametersManager
                .GetGlobalParametersOrdered(_document)
                .Select(id => _document.GetElement(id))
                .Cast<GlobalParameter>()
                .Where(p => p.GetValue() is DoubleParameterValue);
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
                FamilyTop,
                FamilyBottom
            };
            return new FilteredElementCollector(_document)
                .OfClass(typeof(FamilyInstance))
                .OfType<AnnotationSymbol>()
                .Where(item => IsNeededAnnotationInstance(item, familyNames));
        }

        public TransactionGroup StartTransactionGroup(string text) {
            var t = new TransactionGroup(_document);
            t.BIMStart(text);
            return t;
        }

        public Transaction StartTransaction(string text) {
            var t = new Transaction(_document);
            t.BIMStart(text);
            return t;
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

        public void MirrorAnnotation(FamilyInstance annotation) {
            if(annotation.Location is LocationPoint point) {
                Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(1, 0, 0), point.Point);
                ElementTransformUtils.MirrorElement(_document, annotation.Id, plane);
                _document.Delete(annotation.Id);
            }
        }

        public void DeleteElement(Element element) {
            using(Transaction t = new Transaction(_document)) {
                t.BIMStart("Удаление элемента");
                _document.Delete(element.Id);
                t.Commit();
            }
        }

        public AnnotationsSettings GetSettings(AnnotationsConfig congig) {
            return congig.GetSettings(_document);
        }
        public AnnotationsSettings AddSettings(AnnotationsConfig congig) {
            return congig.AddSettings(_document);
        }

        public Family GetTopAnnotaionFamily() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(item => item.Name.Equals(FamilyTop, StringComparison.CurrentCultureIgnoreCase));
        }

        public Family GetBottomAnnotaionFamily() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(item => item.Name.Equals(FamilyBottom, StringComparison.CurrentCultureIgnoreCase));
        }

        public Document GetFamilyDocument(Family family) {
            return _document.EditFamily(family);
        }

        public IEnumerable<FamilySymbol> GetElevationSymbols() {
            return new FilteredElementCollector(_document)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Select(item => _document.GetElement((ElementId) item.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL)))
            .OfType<FamilySymbol>();
        }

        private bool IsNeededAnnotationSymbol(FamilySymbol symbol, string typeName, string familyName) {
            return symbol.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase)
                    && symbol.Family.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool IsNeededAnnotationInstance(AnnotationSymbol instance, IEnumerable<string> familyNames) {
            return instance.Symbol != null
                    && instance.Symbol.Family != null
                    && familyNames.Contains(instance.Symbol.Family.Name, StringComparer.CurrentCultureIgnoreCase);
        }
    }

    internal enum SpotOrientation {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom
    }

    internal class ElementNameEquatable<T> : IEqualityComparer<T> where T : Element {
        public bool Equals(T x, T y) {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(T obj) {
            return obj?.Id.GetHashCode() ?? 0;
        }
    }
}

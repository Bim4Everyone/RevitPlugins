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
        public const string FilterSpotName = "_(auto)";
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

        public IEnumerable<FamilySymbol> GetAnnotationSymbols() {
            var families = new[] { FamilyTop, FamilyBottom };
            var types = new[] { TypeTop, TypeBottom };
            return new FilteredElementCollector(_document)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(item =>
                    families.Any(f => f.Equals(item.Family.Name, StringComparison.CurrentCultureIgnoreCase))
                    && types.Any(t => t.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)));
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

        public IAnnotationPosition GetSpotOrientation(SpotDimension spot, IEnumerable<FamilySymbol> symbols) {
            var bb = spot.get_BoundingBox(spot.View);
            var dir = spot.View.RightDirection;
            var bbDir = new XYZ((bb.Max - bb.Min).X, (bb.Max - bb.Min).Y, 0).Normalize();
            XYZ Max = bb.Max;
            XYZ Min = bb.Min;
            if(Math.Abs(dir.AngleTo(bbDir) - Math.PI) > 0.01 && Math.Abs(dir.AngleTo(bbDir)) > 0.01) {
                Max = new XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z);
                Min = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);
            }
            var minOriginDist = Math.Sqrt(Math.Pow(spot.Origin.X - Min.X, 2) + Math.Pow(spot.Origin.Y - Min.Y, 2));
            var maxOriginDist = Math.Sqrt(Math.Pow(spot.Origin.X - Max.X, 2) + Math.Pow(spot.Origin.Y - Max.Y, 2));
            var maxMinDist = Math.Sqrt(Math.Pow(Min.X - Max.X, 2) + Math.Pow(Min.Y - Max.Y, 2));
            if(spot.Origin.Z < Max.Z && Math.Abs(spot.Origin.Z - Max.Z) > 0.01) {
                if(new XYZ(Min.X + dir.X * maxMinDist, Min.Y + dir.Y * maxMinDist, Min.Z).DistanceTo(new XYZ(Max.X, Max.Y, Min.Z)) < 0.01) {
                    if(minOriginDist > maxOriginDist) {
                        return new LeftTopAnnotation(symbols, spot.View.RightDirection);
                    } else {
                        return new RightTopAnnotation(symbols, spot.View.RightDirection);
                    }
                } else {
                    if(minOriginDist > maxOriginDist) {
                        return new RightTopAnnotation(symbols, spot.View.RightDirection);
                    } else {
                        return new LeftTopAnnotation(symbols, spot.View.RightDirection);
                    }
                }
            } else {
                if(new XYZ(Min.X + dir.X * maxMinDist, Min.Y + dir.Y * maxMinDist, Min.Z).DistanceTo(new XYZ(Max.X, Max.Y, Min.Z)) < 0.01) {
                    if(minOriginDist > maxOriginDist) {
                        return new LeftBottomAnnotation(symbols, spot.View.RightDirection);
                    } else {
                        return new RightBottomAnnotation(symbols, spot.View.RightDirection);
                    }
                } else {
                    if(minOriginDist > maxOriginDist) {
                        return new RightBottomAnnotation(symbols, spot.View.RightDirection);
                    } else {
                        return new LeftBottomAnnotation(symbols, spot.View.RightDirection);
                    }
                }
            }
        }

        public void MirrorAnnotation(FamilyInstance annotation, XYZ axis) {
            using(Transaction t = _document.StartTransaction("Отражение аннотации")) {
                if(annotation.Location is LocationPoint point) {
                    Plane plane = Plane.CreateByNormalAndOrigin(axis, point.Point);
                    ElementTransformUtils.MirrorElement(_document, annotation.Id, plane);
                    _document.Delete(annotation.Id);
                }
                t.Commit();
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

    internal class ElementNameEquatable<T> : IEqualityComparer<T> where T : Element {
        public bool Equals(T x, T y) {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(T obj) {
            return obj?.Id.GetHashCode() ?? 0;
        }
    }
}

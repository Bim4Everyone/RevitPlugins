using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models.Positions;

namespace RevitMarkPlacement.Models;

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

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }
    
    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    
    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;
    
    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;

    public IEnumerable<SpotDimensionType> GetSpotDimensionTypes(ISpotDimensionSelection selection) {
        return selection.GetElements()
            .Select(s => s.SpotDimensionType)
            .Distinct(new ElementNameEquatable<SpotDimensionType>());
    }

    public FamilyInstance CreateAnnotation(FamilySymbol symbol, XYZ point, View view) {
        return Document.Create.NewFamilyInstance(point, symbol, view);
    }

    public Element GetElement(ElementId elementId) {
        return Document.GetElement(elementId);
    }

    public FamilySymbol GetAnnotationSymbolType(string typeName, string familyName) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(item => IsNeededAnnotationSymbol(item, typeName, familyName));
    }

    public IEnumerable<FamilySymbol> GetAnnotationSymbols() {
        string[] families = new[] { FamilyTop, FamilyBottom };
        string[] types = new[] { TypeTop, TypeBottom };
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .Where(item =>
                families.Any(f => f.Equals(item.Family.Name, StringComparison.CurrentCultureIgnoreCase))
                && types.Any(t => t.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)));
    }

    public IEnumerable<SpotDimension> GetSpotDimensions(ISpotDimensionSelection selection) {
        return selection.GetElements();
    }

    public IEnumerable<AnnotationSymbol> GetAnnotations() {
        var familyNames = new List<string> {
            FamilyTop,
            FamilyBottom
        };
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilyInstance))
            .OfType<AnnotationSymbol>()
            .Where(item => IsNeededAnnotationInstance(item, familyNames));
    }

    public TransactionGroup StartTransactionGroup(string text) {
        var t = new TransactionGroup(Document);
        t.BIMStart(text);
        return t;
    }

    public Transaction StartTransaction(string text) {
        var t = new Transaction(Document);
        t.BIMStart(text);
        return t;
    }

    public IAnnotationPosition GetSpotOrientation(SpotDimension spot, IEnumerable<FamilySymbol> symbols) {
        var bb = spot.get_BoundingBox(spot.View);
        var dir = spot.View.RightDirection;
        var bbDir = new XYZ((bb.Max - bb.Min).X, (bb.Max - bb.Min).Y, 0).Normalize();
        var Max = bb.Max;
        var Min = bb.Min;
        if(Math.Abs(dir.AngleTo(bbDir) - Math.PI) > 0.01
           && Math.Abs(dir.AngleTo(bbDir)) > 0.01) {
            Max = new XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z);
            Min = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);
        }

        double minOriginDist = Math.Sqrt(Math.Pow(spot.Origin.X - Min.X, 2) + Math.Pow(spot.Origin.Y - Min.Y, 2));
        double maxOriginDist = Math.Sqrt(Math.Pow(spot.Origin.X - Max.X, 2) + Math.Pow(spot.Origin.Y - Max.Y, 2));
        double maxMinDist = Math.Sqrt(Math.Pow(Min.X - Max.X, 2) + Math.Pow(Min.Y - Max.Y, 2));
        if(spot.Origin.Z < Max.Z
           && Math.Abs(spot.Origin.Z - Max.Z) > 0.01) {
            if(new XYZ(Min.X + dir.X * maxMinDist, Min.Y + dir.Y * maxMinDist, Min.Z).DistanceTo(
                   new XYZ(Max.X, Max.Y, Min.Z))
               < 0.01) {
                if(minOriginDist > maxOriginDist) {
                    return new LeftTopAnnotation(spot.View.RightDirection, symbols);
                }

                return new RightTopAnnotation(spot.View.RightDirection, symbols);
            }

            if(minOriginDist > maxOriginDist) {
                return new RightTopAnnotation(spot.View.RightDirection, symbols);
            }

            return new LeftTopAnnotation(spot.View.RightDirection, symbols);
        }

        if(new XYZ(Min.X + dir.X * maxMinDist, Min.Y + dir.Y * maxMinDist, Min.Z).DistanceTo(
               new XYZ(Max.X, Max.Y, Min.Z))
           < 0.01) {
            if(minOriginDist > maxOriginDist) {
                return new LeftBottomAnnotation(spot.View.RightDirection, symbols);
            }

            return new RightBottomAnnotation(spot.View.RightDirection, symbols);
        }

        if(minOriginDist > maxOriginDist) {
            return new RightBottomAnnotation(spot.View.RightDirection, symbols);
        }

        return new LeftBottomAnnotation(spot.View.RightDirection, symbols);
    }

    public void MirrorAnnotation(FamilyInstance annotation, XYZ axis) {
        using(var t = Document.StartTransaction("Отражение аннотации")) {
            if(annotation.Location is LocationPoint point) {
                var plane = Plane.CreateByNormalAndOrigin(axis, point.Point);
                ElementTransformUtils.MirrorElement(Document, annotation.Id, plane);
                Document.Delete(annotation.Id);
            }

            t.Commit();
        }
    }

    public void DeleteElement(Element element) {
        using(var t = new Transaction(Document)) {
            t.BIMStart("Удаление элемента");
            Document.Delete(element.Id);
            t.Commit();
        }
    }

    public Family GetTopAnnotaionFamily() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(item => item.Name.Equals(FamilyTop, StringComparison.CurrentCultureIgnoreCase));
    }

    public Family GetBottomAnnotaionFamily() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(item => item.Name.Equals(FamilyBottom, StringComparison.CurrentCultureIgnoreCase));
    }

    public Document GetFamilyDocument(Family family) {
        return Document.EditFamily(family);
    }

    public IEnumerable<FamilySymbol> GetElevationSymbols() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(SpotDimension))
            .OfType<SpotDimension>()
            .Select(item => Document.GetElement(
                (ElementId) item.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL)))
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

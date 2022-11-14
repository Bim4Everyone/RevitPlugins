using System.Collections.Generic;
using System.Linq;

using dosymep.Revit;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using System;
using RevitClashDetective.Models;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;

namespace RevitOpeningPlacement.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        private readonly RevitClashDetective.Models.RevitRepository _clashRevitRepository;

        public RevitRepository(Application application, Document document) {

            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _clashRevitRepository = new RevitClashDetective.Models.RevitRepository(_application, _document);

            UIApplication = _uiApplication;
            DocInfos = GetDocInfos();
        }

        public UIApplication UIApplication { get; }
        public List<DocInfo> DocInfos { get; }

        public static Dictionary<MepCategoryEnum, string> MepCategoryNames => new Dictionary<MepCategoryEnum, string> {
            {MepCategoryEnum.Pipe, "Трубы" },
            {MepCategoryEnum.RectangleDuct, "Воздуховоды (прямоугольное сечение)" },
            {MepCategoryEnum.RoundDuct, "Воздуховоды (круглое сечение)" },
            {MepCategoryEnum.CableTray, "Лотки" },
            {MepCategoryEnum.Conduit, "Короба" }
        };

        public static Dictionary<StructureCategoryEnum, string> StructureCategoryNames => new Dictionary<StructureCategoryEnum, string> {
            {StructureCategoryEnum.Wall, "Стены" },
            {StructureCategoryEnum.Floor, "Перекрытия" },
        };

        public static Dictionary<Parameters, string> ParameterNames => new Dictionary<Parameters, string>() {
            {Parameters.Diameter, "Диаметр" },
            {Parameters.Height, "Высота" },
            {Parameters.Width, "Ширина" }
        };

        public static Dictionary<OpeningType, string> FamilyName => new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В перекрытии" },
            {OpeningType.FloorRound, "ОбщМд_Отв_Отверстие_Круглое_В перекрытии" },
            {OpeningType.WallRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В стене" },
            {OpeningType.WallRound, "ОбщМд_Отв_Отверстие_Круглое_В стене" },
        };

        public static Dictionary<OpeningType, string> TypeName => new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Прямоугольное" },
            {OpeningType.FloorRound, "Круглое" },
            {OpeningType.WallRectangle, "Прямоугольное" },
            {OpeningType.WallRound, "Круглое" },
        };

        public static string OpeningDiameter => "ADSK_Размер_Диаметр";
        public static string OpeningThickness => "ADSK_Размер_Глубина";
        public static string OpeningHeight => "ADSK_Размер_Высота";
        public static string OpeningWidth => "ADSK_Размер_Ширина";

        public static List<BuiltInParameter> MepCurveDiameters => new List<BuiltInParameter>() {
            BuiltInParameter.RBS_PIPE_OUTER_DIAMETER,
            BuiltInParameter.RBS_CURVE_DIAMETER_PARAM,
            BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM
        };

        public static List<BuiltInParameter> MepCurveHeights => new List<BuiltInParameter>() {
            BuiltInParameter.RBS_CURVE_HEIGHT_PARAM,
            BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM
        };

        public static List<BuiltInParameter> MepCurveWidths => new List<BuiltInParameter>() {
            BuiltInParameter.RBS_CURVE_WIDTH_PARAM,
            BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM
        };

        public static List<BuiltInParameter> BottomElevation => new List<BuiltInParameter>() {
            BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION,
            BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION,
            BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION
        };

        public static List<BuiltInParameter> TopElevation => new List<BuiltInParameter>() {
            BuiltInParameter.RBS_CTC_TOP_ELEVATION,
            BuiltInParameter.RBS_DUCT_TOP_ELEVATION,
            BuiltInParameter.RBS_PIPE_TOP_ELEVATION
        };

        public FamilySymbol GetOpeningType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .FirstOrDefault(item => item.Name.Equals(TypeName[type]) && item.FamilyName.Equals(FamilyName[type]));
        }

        public Transaction GetTransaction(string transactionName) {
            return _document.StartTransaction(transactionName);
        }

        public Level GetLevel(string name) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(item => item.Name.Equals(name, StringComparison.CurrentCulture));
        }

        public string GetLevel(Element element) {
            return _clashRevitRepository.GetLevel(element);
        }

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(string fileName, int id) {
            return _clashRevitRepository.GetElement(fileName, id);
        }

        public void SelectAndShowElement(ICollection<Element> elements) {
            _clashRevitRepository.SelectAndShowElement(elements);
        }

        public string GetDocumentName(Document doc) {
            return _clashRevitRepository.GetDocumentName(doc);
        }

        public FamilyInstance CreateInstance(FamilySymbol type, XYZ point, Level level) {
            if(level != null) {
                if(!type.IsActive) {
                    type.Activate();
                }
                point = point - XYZ.BasisZ * level.ProjectElevation;
                return _document.Create.NewFamilyInstance(point, type, level, StructuralType.NonStructural);
            }
            return _document.Create.NewFamilyInstance(point, type, StructuralType.NonStructural);
        }

        public void RotateElement(Element element, XYZ point, Rotates angle) {
            if(point != null) {
                RotateElement(element, point, Line.CreateBound(point, new XYZ(point.X + 1, point.Y, point.Z)), angle.X);
                RotateElement(element, point, Line.CreateBound(point, new XYZ(point.X, point.Y + 1, point.Z)), angle.Y);
                RotateElement(element, point, Line.CreateBound(point, new XYZ(point.X, point.Y, point.Z + 1)), angle.Z);
            }
        }

        public IEnumerable<FamilyInstance> GetOpenings() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfType<FamilyInstance>()
                .OfType<FamilyInstance>()
                .Where(item => TypeName.Any(n => n.Value.Equals(item.Name))
                            && FamilyName.Any(n => n.Value.Equals(GetFamilyName(item))))
                .ToList();
        }

        public string GetFamilyName(Element element) {
            if(element is ElementType type) {
                return type.FamilyName;
            }
            var typeId = element.GetTypeId();
            if(typeId.IsNotNull()) {
                type = _document.GetElement(typeId) as ElementType;
                return type?.FamilyName;
            }
            return null;
        }

        public List<DocInfo> GetDocInfos() {
            return _clashRevitRepository.DocInfos;
        }

        public RevitClashDetective.Models.RevitRepository GetClashRevitRepository() {
            return _clashRevitRepository;
        }

        public Transform GetTransform(Element element) {
            return DocInfos.FirstOrDefault(item => item.Name.Equals(GetDocumentName(element.Document), StringComparison.CurrentCultureIgnoreCase))?.Transform
                ?? Transform.Identity;
        }

        private void RotateElement(Element element, XYZ point, Line axis, double angle) {
            if(Math.Abs(angle) > 0.00001) {
                ElementTransformUtils.RotateElement(_document, element.Id, axis, angle);
            }
        }
    }


    internal enum Parameters {
        Height,
        Width,
        Diameter
    }

    internal enum MepCategoryEnum {
        Pipe,
        RectangleDuct,
        RoundDuct,
        CableTray,
        Conduit
    }

    internal enum StructureCategoryEnum {
        Wall,
        Floor,
    }

    internal enum OpeningType {
        WallRound,
        WallRectangle,
        FloorRound,
        FloorRectangle
    }
}
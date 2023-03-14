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
using RevitClashDetective.Models.Handlers;
using RevitOpeningPlacement.Models.OpeningPlacement;
using System.Threading.Tasks;
using RevitOpeningPlacement.Models.RevitViews;
using System.Text.RegularExpressions;

namespace RevitOpeningPlacement.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        private readonly RevitClashDetective.Models.RevitRepository _clashRevitRepository;
        private readonly RevitEventHandler _revitEventHandler;

        private View3DProvider _view3DProvider;
        private View3D _view;

        public RevitRepository(Application application, Document document) {

            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _clashRevitRepository = new RevitClashDetective.Models.RevitRepository(_application, _document);
            _revitEventHandler = new RevitEventHandler();

            _view3DProvider = new View3DProvider();
            _view = _view3DProvider.GetView(_document, $"BIM_Задания на отверстия_{_application.Username}");

            UIApplication = _uiApplication;
            DocInfos = GetDocInfos();
        }

        public UIApplication UIApplication { get; }
        public List<DocInfo> DocInfos { get; }

        public static Dictionary<MepCategoryEnum, string> MepCategoryNames { get; } = new Dictionary<MepCategoryEnum, string> {
            {MepCategoryEnum.Pipe, "Трубы" },
            {MepCategoryEnum.RectangleDuct, "Воздуховоды (прямоугольное сечение)" },
            {MepCategoryEnum.RoundDuct, "Воздуховоды (круглое сечение)" },
            {MepCategoryEnum.CableTray, "Лотки" },
            {MepCategoryEnum.Conduit, "Короба" }
        };

        public static Dictionary<FittingCategoryEnum, string> FittingCategoryNames { get; } = new Dictionary<FittingCategoryEnum, string> {
            {FittingCategoryEnum.CableTrayFitting, "Соединительные детали кабельных лотков" },
            {FittingCategoryEnum.DuctFitting, "Соединительные детали воздуховодов" },
            {FittingCategoryEnum.ConduitFitting, "Соединительные детали коробов" },
            {FittingCategoryEnum.PipeFitting, "Соединительные детали трубопроводов" },
        };

        public static Dictionary<StructureCategoryEnum, string> StructureCategoryNames { get; } = new Dictionary<StructureCategoryEnum, string> {
            {StructureCategoryEnum.Wall, "Стены" },
            {StructureCategoryEnum.Floor, "Перекрытия" },
        };

        public static Dictionary<Parameters, string> ParameterNames { get; } = new Dictionary<Parameters, string>() {
            {Parameters.Diameter, "Диаметр" },
            {Parameters.Height, "Высота" },
            {Parameters.Width, "Ширина" }
        };

        public static Dictionary<OpeningType, string> FamilyName { get; } = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В перекрытии" },
            {OpeningType.FloorRound, "ОбщМд_Отв_Отверстие_Круглое_В перекрытии" },
            {OpeningType.WallRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В стене" },
            {OpeningType.WallRound, "ОбщМд_Отв_Отверстие_Круглое_В стене" },
        };

        public static List<string> WallFamilyNames { get; } = new List<string> {
            FamilyName[OpeningType.WallRound],
            FamilyName[OpeningType.WallRectangle]
        };

        public static List<string> FloorFamilyNames { get; } = new List<string> {
            FamilyName[OpeningType.FloorRectangle],
            FamilyName[OpeningType.FloorRound]
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

        public static string SystemCheck => "Системная проверка";

        /// <summary>
        /// Шаблон названий АР файлов в соответствии с BIM стандартом: \\uprav-stroy.loc\corparate\Департаменты\Проектный институт\Типовые ТЗ\BIM-стандарт A101
        /// </summary>
        private static Regex _RegexAR => new Regex(@"^.+_AR.*$");

        /// <summary>
        /// Шаблон названий КР файлов в соответствии с BIM стандартом: \\uprav-stroy.loc\corparate\Департаменты\Проектный институт\Типовые ТЗ\BIM-стандарт A101
        /// </summary>
        private static Regex _RegexKR => new Regex(@"^.+_(KR|KM).*$");

        /// <summary>
        /// Шаблон названий файлов инженерных систем в соответствии с BIM стандартом: \\uprav-stroy.loc\corparate\Департаменты\Проектный институт\Типовые ТЗ\BIM-стандарт A101
        /// </summary>
        private static Regex _RegexMEP => new Regex(@"^.+_(OV|ITP|HC|VK|EOM|EG|SS).*$");

        public FamilySymbol GetOpeningType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .FirstOrDefault(item => item.Name.Equals(TypeName[type]) && item.FamilyName.Equals(FamilyName[type]));
        }

        public Family GetFamily(OpeningType openingType) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .OfType<Family>()
                .FirstOrDefault(item => item?.Name?.Equals(FamilyName[openingType], StringComparison.CurrentCulture) == true);
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

        public string GetLevelName(Element element) {
            return _clashRevitRepository.GetLevelName(element);
        }

        public Level GetLevel(Element element) {
            return _clashRevitRepository.GetLevel(element);
        }

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(string fileName, int id) {
            return _clashRevitRepository.GetElement(fileName, id);
        }

        public void SelectAndShowElement(ICollection<Element> elements) {
            _clashRevitRepository.SelectAndShowElement(elements, _view);
        }

        public string GetDocumentName(Document doc) {
            return _clashRevitRepository.GetDocumentName(doc);
        }

        /// <summary>
        /// Возвращает название файла репозитория без суффикса пользователя и суффикса "отсоединено"
        /// </summary>
        /// <returns></returns>
        public string GetDocumentName() {
            return _clashRevitRepository.GetDocumentName();
        }

        /// <summary>
        /// Возвращает раздел проектирования <see cref="_document">файла репозитория</see>
        /// </summary>
        /// <returns></returns>
        public DocTypeEnum GetDocumentType() {
            string docName = GetDocumentName();
            if(_RegexAR.IsMatch(docName)) {
                return DocTypeEnum.AR;
            }
            if(_RegexKR.IsMatch(docName)) {
                return DocTypeEnum.KR;
            }
            if(_RegexMEP.IsMatch(docName)) {
                return DocTypeEnum.MEP;
            }
            return DocTypeEnum.NotDefined;
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

        /// <summary>
        /// Возвращает список экземпляров семейств-заданий на отверстия из текущего файла ревит ("исходящие" задания).
        /// </summary>
        /// <returns>Список экземпляров семейств, названия семейств и типов которых заданы в соответствующих словарях
        /// <see cref="TypeName">названий типов</see> и
        /// <see cref="FamilyName">названий семейств</see></returns>
        public List<FamilyInstance> GetOpeningsTaskFromCurrentDoc() {
            return GetFamilyInstances()
                .Where(item => TypeName.Any(n => n.Value.Equals(item.Name))
                            && FamilyName.Any(n => n.Value.Equals(GetFamilyName(item))))
                .ToList();
        }

        public List<FamilyInstance> GetWallOpenings() {
            var wallTypes = new[] { OpeningType.WallRectangle, OpeningType.WallRound };
            return GetOpenings(wallTypes);
        }

        public List<FamilyInstance> GetFloorOpenings() {
            var floorTypes = new[] { OpeningType.FloorRectangle, OpeningType.FloorRound };
            return GetOpenings(floorTypes);
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

        public void DeleteAllOpenings() {
            var openings = GetOpeningsTaskFromCurrentDoc();
            using(Transaction t = _document.StartTransaction("Удаление старых заданий на отверстия")) {
                _document.Delete(openings.Select(item => item.Id).ToArray());
                t.Commit();
            }
        }

        public void DeleteElements(ICollection<Element> elements) {
            using(Transaction t = _document.StartTransaction("Удаление объединенных заданий на отверстия")) {
                _document.Delete(elements.Select(item => item.Id).ToArray());
                t.Commit();
            }
        }

        public async Task<FamilyInstance> UniteOpenings(OpeningPlacer placer, ICollection<Element> elements) {
            FamilyInstance createdOpening = null;
            _revitEventHandler.TransactAction = () => {
                using(var t = GetTransaction("Объединение отверстий")) {
                    createdOpening = placer.Place();
                    t.Commit();
                }
                DeleteElements(elements);
            };

            await _revitEventHandler.Raise();

            return createdOpening;
        }

        public void DoAction(Action action) {
            _clashRevitRepository.DoAction(action);
        }

        private void RotateElement(Element element, XYZ point, Line axis, double angle) {
            if(Math.Abs(angle) > 0.00001) {
                ElementTransformUtils.RotateElement(_document, element.Id, axis, angle);
            }
        }

        private IEnumerable<FamilyInstance> GetFamilyInstances() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfType<FamilyInstance>();
        }

        private List<FamilyInstance> GetOpenings(ICollection<OpeningType> types) {
            return GetFamilyInstances()
               .Where(item => types.Any(e => TypeName[e].Equals(item.Name))
                           && types.Any(e => FamilyName[e].Equals(GetFamilyName(item))))
               .ToList();
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

    internal enum FittingCategoryEnum {
        PipeFitting,
        CableTrayFitting,
        DuctFitting,
        ConduitFitting
    }

    internal enum StructureCategoryEnum {
        Wall,
        Floor,
    }

    /// <summary>
    /// Типы проемов - заданий на отверстия
    /// </summary>
    internal enum OpeningType {
        /// <summary>
        /// Круглый проем в стене
        /// </summary>
        WallRound,
        /// <summary>
        /// Прямоугольный проем в стене
        /// </summary>
        WallRectangle,
        /// <summary>
        /// Круглый проем в перекрытии
        /// </summary>
        FloorRound,
        /// <summary>
        /// Прямоугольный проем в перекрытии
        /// </summary>
        FloorRectangle
    }

    /// <summary>
    /// Типы файов ревита в соответствии с шифрами разделов проектирования
    /// </summary>
    internal enum DocTypeEnum {
        /// <summary>
        /// Архитектурный раздел
        /// </summary>
        AR,
        /// <summary>
        /// Конструкторский раздел
        /// </summary>
        KR,
        /// <summary>
        /// Раздел инженерных систем
        /// </summary>
        MEP,
        /// <summary>
        /// Раздел не определен
        /// </summary>
        NotDefined
    }
}
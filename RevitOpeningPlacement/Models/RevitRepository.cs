using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.RevitViews;
using RevitOpeningPlacement.OpeningModels;

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

        public const string OpeningDiameter = "ADSK_Размер_Диаметр";
        public const string OpeningThickness = "ADSK_Размер_Глубина";
        public const string OpeningHeight = "ADSK_Размер_Высота";
        public const string OpeningWidth = "ADSK_Размер_Ширина";
        public const string OpeningDate = "ФОП_Дата";
        public const string OpeningDescription = "ФОП_Описание";
        public const string OpeningMepSystem = "ФОП_ВИС_Имя системы";
        public const string OpeningOffsetCenter = "ФОП_ВИС_Отметка оси от нуля";
        public const string OpeningOffsetBottom = "ФОП_ВИС_Отметка низа от нуля";

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
            var bimModelPartsService = GetPlatformService<IBimModelPartsService>();

            if(bimModelPartsService.InAnyBimModelParts(_document, BimModelPart.ARPart)) {
                return DocTypeEnum.AR;
            }

            if(bimModelPartsService.InAnyBimModelParts(_document, BimModelPart.KRPart, BimModelPart.KMPart)) {
                return DocTypeEnum.KR;
            }

            if(bimModelPartsService.InAnyBimModelParts(_document, BimModelPart.KOORDPart)) {
                return DocTypeEnum.KOORD;
            }

            return DocTypeEnum.MEP;
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
        /// Возвращает исходящие задания на отверстия в стенах от инженера из текущего файла Revit
        /// </summary>
        /// <returns></returns>
        public List<FamilyInstance> GetWallOpeningsMepTasksOutcoming() {
            var wallTypes = new[] { OpeningType.WallRectangle, OpeningType.WallRound };
            return GetOpeningsMepTasksOutcoming(wallTypes);
        }

        /// <summary>
        /// Возвращает исходящие задания на отверстия в перекрытиях от инженера из текущего файла Revit
        /// </summary>
        /// <returns></returns>
        public List<FamilyInstance> GetFloorOpeningsMepTasksOutcoming() {
            var floorTypes = new[] { OpeningType.FloorRectangle, OpeningType.FloorRound };
            return GetOpeningsMepTasksOutcoming(floorTypes);
        }

        public string GetFamilyName(Element element) {
            if(element is ElementType type) {
                return type.FamilyName;
            }
            var typeId = element.GetTypeId();
            if(typeId.IsNotNull()) {
                type = element.Document.GetElement(typeId) as ElementType;
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

        /// <summary>
        /// Возвращает коллекцию исходящих заданий на отверстия, размещенных в текущем файле Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningTaskOutcoming> GetPlacedOutcomingTasks() {
            return GetOpeningsTaskFromCurrentDoc().Select(f => new OpeningTaskOutcoming(f)).ToList();
        }

        public void DeleteElements(ICollection<Element> elements) {
            using(Transaction t = _document.StartTransaction("Удаление объединенных заданий на отверстия")) {
                _document.Delete(elements.Select(item => item.Id).ToArray());
                t.Commit();
            }
        }

        /// <summary>
        /// Удаляет элемент из документа Revit без запуска транзакции
        /// </summary>
        /// <param name="elementId">Id элемента, который нужно удалить</param>
        public void DeleteElement(int elementId) {
            _document.Delete(new ElementId(elementId));
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

        //public ICollection<>

        /// <summary>
        /// Возвращает список экземпляров семейств-заданий на отверстия от инженера из текущего файла ревит ("исходящие" задания).
        /// </summary>
        /// <returns>Список экземпляров семейств, названия семейств и типов которых заданы в соответствующих словарях
        /// <see cref="TypeName">названий типов</see> и
        /// <see cref="FamilyName">названий семейств</see></returns>
        private List<FamilyInstance> GetOpeningsTaskFromCurrentDoc() {
            return GetGenericModelsFamilyInstances()
                .Where(item => TypeName.Any(n => n.Value.Equals(item.Name))
                            && FamilyName.Any(n => n.Value.Equals(GetFamilyName(item))))
                .ToList();
        }

        private void RotateElement(Element element, XYZ point, Line axis, double angle) {
            if(Math.Abs(angle) > 0.00001) {
                ElementTransformUtils.RotateElement(_document, element.Id, axis, angle);
            }
        }


        /// <summary>
        /// Возвращает экземпляры семейств категории "Обобщенные модели" из текущего документа Revit
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FamilyInstance> GetGenericModelsFamilyInstances() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfType<FamilyInstance>();
        }

        /// <summary>
        /// Возвращает задания на отверстия от инженера из текущего файла Revit
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private List<FamilyInstance> GetOpeningsMepTasksOutcoming(ICollection<OpeningType> types) {
            return GetGenericModelsFamilyInstances()
               .Where(item => types.Any(e => TypeName[e].Equals(item.Name))
                           && types.Any(e => FamilyName[e].Equals(GetFamilyName(item))))
               .ToList();
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
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
    /// Типы файлов Revit в соответствии с шифрами разделов проектирования
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
        /// Координационный файл
        /// </summary>
        KOORD,
        /// <summary>
        /// Раздел не определен
        /// </summary>
        NotDefined
    }
}
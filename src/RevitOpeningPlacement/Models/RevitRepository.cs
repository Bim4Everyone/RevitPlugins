using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.Models.RevitViews;
using RevitOpeningPlacement.Models.Selection;
using RevitOpeningPlacement.OpeningModels;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitOpeningPlacement.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        private readonly RevitClashDetective.Models.RevitRepository _clashRevitRepository;
        private readonly RevitEventHandler _revitEventHandler;

        private readonly View3DProvider _view3DProvider;
        private readonly View3D _view;

        public RevitRepository(Application application, Document document) {

            _application = application ?? throw new ArgumentNullException(nameof(application));
            _uiApplication = new UIApplication(application);

            _document = document ?? throw new ArgumentNullException(nameof(document));
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

        public Document Doc => _document;

        public static Dictionary<MepCategoryEnum, string> MepCategoryNames { get; }
            = new Dictionary<MepCategoryEnum, string> {
            {MepCategoryEnum.Pipe, "Трубы" },
            {MepCategoryEnum.RectangleDuct, "Воздуховоды (прямоугольное сечение)" },
            {MepCategoryEnum.RoundDuct, "Воздуховоды (круглое сечение)" },
            {MepCategoryEnum.CableTray, "Лотки" },
            {MepCategoryEnum.Conduit, "Короба" }
        };

        public static Dictionary<FittingCategoryEnum, string> FittingCategoryNames { get; }
            = new Dictionary<FittingCategoryEnum, string> {
            {FittingCategoryEnum.CableTrayFitting, "Соединительные детали кабельных лотков" },
            {FittingCategoryEnum.DuctFitting, "Соединительные детали воздуховодов" },
            {FittingCategoryEnum.ConduitFitting, "Соединительные детали коробов" },
            {FittingCategoryEnum.PipeFitting, "Соединительные детали трубопроводов" },
        };

        public static Dictionary<StructureCategoryEnum, string> StructureCategoryNames { get; }
            = new Dictionary<StructureCategoryEnum, string> {
            {StructureCategoryEnum.Wall, "Стены" },
            {StructureCategoryEnum.Floor, "Перекрытия" },
        };

        public static Dictionary<Parameters, string> ParameterNames { get; } = new Dictionary<Parameters, string>() {
            {Parameters.Diameter, "Диаметр" },
            {Parameters.Height, "Высота" },
            {Parameters.Width, "Ширина" }
        };

        /// <summary>
        /// Названия вложенных общих семейств, по которым берется солид инженерного элемента, если его надо переопределить.
        /// </summary>
        public static IReadOnlyCollection<string> CustomGeometryFamilies { get; }
            = new ReadOnlyCollection<string>(new string[] {
                "ОбщМд_Отв_Принудительный габарит_Прямоугольный",
                "ОбщМд_Отв_Принудительный габарит_Круглый"
            });

        /// <summary>
        /// Словарь типов проемов и названий семейств заданий на отверстия
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningTaskFamilyName { get; }
            = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В перекрытии" },
            {OpeningType.FloorRound, "ОбщМд_Отв_Отверстие_Круглое_В перекрытии" },
            {OpeningType.WallRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В стене" },
            {OpeningType.WallRound, "ОбщМд_Отв_Отверстие_Круглое_В стене" },
        };

        /// <summary>
        /// Словарь типов проемов и названий типов семейств заданий на отверстия
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningTaskTypeName => new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Прямоугольное" },
            {OpeningType.FloorRound, "Круглое" },
            {OpeningType.WallRectangle, "Прямоугольное" },
            {OpeningType.WallRound, "Круглое" },
        };

        /// <summary>
        /// Словарь типов проемов и названий семейств чистовых отверстий АР
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningRealArFamilyName { get; }
            = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Окн_Отв_Прямоуг_Перекрытие" },
            {OpeningType.FloorRound, "Окн_Отв_Круг_Перекрытие" },
            {OpeningType.WallRectangle, "Окн_Отв_Прямоуг_Стена" },
            {OpeningType.WallRound, "Окн_Отв_Круг_Стена" },
        };

        /// <summary>
        /// Словарь типов проемов и названий типов семейств чистовых отверстий АР
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningRealArTypeName => new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Окн_Отв_Прямоуг_Перекрытие" },
            {OpeningType.FloorRound, "Окн_Отв_Круг_Перекрытие" },
            {OpeningType.WallRectangle, "Окн_Отв_Прямоуг_Стена" },
            {OpeningType.WallRound, "Окн_Отв_Круг_Стена" },
        };

        /// <summary>
        /// Словарь типов проемов и названий семейств чистовых отверстий КР
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningRealKrFamilyName { get; }
            = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "ОбщМд_Отверстие_Перекрытие_Прямоугольное" },
            {OpeningType.WallRectangle, "ОбщМд_Отверстие_Стена_Прямоугольное"},
            {OpeningType.WallRound, "ОбщМд_Отверстие_Стена_Круглое"}
        };

        public static Dictionary<OpeningType, string> OpeningRealKrTypeName { get; }
            = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Отверстие прямоугольное"},
            {OpeningType.WallRectangle, "Отверстие прямоугольное" },
            {OpeningType.WallRound, "Отверстие круглое" }
        };

        /// <summary>
        /// Используемые в плагине линейные категории для труб: Трубы
        /// </summary>
        public static BuiltInCategory MepPipeLinearCategory { get; } = BuiltInCategory.OST_PipeCurves;

        /// <summary>
        /// Используемые в плагине нелинейные категории для труб: Соединительные детали трубопроводов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepPipeFittingCategories { get; } =
            new ReadOnlyCollection<BuiltInCategory>(new BuiltInCategory[] {
                BuiltInCategory.OST_PipeFitting
            });

        /// <summary>
        /// Все используемые в плагине категории для труб: Трубы, Соединительные детали трубопроводов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepPipeCategories { get; } =
           new ReadOnlyCollection<BuiltInCategory>(
               new List<BuiltInCategory>(MepPipeFittingCategories)
               .Append(MepPipeLinearCategory)
               .ToArray());

        /// <summary>
        /// Используемые в плагине линейные категории для воздуховодов: Воздуховоды
        /// </summary>
        public static BuiltInCategory MepDuctLinearCategory { get; } = BuiltInCategory.OST_DuctCurves;

        /// <summary>
        /// Используемые в плагине нелинейные категории для воздуховодов: Соединители детали воздуховодов, Арматура воздуховодов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepDuctFittingCategories { get; } =
            new ReadOnlyCollection<BuiltInCategory>(new BuiltInCategory[] {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctAccessory
            });

        /// <summary>
        /// Все используемые в плагине категории для воздуховодов: Воздуховоды, Соединители детали воздуховодов, Арматура воздуховодов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepDuctCategories { get; } =
           new ReadOnlyCollection<BuiltInCategory>(
               new List<BuiltInCategory>(MepDuctFittingCategories)
               .Append(MepDuctLinearCategory)
               .ToArray());

        /// <summary>
        /// Используемые в плагине линейные категории для кабельных лотков: Кабельные лотки
        /// </summary>
        public static BuiltInCategory MepCableTrayLinearCategory { get; } = BuiltInCategory.OST_CableTray;

        /// <summary>
        /// Используемые в плагине нелинейные категории для кабельных лотков: Соединители детали кабельных лотков
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepCableTrayFittingCategories { get; } =
            new ReadOnlyCollection<BuiltInCategory>(new BuiltInCategory[] {
                BuiltInCategory.OST_CableTrayFitting
            });

        /// <summary>
        /// Все используемые в плагине категории для кабельных лотков: Кабельные лотки, Соединители детали кабельных лотков
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepCableTrayCategories { get; } =
           new ReadOnlyCollection<BuiltInCategory>(
               new List<BuiltInCategory>(MepCableTrayFittingCategories)
               .Append(MepCableTrayLinearCategory)
               .ToArray());

        /// <summary>
        /// Используемые в плагине линейные категории для коробов: Короба
        /// </summary>
        public static BuiltInCategory MepConduitLinearCategory { get; } = BuiltInCategory.OST_Conduit;

        /// <summary>
        /// Используемые в плагине нелинейные категории для коробов: Соединители детали коробов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepConduitFittingCategories { get; } =
            new ReadOnlyCollection<BuiltInCategory>(new BuiltInCategory[] {
                BuiltInCategory.OST_ConduitFitting
            });

        /// <summary>
        /// Все используемые в плагине категории для коробов: Короба, Соединители детали коробов
        /// </summary>
        public static IReadOnlyCollection<BuiltInCategory> MepConduitCategories { get; } =
           new ReadOnlyCollection<BuiltInCategory>(
               new List<BuiltInCategory>(MepConduitFittingCategories)
               .Append(MepConduitLinearCategory)
               .ToArray());


        public const string OpeningDiameter = "ADSK_Размер_Диаметр";
        public const string OpeningThickness = "ADSK_Размер_Глубина";
        public const string OpeningHeight = "ADSK_Размер_Высота";
        public const string OpeningWidth = "ADSK_Размер_Ширина";
        public const string OpeningDate = "ФОП_Дата";
        public const string OpeningDescription = "ФОП_Описание";
        public const string OpeningMepSystem = "ФОП_ВИС_Имя системы";
        public const string OpeningOffsetCenter = "ФОП_ВИС_Отметка оси от нуля";
        public const string OpeningOffsetBottom = "ФОП_ВИС_Отметка низа от нуля";
        public const string OpeningAuthor = "ФОП_Автор задания";
        public const string OpeningIsManuallyPlaced = "ФОП_Размещено вручную";
        public const string OpeningOffsetBottomAdsk = "ADSK_Отверстие_Отметка от нуля";
        public const string OpeningOffsetFromLevelAdsk = "ADSK_Отверстие_Отметка от этажа";
        public const string OpeningLevelOffsetAdsk = "ADSK_Отверстие_Отметка этажа";

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
        /// Возвращает типоразмер семейства задания на отверстие из репозитория
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FamilySymbol GetOpeningTaskType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .FirstOrDefault(
                    item => item.Name.Equals(OpeningTaskTypeName[type])
                    && item.FamilyName.Equals(OpeningTaskFamilyName[type]));
        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия АР из репозитория
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FamilySymbol GetOpeningRealArType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .FirstOrDefault(
                    item => item.Name.Equals(OpeningRealArTypeName[type])
                    && item.FamilyName.Equals(OpeningRealArFamilyName[type]));
        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия КР из репозитория
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FamilySymbol GetOpeningRealKrType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(
                    item => item.Name
                        .Equals(OpeningRealKrTypeName[type]) && item.FamilyName.Equals(OpeningRealKrFamilyName[type]));
        }

        /// <summary>
        /// Возвращает семейство задания на отверстие из репозитория
        /// </summary>
        /// <param name="openingType"></param>
        /// <returns></returns>
        public Family GetOpeningTaskFamily(OpeningType openingType) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .OfType<Family>()
                .FirstOrDefault(
                    item => item?.Name
                        ?.Equals(OpeningTaskFamilyName[openingType], StringComparison.CurrentCultureIgnoreCase)
                            == true);
        }

        /// <summary>
        /// Возвращает семейство чистового отверстия АР из репозитория
        /// </summary>
        /// <param name="openingType"></param>
        /// <returns></returns>
        public Family GetOpeningRealArFamily(OpeningType openingType) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .OfType<Family>()
                .FirstOrDefault(
                    item => item?.Name
                        ?.Equals(OpeningRealArFamilyName[openingType], StringComparison.CurrentCultureIgnoreCase)
                            == true);
        }

        /// <summary>
        /// Возвращает семейство чистового отверстия КР из репозитория
        /// </summary>
        /// <param name="openingType"></param>
        /// <returns></returns>
        public Family GetOpeningRealKrFamily(OpeningType openingType) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .OfType<Family>()
                .FirstOrDefault(
                    item => item?.Name
                        ?.Equals(OpeningRealKrFamilyName[openingType], StringComparison.CurrentCultureIgnoreCase)
                            == true);
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

        public static string GetLevelName(Element element) {
            return RevitClashDetective.Models.RevitRepository.GetLevelName(element);
        }

        public static Level GetLevel(Element element) {
            return RevitClashDetective.Models.RevitRepository.GetLevel(element);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentNullException"/>
        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(string fileName, ElementId id) {
            return _clashRevitRepository.GetElement(fileName, id);
        }

        public void SelectAndShowElement(ICollection<ElementModel> elements) {
            double additionalSize = 2;
            _clashRevitRepository.SelectAndShowElement(elements, additionalSize, _view);
        }

        public void SelectAndShowElement(ISelectorAndHighlighter selectorAndHighlighter) {
            var elementToHighlight = selectorAndHighlighter.GetElementToHighlight();
            if(elementToHighlight != null) {
                try {
                    new ElementHighlighter(this, _view, elementToHighlight).HighlightElement();
                } catch(ArgumentException) {
                    // элемент для выделения не стена и не перекрытие
                }
            }
            double additionalSize = 2;
            var elementsToSelect = selectorAndHighlighter.GetElementsToSelect();
            _clashRevitRepository.SelectAndShowElement(elementsToSelect, additionalSize, _view);
        }

        public static string GetDocumentName(Document doc) {
            return RevitClashDetective.Models.RevitRepository.GetDocumentName(doc);
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
            var bimModelPartsService = GetBimModelPartsService();

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

        /// <summary>
        /// Возвращает сервис для работы с разделами проектной документации
        /// </summary>
        /// <returns></returns>
        public static IBimModelPartsService GetBimModelPartsService() {
            return GetPlatformService<IBimModelPartsService>();
        }

        /// <summary>
        /// Размещает экземпляр заданного типоразмера семейства в хосте по точке вставки с уровнем по хосту
        /// </summary>
        /// <param name="point">Точка вставки</param>
        /// <param name="familySymbol">Типоразмер семейства</param>
        /// <param name="host">Хост экземпляра семейства</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentException"></exception>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentNullException"></exception>
        public FamilyInstance CreateInstance(XYZ point, FamilySymbol familySymbol, Element host) {
            if(point is null) { throw new ArgumentNullException(nameof(point)); }
            if(familySymbol is null) { throw new ArgumentNullException(nameof(familySymbol)); }
            if(host is null) { throw new ArgumentNullException(nameof(host)); }

            if(!familySymbol.IsActive) { familySymbol.Activate(); }

            var level = GetElement(host.LevelId) as Level;
            return _document.Create.NewFamilyInstance(point, familySymbol, host, level, StructuralType.NonStructural);
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
                RotateElement(element, Line.CreateBound(point, new XYZ(point.X + 1, point.Y, point.Z)), angle.X);
                RotateElement(element, Line.CreateBound(point, new XYZ(point.X, point.Y + 1, point.Z)), angle.Y);
                RotateElement(element, Line.CreateBound(point, new XYZ(point.X, point.Y, point.Z + 1)), angle.Z);
            }
        }

        /// <summary>
        /// Возвращает коллекцию всех экземпляров семейств исходящих заданий на отверстия 
        /// из текущего файла инженерных систем
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningMepTaskOutcoming> GetOpeningsMepTasksOutcoming() {
            var openingsInWalls = GetWallOpeningsMepTasksOutcoming();
            var openingsInFloor = GetFloorOpeningsMepTasksOutcoming();
            openingsInFloor.AddRange(openingsInWalls);
            return openingsInFloor.Select(famInst => new OpeningMepTaskOutcoming(famInst)).ToHashSet();
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
            return DocInfos
                .FirstOrDefault(
                    item => item.Name
                        .Equals(GetDocumentName(element.Document), StringComparison.CurrentCultureIgnoreCase))
                ?.Transform
                ?? Transform.Identity;
        }

        public IEnumerable<Element> GetFilteredElements(
            Document doc,
            IEnumerable<ElementId> categories,
            ElementFilter filter) {

            return _clashRevitRepository.GetFilteredElements(doc, categories, filter);
        }

        /// <summary>
        /// Возвращает коллекцию исходящих заданий на отверстия, размещенных в текущем файле Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningMepTaskOutcoming> GetPlacedOutcomingTasks() {
            return GetOpeningsTaskFromCurrentDoc().Select(f => new OpeningMepTaskOutcoming(f)).ToHashSet();
        }

        public void DeleteElements(ICollection<ElementId> elements) {
            using(Transaction t = _document.StartTransaction("Удаление объединенных заданий на отверстия")) {
                _document.Delete(elements);
                t.Commit();
            }
        }

        /// <summary>
        /// Удаляет элемент из документа Revit без запуска транзакции
        /// </summary>
        /// <param name="elementId">Id элемента, который нужно удалить</param>
        public void DeleteElement(ElementId elementId) {
            _document.Delete(elementId);
        }

        /// <summary>
        /// Объединяет задания на отверстия из активного документа и удаляет старые
        /// </summary>
        /// <param name="openingTasks">Коллекция объединяемых заданий на отверстия</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public FamilyInstance UniteOpenings(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            var placer = GetOpeningPlacer(openingTasks);
            FamilyInstance createdOpening = null;
            try {
                using(var t = GetTransaction("Объединение отверстий")) {
                    createdOpening = placer.Place();
                    t.Commit();
                }

            } catch(OpeningNotPlacedException e) {
                var dialog = GetMessageBoxService();
                dialog.Show(
                    e.Message,
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            }
            DeleteElements(openingTasks.Select(task => task.Id).ToHashSet());
            return createdOpening;
        }

        public void DoAction(Action action) {
            _revitEventHandler.TransactAction = action;
            _revitEventHandler.Raise();
        }

        public List<ParameterValueProvider> GetParameters(Document doc, IEnumerable<Category> categories) {
            return _clashRevitRepository.GetParameters(doc, categories);
        }

        /// <summary>
        /// Возвращает значение элемента перечисления категорий инженерных систем
        /// </summary>
        /// <param name="mepCategoryName">Название категории инженерных систем</param>
        /// <returns></returns>
        public MepCategoryEnum GetMepCategoryEnum(string mepCategoryName) {
            return MepCategoryNames
                .First(pair => pair.Value.Equals(mepCategoryName, StringComparison.CurrentCultureIgnoreCase))
                .Key;
        }

        /// <summary>
        /// Возвращает массив категорий Revit, которые соответствуют заданному <see cref="MepCategoryEnum"/>
        /// </summary>
        /// <param name="mepCategory">Категория элементов инженерных систем</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Исключение, 
        /// если поданная категория <paramref name="mepCategory"/> не поддерживается</exception>
        public Category[] GetCategories(MepCategoryEnum mepCategory) {
            IReadOnlyCollection<BuiltInCategory> categoryCollection;
            switch(mepCategory) {
                case MepCategoryEnum.Pipe:
                    categoryCollection = MepPipeCategories;
                    break;
                case MepCategoryEnum.RectangleDuct:
                case MepCategoryEnum.RoundDuct:
                    categoryCollection = MepDuctCategories;
                    break;
                case MepCategoryEnum.CableTray:
                    categoryCollection = MepCableTrayCategories;
                    break;
                case MepCategoryEnum.Conduit:
                    categoryCollection = MepConduitCategories;
                    break;
                default:
                    throw new NotImplementedException(nameof(mepCategory));
            }
            return categoryCollection.Select(c => Category.GetCategory(_document, c)).ToArray();
        }

        public bool ElementBelongsToMepCategory(MepCategoryEnum mepCategory, Element element) {
            BuiltInCategory elCategory = element.Category.GetBuiltInCategory();
            switch(mepCategory) {
                case MepCategoryEnum.Pipe:
                    return MepPipeCategories.Contains(elCategory);
                case MepCategoryEnum.RectangleDuct: {
                    //либо это элемент из категории для воздуховодов и не Воздуховод, либо это Воздуховод только прямоугольного сечения
                    return MepDuctCategories.Contains(elCategory)
                        && (!(element is Duct)
                        || ((element is Duct duct)
                        && (duct.DuctType.Shape == ConnectorProfileType.Rectangular)));
                }
                case MepCategoryEnum.RoundDuct: {
                    //либо это элемент из категории для воздуховодов и не Воздуховод, либо это Воздуховод только круглого сечения
                    return MepDuctCategories.Contains(elCategory)
                        && (!(element is Duct)
                        || ((element is Duct duct)
                        && (duct.DuctType.Shape == ConnectorProfileType.Round)));
                }
                case MepCategoryEnum.CableTray:
                    return MepCableTrayCategories.Contains(elCategory);
                case MepCategoryEnum.Conduit:
                    return MepConduitCategories.Contains(elCategory);
                default:
                    throw new NotImplementedException(nameof(mepCategory));
            };
        }

        /// <summary>
        /// Спрашивает у пользователя, нужно ли продолжать операцию, если загружены не все связи
        /// </summary>
        /// <returns></returns>
        public bool ContinueIfNotAllLinksLoaded() {
            var notLoadedLinksNames = GetRevitLinkNotLoadedNames();
            if(notLoadedLinksNames.Count > 0) {
                var dialog = GetMessageBoxService();
                return dialog.Show(
                    $"Связи:\n{string.Join(";\n", notLoadedLinksNames)} \nне загружены, хотите продолжить?",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning,
                    System.Windows.MessageBoxResult.No) == System.Windows.MessageBoxResult.Yes;

            } else {
                return true;
            }
        }

        /// <summary>
        /// Спрашивает у пользователя, нужно ли продолжать операцию, 
        /// если семейства заданий на отверстия не самой последней версии
        /// </summary>
        /// <returns></returns>
        public bool ContinueIfTaskFamiliesNotLatest() {
            var checker = new FamiliesParametersChecker(this);
            bool familiesLatest = checker.IsCorrect();

            if(!familiesLatest) {
                var dialog = GetMessageBoxService();
                return dialog.Show(
                    $"{checker.GetErrorMessage()}Хотите продолжить?",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning,
                    System.Windows.MessageBoxResult.No) == System.Windows.MessageBoxResult.Yes;

            } else {
                return true;
            }
        }

        /// <summary>
        /// Возвращает сервис диалоговых окон
        /// </summary>
        /// <returns></returns>
        public IMessageBoxService GetMessageBoxService() {
            return GetPlatformService<IMessageBoxService>();
        }

        /// <summary>
        /// Возвращает прогресс бар
        /// </summary>
        /// <returns></returns>
        public IProgressDialogService GetProgressDialogService() {
            return GetPlatformService<IProgressDialogService>();
        }

        /// <summary>
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий из текущего АР документа Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningRealAr> GetRealOpeningsAr() {
            return GetRealOpeningsAr(_document);
        }

        /// <summary>
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий из заданного АР документа Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningRealAr> GetRealOpeningsAr(Document document) {
            return GetOpeningsAr(document)
                .Select(famInst => new OpeningRealAr(famInst))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий из текущего КР документа Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningRealKr> GetRealOpeningsKr() {
            return GetRealOpeningsKr(_document);
        }

        /// <summary>
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий из заданного КР документа Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningRealKr> GetRealOpeningsKr(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedOpeningsKrCategories())
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(famInst => famInst.Host != null)
                .Where(famInst => famInst.Symbol.FamilyName.Contains("ОбщМд_Отверстие_"))
                .Select(famInst => new OpeningRealKr(famInst))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает коллекцию Id всех элементов конструкций из текущего документа ревита, 
        /// для которых создаются задания на отверстия
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementId> GetConstructureElementsIds() {
            return GetConstructureElementsIds(_document);
        }

        /// <summary>
        /// Возвращает коллекцию Id всех элементов конструкций из заданного документа ревита, 
        /// для которых создаются задания на отверстия
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementId> GetConstructureElementsIds(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedStructureCategories())
                .ToElementIds();
        }

        /// <summary>
        /// Возвращает коллекцию Id всех элементов инженерных систем из текущего документа ревита, 
        /// для которых создаются задания на отверстия
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementId> GetMepElementsIds() {
            return new FilteredElementCollector(_document)
                .WherePasses(FiltersInitializer.GetFilterByAllUsedMepCategories())
                .ToElementIds();
        }

        /// <summary>
        /// Возвращает коллекцию всех входящих заданий на отверстия из связанных файлов ВИС
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningMepTaskIncoming> GetOpeningsMepTasksIncoming() {
            var links = GetRevitLinks();
            HashSet<OpeningMepTaskIncoming> genericModelsInLinks = new HashSet<OpeningMepTaskIncoming>();
            foreach(RevitLinkInstance link in links) {
                var linkDoc = link.GetLinkDocument();
                var transform = link.GetTransform();
                var genericModelsInLink = new FilteredElementCollector(linkDoc)
                    .WhereElementIsNotElementType()
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .OfType<FamilyInstance>()
                    .Where(item => OpeningTaskTypeName.Any(n => n.Value.Equals(item.Name))
                                && OpeningTaskFamilyName.Any(n => n.Value.Equals(GetFamilyName(item))))
                    .Select(famInst => new OpeningMepTaskIncoming(famInst, this, transform))
                    .ToHashSet();
                genericModelsInLinks.UnionWith(genericModelsInLink);
            }
            return genericModelsInLinks;
        }

        /// <summary>
        /// Возвращает коллекцию всех входящих заданий на отверстия из связанных файлов АР
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningArTaskIncoming> GetOpeningsArTasksIncoming() {
            var service = GetBimModelPartsService();
            var links = GetRevitLinks().Where(link => service.InAnyBimModelParts(link, BimModelPart.ARPart));
            HashSet<OpeningArTaskIncoming> openingsArInLinks = new HashSet<OpeningArTaskIncoming>();
            foreach(RevitLinkInstance link in links) {
                var linkDoc = link.GetLinkDocument();
                var transform = link.GetTransform();
                var openingsArInLink = GetOpeningsAr(linkDoc)
                    .Select(famInst => new OpeningArTaskIncoming(this, famInst, transform));
                openingsArInLinks.UnionWith(openingsArInLink);
            }
            return openingsArInLinks;
        }

        /// <summary>
        /// Возвращает коллекцию всех связей АР и КР из документа репозитория
        /// </summary>
        /// <returns></returns>
        public ICollection<RevitLinkInstance> GetConstructureLinks() {
            var bimModelPartsService = GetBimModelPartsService();
            return GetRevitLinks()
                .Where(link => bimModelPartsService.InAnyBimModelParts(
                    link.Name,
                    BimModelPart.ARPart,
                    BimModelPart.KRPart,
                    BimModelPart.KMPart
                    ))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает коллекцию всех связей инженерных систем из документа репозитория
        /// </summary>
        /// <returns></returns>
        public ICollection<RevitLinkInstance> GetMepLinks() {
            var bimModelPartsService = GetBimModelPartsService();
            return GetRevitLinks()
                .Where(link => bimModelPartsService.InAnyBimModelParts(
                    link.Name,
                    BimModelPart.OVPart,
                    BimModelPart.ITPPart,
                    BimModelPart.HCPart,
                    BimModelPart.VKPart,
                    BimModelPart.EOMPart,
                    BimModelPart.EGPart,
                    BimModelPart.SSPart,
                    BimModelPart.VNPart,
                    BimModelPart.KVPart,
                    BimModelPart.OTPart,
                    BimModelPart.DUPart,
                    BimModelPart.VSPart,
                    BimModelPart.KNPart,
                    BimModelPart.PTPart,
                    BimModelPart.EOPart,
                    BimModelPart.EMPart
                    ))
            .ToHashSet();
        }

        /// <summary>
        /// Возвращает коллекцию всех связей архитектурных файлов из документа репозитория
        /// </summary>
        /// <returns></returns>
        public ICollection<RevitLinkInstance> GetArLinks() {
            var service = GetBimModelPartsService();
            return GetRevitLinks()
                .Where(link => service.InAnyBimModelParts(
                    link,
                    BimModelPart.ARPart
                    ))
                .ToHashSet();
        }

        /// <summary>
        /// Предлагает пользователю выбрать системную стену или системное перекрытие и возвращает его выбор
        /// </summary>
        /// <returns>Выбранный пользователем элемент классов <see cref="Autodesk.Revit.DB.Wall"/> 
        /// или <see cref="Autodesk.Revit.DB.Floor"/></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public Element PickHostForRealOpening() {
            // фильтр по классам, а не по категориям ревита,
            // так как для хоста нужна системная стена или системное перекрытие,
            // при этом необходимо исключить выбор моделей в контексте, которые могут быть стенами и перекрытиями
            ISelectionFilter filter = new SelectionFilterElementsOfClasses(new Type[] { typeof(Wall), typeof(Floor) });
            Reference reference = _uiDocument.Selection.PickObject(
                ObjectType.Element,
                filter,
                "Выберите стену или перекрытие");
            return _document.GetElement(reference);
        }

        /// <summary>
        /// Предлагает пользователю выбрать стены и перекрытия и возвращает его выбор
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public ICollection<Element> PickHostsForRealOpenings() {
            // фильтр по классам, а не по категориям ревита,
            // так как для хоста нужна системная стена или системное перекрытие,
            // при этом необходимо исключить выбор моделей в контексте, которые могут быть стенами и перекрытиями
            ISelectionFilter filter = new SelectionFilterElementsOfClasses(new Type[] { typeof(Wall), typeof(Floor) });
            IList<Reference> references = _uiDocument.Selection.PickObjects(
                ObjectType.Element,
                filter,
                "Выберите стену(ы) и(или) перекрытие(я)");

            HashSet<Element> hosts = new HashSet<Element>();
            foreach(Reference reference in references) {
                if(reference != null) {
                    Element element = _document.GetElement(reference);
                    hosts.Add(element);
                }
            }
            return hosts;
        }

        /// <summary>
        /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из связанных файлов, 
        /// подгруженных в активный документ, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранная пользователем коллекция элементов</returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public ICollection<OpeningMepTaskIncoming> PickManyOpeningMepTasksIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningMepTasksIncoming(_document);
            IList<Reference> references = _uiDocument.Selection.PickObjects(
                ObjectType.LinkedElement,
                filter,
                "Выберите задание(я) на отверстие(я) из связи(ей) ВИС и нажмите \"Готово\"");

            HashSet<OpeningMepTaskIncoming> openingTasks = new HashSet<OpeningMepTaskIncoming>();
            foreach(var reference in references) {
                if((reference != null) && (_document.GetElement(reference) is RevitLinkInstance link)) {
                    Element opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                    if((opening != null) && (opening is FamilyInstance famInst)) {
                        openingTasks.Add(new OpeningMepTaskIncoming(famInst, this, link.GetTransform()));
                    }
                }
            }
            return openingTasks;
        }

        /// <summary>
        /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из активного документа 
        /// и возвращает его выбор
        /// </summary>
        /// <returns>Выбранная пользователем коллекция элементов - заданий на отверстия</returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public ICollection<OpeningMepTaskOutcoming> PickManyOpeningMepTasksOutcoming() {
            ISelectionFilter filter = new SelectionFilterOpeningMepTasksOutcoming();
            IList<Reference> references = _uiDocument.Selection.PickObjects(
                ObjectType.Element,
                filter,
                "Выберите исходящие задания на отверстия и нажмите \"Готово\"");

            HashSet<OpeningMepTaskOutcoming> openingTasks = new HashSet<OpeningMepTaskOutcoming>();
            foreach(var reference in references) {
                if((reference != null) && (_document.GetElement(reference) is FamilyInstance famInst)) {
                    openingTasks.Add(new OpeningMepTaskOutcoming(famInst));
                }
            }
            return openingTasks;
        }

        /// <summary>
        /// Предлагает пользователю выбрать один экземпляр семейства задания на отверстие из связанных файлов, 
        /// подгруженных в активный документ, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранный пользователем элемент</returns>
        /// <exception cref="OperationCanceledException"/>
        public OpeningMepTaskIncoming PickSingleOpeningMepTaskIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningMepTasksIncoming(_document);
            Reference reference = _uiDocument.Selection.PickObject(
                ObjectType.LinkedElement,
                filter,
                "Выберите задание на отверстие из связи ВИС и нажмите \"Готово\"");

            if((reference != null) && (_document.GetElement(reference) is RevitLinkInstance link)) {
                Element opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                if((opening != null) && (opening is FamilyInstance famInst)) {
                    return new OpeningMepTaskIncoming(famInst, this, link.GetTransform());
                } else {
                    ShowErrorMessage($"Выбранный элемент не является экземпляром семейства задания на отверстие");
                    throw new OperationCanceledException();
                }
            } else {
                ShowErrorMessage($"Не удалось определить выбранный элемент");
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Предлагает пользователю выбрать один экземпляр семейства задания на отверстие из связанных файлов АР, 
        /// подгруженных в активный документ, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранный пользователем элемент</returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"></exception>
        public OpeningArTaskIncoming PickSingleOpeningArTaskIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningArTasksIncoming(_document);
            Reference reference = _uiDocument.Selection.PickObject(
                ObjectType.LinkedElement,
                filter,
                "Выберите задание на отверстие из связи АР и нажмите \"Готово\"");

            if((reference != null) && (_document.GetElement(reference) is RevitLinkInstance link)) {
                Element opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                if((opening != null) && (opening is FamilyInstance famInst)) {
                    return new OpeningArTaskIncoming(this, famInst, link.GetTransform());
                } else {
                    ShowErrorMessage($"Выбранный элемент не является экземпляром семейства задания на отверстие");
                    throw new OperationCanceledException();
                }
            } else {
                ShowErrorMessage($"Не удалось определить выбранный элемент");
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из связанных файлов АР, 
        /// подгруженных в активный документ КР, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранная пользователем коллекция элементов</returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"></exception>
        public ICollection<OpeningArTaskIncoming> PickManyOpeningArTasksIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningArTasksIncoming(_document);
            IList<Reference> references = _uiDocument.Selection
                .PickObjects(
                ObjectType.LinkedElement,
                filter,
                "Выберите задание(я) на отверстие(я) из связи(ей) АР и нажмите \"Готово\"");

            HashSet<OpeningArTaskIncoming> openingTasks = new HashSet<OpeningArTaskIncoming>();
            foreach(var reference in references) {
                if((reference != null) && (_document.GetElement(reference) is RevitLinkInstance link)) {
                    Element opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                    if((opening != null) && (opening is FamilyInstance famInst)) {
                        openingTasks.Add(new OpeningArTaskIncoming(this, famInst, link.GetTransform()));
                    }
                }
            }
            return openingTasks;
        }

        /// <summary>
        /// Предлагает пользователю выбрать экземпляры элементов ВИС, категории которых выбраны в настройках плагина,
        /// и возвращает его выбор
        /// </summary>
        /// <param name="mepCategories">Категории из настроек плагина</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ElementId[] PickMepElements(MepCategoryCollection mepCategories) {
            if(mepCategories is null) { throw new ArgumentNullException(nameof(mepCategories)); }

            var categories = mepCategories
                .Where(c => c.IsSelected)
                .Select(c => GetMepCategoryEnum(c.Name))
                .ToArray();
            ISelectionFilter filter = new SelectionFilterMepElements(this, categories);

            IList<Reference> references = _uiDocument.Selection
                .PickObjects(ObjectType.Element, filter, "Выберите элементы ВИС и нажмите \"Готово\"");

            HashSet<ElementId> mepElements = new HashSet<ElementId>();
            foreach(var reference in references) {
                if(reference != null) {
                    ElementId elId = reference.ElementId;
                    mepElements.Add(elId);
                }
            }
            return mepElements.ToArray();
        }

        /// <summary>
        /// Выбирает элементы из активного документа по Id
        /// </summary>
        /// <param name="elementIds">Коллекция Id элементов из активного документа, которые надо выбрать</param>
        public void SetSelection(ICollection<ElementId> elementIds) {
            if(elementIds != null) {
                _uiDocument.Selection.SetElementIds(elementIds);
            }
        }

        /// <summary>
        /// Выбирает элемент из активного документа по Id
        /// </summary>
        /// <param name="elementIds">Id элемента из активного документа, который надо выбрать</param>
        public void SetSelection(ElementId elementId) {
            if(elementId != null) {
                _uiDocument.Selection.SetElementIds(new ElementId[] { elementId });
            }
        }

        /// <summary>
        /// Возвращает тип проема по названию семейства
        /// </summary>
        /// <param name="familyName">Название семейства</param>
        /// <returns></returns>
        public static OpeningType GetOpeningType(string familyName) {
            IDictionary<OpeningType, string> openingTypeAndFamNameDict;

            if(OpeningTaskFamilyName.Values.Contains(familyName)) {
                openingTypeAndFamNameDict = OpeningTaskFamilyName;
            } else if(OpeningRealArFamilyName.Values.Contains(familyName)) {
                openingTypeAndFamNameDict = OpeningRealArFamilyName;
            } else {
                return OpeningType.WallRectangle;
            }
            return openingTypeAndFamNameDict
                .FirstOrDefault(pair => pair.Value.Equals(familyName, StringComparison.CurrentCultureIgnoreCase))
                .Key;
        }

        /// <summary>
        /// Возвращает тип архитектурного проема по названию семейств
        /// </summary>
        /// <param name="familyName">Название семейства архитектурного проема</param>
        /// <returns></returns>
        public static OpeningType GetOpeningRealArType(string familyName) {
            return OpeningRealArFamilyName
                .FirstOrDefault(pair => pair.Value.Equals(familyName, StringComparison.CurrentCultureIgnoreCase))
                .Key;
        }

        /// <summary>
        /// Возвращает коллекцию заголовков файлов Revit связей, 
        /// которые дублируются и среди которых есть родительские связи.
        /// То есть дублирующиеся вложенные связи не будут попадать в список.
        /// </summary>
        /// <returns>Коллекция заголовков дублированных Revit-связей</returns>
        public ICollection<string> GetDuplicatedLinksNames() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(link => RevitLinkType.IsLoaded(_document, link.GetTypeId()))
                .GroupBy(inst => inst.GetLinkDocument().Title)
                .Where(group => group.Count() > 1)
                .Where(group => group.ToArray().Any(linkInstance => _clashRevitRepository.IsParentLink(linkInstance)))
                .Select(group => group.Key)
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает ссылку на документ семейства
        /// </summary>
        /// <param name="family"></param>
        /// <returns></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentNullException"/>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentException"/>
        /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException"/>
        /// <exception cref="Autodesk.Revit.Exceptions.ForbiddenForDynamicUpdateException"/>
        public Document EditFamily(Family family) {
            return _document.EditFamily(family);
        }

        /// <summary>
        /// Выводит сообщение об ошибке
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public void ShowErrorMessage(string message) {
            var dialog = GetMessageBoxService();
            dialog.Show(
                message,
                "Задания на отверстия",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }

        /// <summary>
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий АР из документа Revit
        /// </summary>
        /// <returns></returns>
        private ICollection<FamilyInstance> GetOpeningsAr(Document document) {
            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedOpeningsArCategories())
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(famInst => famInst.Host != null)
                .Where(famInst => famInst.Symbol.FamilyName.Contains("Отв"))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает класс, размещающий объединенное задание на отверстие
        /// </summary>
        /// <param name="openingTasks">Задания на отверстия из активного документа, которые надо объединить</param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"/>
        private OpeningPlacer GetOpeningPlacer(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            try {
                OpeningsGroup group = new OpeningsGroup(openingTasks);
                return group.GetOpeningPlacer(this);

            } catch(ArgumentNullException nullEx) {
                var dialog = GetMessageBoxService();
                dialog.Show(
                    nullEx.Message,
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();

            } catch(ArgumentOutOfRangeException) {
                var dialog = GetMessageBoxService();
                dialog.Show(
                    "Необходимо выбрать как минимум 2 задания на отверстия",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            } catch(InvalidOperationException) {
                var dialog = GetMessageBoxService();
                dialog.Show(
                    "Не удалось выполнить объединение",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров семейств-заданий на отверстия от инженера из текущего файла ревит 
        /// ("исходящие" задания).
        /// </summary>
        /// <returns>Коллекция экземпляров семейств, названия семейств и типов которых заданы в соответствующих словарях
        private ICollection<FamilyInstance> GetOpeningsTaskFromCurrentDoc() {
            return GetGenericModelsFamilyInstances()
                .Where(item => OpeningTaskTypeName.Any(n => n.Value.Equals(item.Name))
                            && OpeningTaskFamilyName.Any(n => n.Value.Equals(GetFamilyName(item))))
                .ToHashSet();
        }

        private void RotateElement(Element element, Line axis, double angle) {
            if(Math.Abs(angle) > 0.00001) {
                ElementTransformUtils.RotateElement(_document, element.Id, axis, angle);
            }
        }

        private ICollection<string> GetRevitLinkNotLoadedNames() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .Where(link => !RevitLinkType.IsLoaded(
                                   _document,
                                   link.Id))
                .Select(link => link.Name)
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает экземпляры семейств категории "Обобщенные модели" из текущего документа Revit
        /// </summary>
        /// <returns></returns>
        private ICollection<FamilyInstance> GetGenericModelsFamilyInstances() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToHashSet();
        }

        private IList<RevitLinkInstance> GetRevitLinks() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(link => RevitLinkType.IsLoaded(
                                   _document,
                                   link.GetTypeId()))
                .ToList();
        }



        /// <summary>
        /// Возвращает задания на отверстия от инженера из текущего файла Revit
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private List<FamilyInstance> GetOpeningsMepTasksOutcoming(ICollection<OpeningType> types) {
            return GetGenericModelsFamilyInstances()
               .Where(item => types.Any(e => OpeningTaskTypeName[e].Equals(item.Name))
                           && types.Any(e => OpeningTaskFamilyName[e].Equals(GetFamilyName(item))))
               .ToList();
        }

        private static T GetPlatformService<T>() {
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

    /// <summary>
    /// Перечисление используемых нелинейных элементов
    /// </summary>
    internal enum FittingCategoryEnum {
        /// <summary>
        /// Категории нелинейных элементов для труб (Соединительные детали трубопроводов)
        /// </summary>
        PipeFitting,
        /// <summary>
        /// Категории нелинейных элементов для кабельных лотков (Соединительные детали кабельных лотков)
        /// </summary>
        CableTrayFitting,
        /// <summary>
        /// Категории нелинейных элементов для воздуховодов (Соединительные детали воздуховодов)
        /// </summary>
        DuctFitting,
        /// <summary>
        /// Категории нелинейных элементов для коробов (Соединительные детали коробов)
        /// </summary>
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

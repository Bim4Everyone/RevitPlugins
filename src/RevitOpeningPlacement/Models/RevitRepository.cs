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

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

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

namespace RevitOpeningPlacement.Models;
internal class RevitRepository {
    private readonly Application _application;
    private readonly UIDocument _uiDocument;

    private readonly RevitClashDetective.Models.RevitRepository _clashRevitRepository;
    private readonly ILocalizationService _localization;
    private readonly View3DProvider _view3DProvider;
    private readonly View3D _view;
    private readonly List<ElementId> _linkTypeIdsToUse;

    public RevitRepository(
        UIApplication uiApplication,
        RevitClashDetective.Models.RevitRepository clashRepository,
        ILocalizationService localization) {

        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        _clashRevitRepository = clashRepository ?? throw new ArgumentNullException(nameof(clashRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _application = UIApplication.Application;
        _uiDocument = UIApplication.ActiveUIDocument;
        Doc = _uiDocument.Document;
        _linkTypeIdsToUse = GetAllRevitLinkTypes().Select(t => t.Id).ToList();

        _view3DProvider = new View3DProvider();
        _view = _view3DProvider.GetView(Doc, $"BIM_Задания на отверстия_{_application.Username}");
    }

    public UIApplication UIApplication { get; }
    public List<DocInfo> DocInfos => _clashRevitRepository.DocInfos;

    public Document Doc { get; }

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
    public static Dictionary<OpeningType, string> OpeningTaskTypeName => new() {
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
    public static Dictionary<OpeningType, string> OpeningRealArTypeName => new() {
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
    /// Категории конструкций, недопустимые для расстановки заданий на отверстия
    /// </summary>
    public static IReadOnlyCollection<BuiltInCategory> UnacceptableStructureCategories { get; } =
        new ReadOnlyCollection<BuiltInCategory>(new BuiltInCategory[] {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming
        });

    /// <summary>
    /// Используемые в плагине категории для стен: Стены
    /// </summary>
    public static BuiltInCategory WallCategory { get; } = BuiltInCategory.OST_Walls;

    /// <summary>
    /// Используемые в плагине категории для перекрытий: Перекрытия
    /// </summary>
    public static BuiltInCategory FloorCategory { get; } = BuiltInCategory.OST_Floors;

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
    public const string OpeningOffsetAdsk = "ADSK_Отверстие_Отметка от нуля";
    public const string OpeningOffsetFromLevelAdsk = "ADSK_Отверстие_Отметка от этажа";
    public const string OpeningLevelOffsetAdsk = "ADSK_Отверстие_Отметка этажа";
    public const string OpeningOffsetAdskOld = "ADSK_Отверстие_ОтметкаОтНуля";
    public const string OpeningOffsetFromLevelAdskOld = "ADSK_Отверстие_ОтметкаОтЭтажа";
    public const string OpeningLevelOffsetAdskOld = "ADSK_Отверстие_ОтметкаЭтажа";

    public static List<BuiltInParameter> MepCurveDiameters => [
        BuiltInParameter.RBS_PIPE_OUTER_DIAMETER,
        BuiltInParameter.RBS_CURVE_DIAMETER_PARAM,
        BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM
    ];

    public static List<BuiltInParameter> MepCurveHeights => [
        BuiltInParameter.RBS_CURVE_HEIGHT_PARAM,
        BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM
    ];

    public static List<BuiltInParameter> MepCurveWidths => [
        BuiltInParameter.RBS_CURVE_WIDTH_PARAM,
        BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM
    ];

    public static List<BuiltInParameter> BottomElevation => [
        BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION,
        BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION,
        BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION
    ];

    public static List<BuiltInParameter> TopElevation => [
        BuiltInParameter.RBS_CTC_TOP_ELEVATION,
        BuiltInParameter.RBS_DUCT_TOP_ELEVATION,
        BuiltInParameter.RBS_PIPE_TOP_ELEVATION
    ];

    public static string SystemCheck => "Системная проверка";

    /// <summary>
    /// Возвращает типоразмер семейства задания на отверстие из репозитория
    /// </summary>
    public FamilySymbol GetOpeningTaskType(OpeningType type) {
        return Doc.GetElement(
            GetFamilySymbol(Doc, OpeningTaskFamilyName[type], OpeningTaskTypeName[type])) as FamilySymbol;
    }

    /// <summary>
    /// Возвращает типоразмер семейства чистового отверстия АР из репозитория
    /// </summary>
    public FamilySymbol GetOpeningRealArType(OpeningType type) {
        return Doc.GetElement(
            GetFamilySymbol(Doc, OpeningRealArFamilyName[type], OpeningRealArTypeName[type])) as FamilySymbol;
    }

    /// <summary>
    /// Возвращает типоразмер семейства чистового отверстия КР из репозитория
    /// </summary>
    public FamilySymbol GetOpeningRealKrType(OpeningType type) {
        return Doc.GetElement(
            GetFamilySymbol(Doc, OpeningRealKrFamilyName[type], OpeningRealKrTypeName[type])) as FamilySymbol;
    }

    /// <summary>
    /// Возвращает семейство задания на отверстие из репозитория
    /// </summary>
    public Family GetOpeningTaskFamily(OpeningType openingType) {
        return Doc.GetElement(
            GetFamily(Doc, OpeningTaskFamilyName[openingType])) as Family;
    }

    /// <summary>
    /// Возвращает семейство чистового отверстия АР из репозитория
    /// </summary>
    public Family GetOpeningRealArFamily(OpeningType openingType) {
        return Doc.GetElement(
            GetFamily(Doc, OpeningRealArFamilyName[openingType])) as Family;
    }

    /// <summary>
    /// Возвращает семейство чистового отверстия КР из репозитория
    /// </summary>
    public Family GetOpeningRealKrFamily(OpeningType openingType) {
        return Doc.GetElement(
            GetFamily(Doc, OpeningRealKrFamilyName[openingType])) as Family;
    }

    public Transaction GetTransaction(string transactionName) {
        return Doc.StartTransaction(transactionName);
    }

    public Level GetLevel(string name) {
        return new FilteredElementCollector(Doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .FirstOrDefault(item => item.Name.Equals(name, StringComparison.CurrentCulture));
    }

    public static Level GetLevel(Element element) {
        return RevitClashDetective.Models.RevitRepository.GetLevel(element);
    }

    public Element GetElement(ElementId id) {
        return Doc.GetElement(id);
    }

    public void SelectAndShowElement(ICollection<ElementModel> elements) {
        double additionalSize = 2;
        _clashRevitRepository.SelectAndShowElement(elements, additionalSize, _view);
    }

    public void SelectAndShowElement(ISelectorAndHighlighter selectorAndHighlighter) {
        var elementToHighlight = selectorAndHighlighter.GetElementToHighlight();
        if(elementToHighlight != null) {
            try {
                new ElementHighlighter(this, _view, elementToHighlight, GetMessageBoxService(), _localization)
                    .HighlightElement();
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
    public string GetDocumentName() {
        return _clashRevitRepository.GetDocumentName();
    }

    /// <summary>
    /// Возвращает сервис для работы с разделами проектной документации
    /// </summary>
    public static IBimModelPartsService GetBimModelPartsService() {
        return GetPlatformService<IBimModelPartsService>();
    }

    /// <summary>
    /// Размещает экземпляр заданного типоразмера семейства в хосте по точке вставки с уровнем по хосту
    /// </summary>
    /// <param name="point">Точка вставки</param>
    /// <param name="familySymbol">Типоразмер семейства</param>
    /// <param name="host">Хост экземпляра семейства</param>
    /// <exception cref="System.ArgumentNullException">Исключение, обязательный параметр null</exception>
    public FamilyInstance CreateInstance(XYZ point, FamilySymbol familySymbol, Element host) {
        if(point is null) { throw new ArgumentNullException(nameof(point)); }
        if(familySymbol is null) { throw new ArgumentNullException(nameof(familySymbol)); }
        if(host is null) { throw new ArgumentNullException(nameof(host)); }

        if(!familySymbol.IsActive) { familySymbol.Activate(); }

        var level = GetElement(host.LevelId) as Level;
        var inst = Doc.Create.NewFamilyInstance(point, familySymbol, host, level, StructuralType.NonStructural);
        Doc.Regenerate(); // решение бага, когда значения параметров, которые назначались этому экземпляру сразу после создания, по итогу не назначались
        return inst;
    }

    public FamilyInstance CreateInstance(FamilySymbol type, XYZ point, Level level) {
        if(!type.IsActive) {
            type.Activate();
        }
        FamilyInstance inst;
        if(level != null) {
            point -= XYZ.BasisZ * level.ProjectElevation;
            inst = Doc.Create.NewFamilyInstance(point, type, level, StructuralType.NonStructural);
        } else {
            inst = Doc.Create.NewFamilyInstance(point, type, StructuralType.NonStructural);
        }
        Doc.Regenerate(); // решение бага, когда значения параметров, которые назначались этому экземпляру сразу после создания, по итогу не назначались
        return inst;
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
    public ICollection<OpeningMepTaskOutcoming> GetOpeningsMepTasksOutcoming() {
        var openingsInWalls = GetWallOpeningsMepTasksOutcoming();
        var openingsInFloor = GetFloorOpeningsMepTasksOutcoming();
        openingsInFloor.AddRange(openingsInWalls);
        return openingsInFloor.Select(famInst => new OpeningMepTaskOutcoming(famInst)).ToHashSet();
    }

    /// <summary>
    /// Возвращает исходящие задания на отверстия в стенах от инженера из текущего файла Revit
    /// </summary>
    public List<FamilyInstance> GetWallOpeningsMepTasksOutcoming() {
        var wallTypes = new[] { OpeningType.WallRectangle, OpeningType.WallRound };
        return GetOpeningsMepTasks(Doc, wallTypes);
    }

    /// <summary>
    /// Возвращает исходящие задания на отверстия в перекрытиях от инженера из текущего файла Revit
    /// </summary>
    public List<FamilyInstance> GetFloorOpeningsMepTasksOutcoming() {
        var floorTypes = new[] { OpeningType.FloorRectangle, OpeningType.FloorRound };
        return GetOpeningsMepTasks(Doc, floorTypes);
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
    public ICollection<OpeningMepTaskOutcoming> GetPlacedOutcomingTasks() {
        return GetOpeningsTasks(Doc).Select(f => new OpeningMepTaskOutcoming(f)).ToHashSet();
    }

    public void DeleteElements(ICollection<ElementId> elements) {
        using var t = Doc.StartTransaction(_localization.GetLocalizedString("Transaction.DeleteUnitedTasks"));
        Doc.Delete(elements);
        t.Commit();
    }

    /// <summary>
    /// Удаляет элемент из документа Revit без запуска транзакции
    /// </summary>
    /// <param name="elementId">Id элемента, который нужно удалить</param>
    public void DeleteElement(ElementId elementId) {
        Doc.Delete(elementId);
    }

    /// <summary>
    /// Объединяет задания на отверстия из активного документа и удаляет старые
    /// </summary>
    /// <param name="openingTasks">Коллекция объединяемых заданий на отверстия</param>
    /// <param name="config">Настройки расстановки заданий на отверстия</param>
    /// <exception cref="OperationCanceledException">Исключение, если пользователь отменил операцию</exception>
    public FamilyInstance UniteOpenings(ICollection<OpeningMepTaskOutcoming> openingTasks, OpeningConfig config) {
        var placer = GetOpeningPlacer(openingTasks, config);
        FamilyInstance createdOpening = null;
        try {
            using var t = GetTransaction(_localization.GetLocalizedString("Transaction.UniteTasks"));
            createdOpening = placer.Place();
            t.Commit();

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
        _clashRevitRepository.DoAction(action);
    }

    public List<ParameterValueProvider> GetParameters(Document doc, IEnumerable<Category> categories) {
        return _clashRevitRepository.GetParameters(doc, categories);
    }

    /// <summary>
    /// Возвращает значение элемента перечисления категорий инженерных систем
    /// </summary>
    /// <param name="mepCategoryName">Название категории инженерных систем</param>
    public MepCategoryEnum GetMepCategoryEnum(string mepCategoryName) {
        return MepCategoryNames
            .First(pair => pair.Value.Equals(mepCategoryName, StringComparison.CurrentCultureIgnoreCase))
            .Key;
    }

    /// <summary>
    /// Возвращает значение элемента перечисления категорий конструкций
    /// </summary>
    /// <param name="structureCategoryName">Название категории конструкций</param>
    public StructureCategoryEnum GetStructureCategoryEnum(string structureCategoryName) {
        return StructureCategoryNames
            .First(pair => pair.Value.Equals(structureCategoryName, StringComparison.CurrentCultureIgnoreCase))
            .Key;
    }

    /// <summary>
    /// Возвращает массив категорий Revit, которые соответствуют заданному <see cref="MepCategoryEnum"/>
    /// </summary>
    /// <param name="mepCategory">Категория элементов инженерных систем</param>
    /// <exception cref="NotSupportedException">Исключение, 
    /// если поданная категория <paramref name="mepCategory"/> не поддерживается</exception>
    public Category[] GetCategories(MepCategoryEnum mepCategory) {
        var categoryCollection = mepCategory switch {
            MepCategoryEnum.Pipe => MepPipeCategories,
            MepCategoryEnum.RectangleDuct or MepCategoryEnum.RoundDuct => MepDuctCategories,
            MepCategoryEnum.CableTray => MepCableTrayCategories,
            MepCategoryEnum.Conduit => MepConduitCategories,
            _ => throw new NotSupportedException(nameof(mepCategory)),
        };
        return categoryCollection.Select(c => Category.GetCategory(Doc, c)).ToArray();
    }

    /// <summary>
    /// Возвращает массив категорий Revit, которые соответствуют заданному <see cref="StructureCategoryEnum"/>
    /// </summary>
    /// <param name="structureCategory">Категория элементов конструкций</param>
    /// <exception cref="NotSupportedException">Исключение, 
    /// если поданная категория <paramref name="structureCategory"/> не поддерживается</exception>
    public Category[] GetCategories(StructureCategoryEnum structureCategory) {
        var category = structureCategory switch {
            StructureCategoryEnum.Wall => WallCategory,
            StructureCategoryEnum.Floor => FloorCategory,
            _ => throw new NotSupportedException(nameof(structureCategory)),
        };
        return new Category[] { Category.GetCategory(Doc, category) };
    }

    public bool ElementBelongsToMepCategory(MepCategoryEnum mepCategory, Element element) {
        var elCategory = element.Category.GetBuiltInCategory();
        switch(mepCategory) {
            case MepCategoryEnum.Pipe:
                return MepPipeCategories.Contains(elCategory);
            case MepCategoryEnum.RectangleDuct: {
                //либо это элемент из категории для воздуховодов и не Воздуховод, либо это Воздуховод только прямоугольного сечения
                return MepDuctCategories.Contains(elCategory)
                    && (element is not Duct
                    || ((element is Duct duct)
                    && (duct.DuctType.Shape == ConnectorProfileType.Rectangular)));
            }
            case MepCategoryEnum.RoundDuct: {
                //либо это элемент из категории для воздуховодов и не Воздуховод, либо это Воздуховод только круглого сечения
                return MepDuctCategories.Contains(elCategory)
                    && (element is not Duct
                    || ((element is Duct duct)
                    && (duct.DuctType.Shape == ConnectorProfileType.Round)));
            }
            case MepCategoryEnum.CableTray:
                return MepCableTrayCategories.Contains(elCategory);
            case MepCategoryEnum.Conduit:
                return MepConduitCategories.Contains(elCategory);
            default:
                throw new NotImplementedException(nameof(mepCategory));
        }
    }

    /// <summary>
    /// Спрашивает у пользователя, нужно ли продолжать операцию, 
    /// если семейства заданий на отверстия не самой последней версии
    /// </summary>
    public bool ContinueIfTaskFamiliesNotLatest() {
        var checker = new FamiliesParametersChecker(this, _localization);
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
    /// Возвращает прогресс бар
    /// </summary>
    public IProgressDialogService GetProgressDialogService() {
        return GetPlatformService<IProgressDialogService>();
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий из текущего АР документа Revit
    /// </summary>
    public ICollection<OpeningRealAr> GetRealOpeningsAr() {
        return GetRealOpeningsAr(Doc);
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий из заданного АР документа Revit
    /// </summary>
    public ICollection<OpeningRealAr> GetRealOpeningsAr(Document document) {
        return GetOpeningsAr(document)
            .Where(famInst => famInst.Host != null)
            .Select(famInst => new OpeningRealAr(famInst))
            .ToHashSet();
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий из текущего КР документа Revit
    /// </summary>
    public ICollection<OpeningRealKr> GetRealOpeningsKr() {
        return GetRealOpeningsKr(Doc);
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий из заданного КР документа Revit
    /// </summary>
    public ICollection<OpeningRealKr> GetRealOpeningsKr(Document document) {
        return GetOpeningsKr(document)
            .Where(famInst => famInst.Host != null)
            .Select(famInst => new OpeningRealKr(famInst))
            .ToHashSet();
    }

    /// <summary>
    /// Возвращает коллекцию Id всех элементов конструкций из текущего документа ревита, 
    /// для которых создаются задания на отверстия
    /// </summary>
    public ICollection<ElementId> GetConstructureElementsIds() {
        return GetConstructureElementsIds(Doc);
    }

    /// <summary>
    /// Возвращает коллекцию Id всех элементов конструкций из заданного документа ревита, 
    /// для которых создаются задания на отверстия
    /// </summary>
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
    public ICollection<ElementId> GetMepElementsIds() {
        return new FilteredElementCollector(Doc)
            .WherePasses(FiltersInitializer.GetFilterByAllUsedMepCategories())
            .ToElementIds();
    }

    /// <summary>
    /// Возвращает коллекцию всех входящих заданий на отверстия из связанных файлов ВИС
    /// </summary>
    public ICollection<OpeningMepTaskIncoming> GetOpeningsMepTasksIncoming() {
        var links = GetSelectedRevitLinks();
        HashSet<OpeningMepTaskIncoming> genericModelsInLinks = [];
        foreach(var link in links) {
            var linkDoc = link.GetLinkDocument();
            var transform = link.GetTransform();
            var genericModelsInLink = GetOpeningsTasks(linkDoc)
                .Select(famInst => new OpeningMepTaskIncoming(famInst, this, transform))
                .ToHashSet();
            genericModelsInLinks.UnionWith(genericModelsInLink);
        }
        return genericModelsInLinks;
    }

    /// <summary>
    /// Возвращает коллекцию всех входящих заданий на отверстия из связанных файлов АР
    /// </summary>
    public ICollection<OpeningArTaskIncoming> GetOpeningsArTasksIncoming() {
        var links = GetSelectedRevitLinks();
        HashSet<OpeningArTaskIncoming> openingsArInLinks = [];
        foreach(var link in links) {
            var linkDoc = link.GetLinkDocument();
            var transform = link.GetTransform();
            var openingsArInLink = GetOpeningsAr(linkDoc)
                .Select(famInst => new OpeningArTaskIncoming(this, famInst, transform));
            openingsArInLinks.UnionWith(openingsArInLink);
        }
        return openingsArInLinks;
    }

    /// <summary>
    /// Находит экземпляры связей из активного документа, 
    /// типоразмеры которых были настроены через метод <see cref="SetRevitLinkTypesToUse"/>
    /// </summary>
    /// <returns>Коллекция экземпляров связей, в которой находятся связи, выбранные пользователем</returns>
    public IList<RevitLinkInstance> GetSelectedRevitLinks() {
        return new FilteredElementCollector(Doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(link => _linkTypeIdsToUse.Contains(link.GetTypeId())
                && RevitLinkType.IsLoaded(Doc, link.GetTypeId()))
            .ToList();
    }

    /// <summary>
    /// Предлагает пользователю выбрать системную стену или системное перекрытие и возвращает его выбор
    /// </summary>
    /// <returns>Выбранный пользователем элемент классов <see cref="Autodesk.Revit.DB.Wall"/> 
    /// или <see cref="Autodesk.Revit.DB.Floor"/></returns>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исклчюение, если пользователь прервал операцию</exception>
    public Element PickHostForRealOpening() {
        // фильтр по классам, а не по категориям ревита,
        // так как для хоста нужна системная стена или системное перекрытие,
        // при этом необходимо исключить выбор моделей в контексте, которые могут быть стенами и перекрытиями
        ISelectionFilter filter = new SelectionFilterElementsOfClasses(new Type[] { typeof(Wall), typeof(Floor) });
        var reference = _uiDocument.Selection.PickObject(
            ObjectType.Element,
            filter,
            "Выберите стену или перекрытие");
        return Doc.GetElement(reference);
    }

    /// <summary>
    /// Предлагает пользователю выбрать стены и перекрытия и возвращает его выбор
    /// </summary>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public ICollection<Element> PickHostsForRealOpenings() {
        // фильтр по классам, а не по категориям ревита,
        // так как для хоста нужна системная стена или системное перекрытие,
        // при этом необходимо исключить выбор моделей в контексте, которые могут быть стенами и перекрытиями
        ISelectionFilter filter = new SelectionFilterElementsOfClasses(new Type[] { typeof(Wall), typeof(Floor) });
        var references = _uiDocument.Selection.PickObjects(
            ObjectType.Element,
            filter,
            _localization.GetLocalizedString("RevitUI.PickWallsOrFloors"));

        HashSet<Element> hosts = [];
        foreach(var reference in references) {
            if(reference != null) {
                var element = Doc.GetElement(reference);
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
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public ICollection<OpeningMepTaskIncoming> PickManyOpeningMepTasksIncoming() {
        ISelectionFilter filter = new SelectionFilterOpeningMepTasksIncoming(Doc);
        var references = _uiDocument.Selection.PickObjects(
            ObjectType.LinkedElement,
            filter,
            _localization.GetLocalizedString("RevitUI.PickOpeningMepTasks"));

        HashSet<OpeningMepTaskIncoming> openingTasks = [];
        foreach(var reference in references) {
            if((reference != null) && (Doc.GetElement(reference) is RevitLinkInstance link)) {
                var opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                if(opening is not null and FamilyInstance famInst) {
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
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public ICollection<OpeningMepTaskOutcoming> PickManyOpeningMepTasksOutcoming() {
        ISelectionFilter filter = new SelectionFilterOpeningMepTasksOutcoming();
        var references = _uiDocument.Selection.PickObjects(
            ObjectType.Element,
            filter,
            "Выберите исходящие задания на отверстия и нажмите \"Готово\"");

        HashSet<OpeningMepTaskOutcoming> openingTasks = [];
        foreach(var reference in references) {
            if((reference != null) && (Doc.GetElement(reference) is FamilyInstance famInst)) {
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
    /// <exception cref="OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public OpeningMepTaskIncoming PickSingleOpeningMepTaskIncoming() {
        ISelectionFilter filter = new SelectionFilterOpeningMepTasksIncoming(Doc);
        var reference = _uiDocument.Selection.PickObject(
            ObjectType.LinkedElement,
            filter,
            _localization.GetLocalizedString("RevitUI.PickOpeningMepTask"));

        if((reference != null) && (Doc.GetElement(reference) is RevitLinkInstance link)) {
            var opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
            if(opening is not null and FamilyInstance famInst) {
                return new OpeningMepTaskIncoming(famInst, this, link.GetTransform());
            } else {
                ShowErrorMessage(_localization.GetLocalizedString("Validation.InvalidTaskFamily"));
                throw new OperationCanceledException();
            }
        } else {
            ShowErrorMessage(_localization.GetLocalizedString("Errors.InvalidElement"));
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Предлагает пользователю выбрать один экземпляр семейства задания на отверстие из связанных файлов АР, 
    /// подгруженных в активный документ, и возвращает его выбор
    /// </summary>
    /// <returns>Выбранный пользователем элемент</returns>
    /// <exception cref="OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public OpeningArTaskIncoming PickSingleOpeningArTaskIncoming() {
        ISelectionFilter filter = new SelectionFilterOpeningArTasksIncoming(Doc);
        var reference = _uiDocument.Selection.PickObject(
            ObjectType.LinkedElement,
            filter,
            _localization.GetLocalizedString("RevitUI.PickOpeningArTask"));

        if((reference != null) && (Doc.GetElement(reference) is RevitLinkInstance link)) {
            var opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
            if(opening is not null and FamilyInstance famInst) {
                return new OpeningArTaskIncoming(this, famInst, link.GetTransform());
            } else {
                ShowErrorMessage(_localization.GetLocalizedString("Errors.InvalidTaskFamily"));
                throw new OperationCanceledException();
            }
        } else {
            ShowErrorMessage(_localization.GetLocalizedString("Errors.InvalidElement"));
            throw new OperationCanceledException();
        }
    }

    /// <summary>
    /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из связанных файлов АР, 
    /// подгруженных в активный документ КР, и возвращает его выбор
    /// </summary>
    /// <returns>Выбранная пользователем коллекция элементов</returns>
    /// <exception cref="OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Исключение, если пользователь прервал операцию</exception>
    public ICollection<OpeningArTaskIncoming> PickManyOpeningArTasksIncoming() {
        ISelectionFilter filter = new SelectionFilterOpeningArTasksIncoming(Doc);
        var references = _uiDocument.Selection
            .PickObjects(
            ObjectType.LinkedElement,
            filter,
            _localization.GetLocalizedString("RevitUI.PickOpeningArTasks"));

        HashSet<OpeningArTaskIncoming> openingTasks = [];
        foreach(var reference in references) {
            if((reference != null) && (Doc.GetElement(reference) is RevitLinkInstance link)) {
                var opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                if(opening is not null and FamilyInstance famInst) {
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
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public ElementId[] PickMepElements(MepCategoryCollection mepCategories) {
        if(mepCategories is null) { throw new ArgumentNullException(nameof(mepCategories)); }

        var categories = mepCategories
            .Where(c => c.IsSelected)
            .Select(c => GetMepCategoryEnum(c.Name))
            .ToArray();
        ISelectionFilter filter = new SelectionFilterMepElements(this, categories);

        var references = _uiDocument.Selection
            .PickObjects(ObjectType.Element, filter, _localization.GetLocalizedString("RevitUI.PickMepElements"));

        HashSet<ElementId> mepElements = [];
        foreach(var reference in references) {
            if(reference != null) {
                var elId = reference.ElementId;
                mepElements.Add(elId);
            }
        }
        return mepElements.ToArray();
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
    /// Возвращает коллекцию заголовков файлов Revit связей, 
    /// которые дублируются и среди которых есть родительские связи.
    /// То есть дублирующиеся вложенные связи не будут попадать в список.
    /// </summary>
    /// <returns>Коллекция заголовков дублированных Revit-связей</returns>
    public ICollection<string> GetDuplicatedLinksNames() {
        return new FilteredElementCollector(Doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(link => RevitLinkType.IsLoaded(Doc, link.GetTypeId()))
            .GroupBy(inst => inst.GetLinkDocument().Title)
            .Where(group => group.Count() > 1)
            .Where(group => group.ToArray().Any(linkInstance => _clashRevitRepository.IsParentLink(linkInstance)))
            .Select(group => group.Key)
            .ToHashSet();
    }

    /// <summary>
    /// Возвращает ссылку на документ семейства
    /// </summary>
    /// <exception cref="Autodesk.Revit.Exceptions.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="Autodesk.Revit.Exceptions.ArgumentException">Исключение, нельзя получить ссылку на документ</exception>
    /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException">Исключение, нельзя получить ссылку на документ</exception>
    /// <exception cref="Autodesk.Revit.Exceptions.ForbiddenForDynamicUpdateException">Исключение, нельзя получить ссылку на документ</exception>
    public Document EditFamily(Family family) {
        return Doc.EditFamily(family);
    }

    /// <summary>
    /// Выводит сообщение об ошибке
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public void ShowErrorMessage(string message) {
        var dialog = GetMessageBoxService();
        dialog.Show(
            message,
            _localization.GetLocalizedString("OpeningTasks"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error,
            System.Windows.MessageBoxResult.OK);
    }

    /// <summary>
    /// Задает коллекцию типоразмеров связей, подгруженных в активный документ,<br/>
    /// которые будут в дальнейшем использоваться в алгоритмах плагина.<br/>
    /// Не загруженные типы связей будут загружены в результате выполнения метода.
    /// </summary>
    /// <param name="linkTypes">Типы связей, которые нужно использовать.</param>
    public void SetRevitLinkTypesToUse(IEnumerable<RevitLinkType> linkTypes) {
        if(linkTypes is null) {
            throw new ArgumentNullException(nameof(linkTypes));
        }

        _linkTypeIdsToUse.Clear();
        foreach(var linkType in linkTypes) {
            if(!RevitLinkType.IsLoaded(Doc, linkType.Id)) {
                try {
                    var result = linkType.Load();
                    if(result.LoadResult != LinkLoadResultType.LinkLoaded) {
                        continue;
                    }
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    continue;
                }
            }
            _linkTypeIdsToUse.Add(linkType.Id);

        }
        _clashRevitRepository.InitializeDocInfos();
    }

    public ICollection<RevitLinkType> GetAllRevitLinkTypes() {
        return new FilteredElementCollector(Doc)
            .WhereElementIsElementType()
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .ToArray();
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий КР из документа Revit
    /// </summary>
    public ICollection<FamilyInstance> GetOpeningsKr(Document document) {
        List<FamilyInstance> elements = [];
        foreach(var openingType in Enum.GetValues(typeof(OpeningType))
            .OfType<OpeningType>()
            .Where(t => t != OpeningType.FloorRound)) {
            elements.AddRange(
                GetFamilyInstances(document,
                OpeningRealKrFamilyName[openingType],
                OpeningRealKrTypeName[openingType]));
        }
        return elements;
    }

    /// <summary>
    /// Возвращает коллекцию чистовых экземпляров семейств отверстий АР из документа Revit
    /// </summary>
    private ICollection<FamilyInstance> GetOpeningsAr(Document document) {
        List<FamilyInstance> elements = [];
        foreach(var openingType in Enum.GetValues(typeof(OpeningType)).OfType<OpeningType>()) {
            elements.AddRange(
                GetFamilyInstances(document,
                OpeningRealArFamilyName[openingType],
                OpeningRealArTypeName[openingType]));
        }
        return elements;
    }

    /// <summary>
    /// Возвращает коллекцию экземпляров семейств заданий на отверстия от ВИС
    /// </summary>
    /// <param name="doc">Документ, в котором будет происходить поиск экземпляров семейств</param>
    /// <returns>Коллекция экземпляров семейств заданий на отверстия от ВИС</returns>
    private ICollection<FamilyInstance> GetOpeningsTasks(Document doc) {
        return GetOpeningsMepTasks(doc,
            Enum.GetValues(typeof(OpeningType))
            .OfType<OpeningType>()
            .ToArray());
    }

    /// <summary>
    /// Возвращает id семейства с заданным названием из заданного документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <param name="familyName">Название семейства</param>
    /// <returns>Id семейства</returns>
    private ElementId GetFamily(Document doc, string familyName) {
        return doc is null
            ? throw new ArgumentNullException(nameof(doc))
            : string.IsNullOrWhiteSpace(familyName)
            ? throw new ArgumentException(nameof(familyName))
            : new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .FirstOrDefault(family => family.Name.Equals(familyName, StringComparison.InvariantCultureIgnoreCase))
            ?.Id ?? ElementId.InvalidElementId;
    }

    /// <summary>
    /// Возвращает заданный типоразмер из заданного семейства из заданного документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <param name="familyName">Название семейства</param>
    /// <param name="symbolName">Название типоразмера</param>
    /// <returns>Id типоразмера семейства</returns>
    private ElementId GetFamilySymbol(Document doc, string familyName, string symbolName) {
        if(doc is null) {
            throw new ArgumentNullException(nameof(doc));
        }
        if(string.IsNullOrWhiteSpace(familyName)) {
            throw new ArgumentException(nameof(familyName));
        }
        if(string.IsNullOrWhiteSpace(symbolName)) {
            throw new ArgumentException(nameof(symbolName));
        }
        var familyId = GetFamily(doc, familyName);
        return familyId.IsNotNull()
            ? new FilteredElementCollector(doc)
                .WherePasses(new FamilySymbolFilter(familyId))
                .FirstOrDefault(s => s.Name.Equals(symbolName, StringComparison.InvariantCultureIgnoreCase))
                ?.Id ?? ElementId.InvalidElementId
            : ElementId.InvalidElementId;
    }

    /// <summary>
    /// Возвращает коллекцию всех экземпляров семейств заданного типоразмера заданного семейства из документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <param name="familyName">Название семейства</param>
    /// <param name="symbolName">Название типоразмера</param>
    /// <returns>Коллкция экземпляров семейств</returns>
    private ICollection<FamilyInstance> GetFamilyInstances(Document doc, string familyName, string symbolName) {
        if(doc is null) {
            throw new ArgumentNullException(nameof(doc));
        }
        if(string.IsNullOrWhiteSpace(familyName)) {
            throw new ArgumentException(nameof(familyName));
        }
        if(string.IsNullOrWhiteSpace(symbolName)) {
            throw new ArgumentException(nameof(symbolName));
        }
        var symbolId = GetFamilySymbol(doc, familyName, symbolName);
        return symbolId.IsNotNull()
            ? new FilteredElementCollector(doc)
                .WherePasses(new FamilyInstanceFilter(doc, symbolId))
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToArray()
            : Array.Empty<FamilyInstance>();
    }

    /// <summary>
    /// Возвращает класс, размещающий объединенное задание на отверстие
    /// </summary>
    /// <param name="openingTasks">Задания на отверстия из активного документа, которые надо объединить</param>
    /// <param name="config">Настройки расстановки заданий на отверстия</param>
    /// <exception cref="System.OperationCanceledException">Исключение, операцию отменил пользователь</exception>
    private OpeningPlacer GetOpeningPlacer(ICollection<OpeningMepTaskOutcoming> openingTasks, OpeningConfig config) {
        try {
            var group = new OpeningsGroup(openingTasks);
            return group.GetOpeningPlacer(this, config);

        } catch(ArgumentNullException nullEx) {
            ShowErrorMessage(nullEx.Message);
            throw new OperationCanceledException();

        } catch(ArgumentOutOfRangeException) {
            ShowErrorMessage(_localization.GetLocalizedString("Errors.SelectTwoTasksAtLeast"));
            throw new OperationCanceledException();
        } catch(InvalidOperationException) {
            ShowErrorMessage(_localization.GetLocalizedString("Errors.CannotUniteTasks"));
            throw new OperationCanceledException();
        }
    }

    private void RotateElement(Element element, Line axis, double angle) {
        if(Math.Abs(angle) > 0.00001) {
            ElementTransformUtils.RotateElement(Doc, element.Id, axis, angle);
        }
    }

    /// <summary>
    /// Возвращает задания на отверстия от инженера из текущего файла Revit
    /// </summary>
    private List<FamilyInstance> GetOpeningsMepTasks(Document document, ICollection<OpeningType> types) {
        List<FamilyInstance> elements = [];
        foreach(var type in types) {
            elements.AddRange(
                GetFamilyInstances(document,
                OpeningTaskFamilyName[type],
                OpeningTaskTypeName[type]));
        }
        return elements;
    }

    /// <summary>
    /// Возвращает сервис диалоговых окон
    /// </summary>
    private IMessageBoxService GetMessageBoxService() {
        return GetPlatformService<IMessageBoxService>();
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

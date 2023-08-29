using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
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

        public Document Doc => _document;

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

        /// <summary>
        /// Словарь типов проемов и названий семейств заданий на отверстия
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningTaskFamilyName { get; } = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В перекрытии" },
            {OpeningType.FloorRound, "ОбщМд_Отв_Отверстие_Круглое_В перекрытии" },
            {OpeningType.WallRectangle, "ОбщМд_Отв_Отверстие_Прямоугольное_В стене" },
            {OpeningType.WallRound, "ОбщМд_Отв_Отверстие_Круглое_В стене" },
        };

        /// <summary>
        /// Словарь типов проемов и названий семейств чистовых отверстий
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningRealFamilyName { get; } = new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Окн_Отв_Прямоуг_Перекрытие" },
            {OpeningType.FloorRound, "Окн_Отв_Круг_Перекрытие" },
            {OpeningType.WallRectangle, "Окн_Отв_Прямоуг_Стена" },
            {OpeningType.WallRound, "Окн_Отв_Круг_Стена" },
        };

        public static List<string> WallFamilyNames { get; } = new List<string> {
            OpeningTaskFamilyName[OpeningType.WallRound],
            OpeningTaskFamilyName[OpeningType.WallRectangle]
        };

        public static List<string> FloorFamilyNames { get; } = new List<string> {
            OpeningTaskFamilyName[OpeningType.FloorRectangle],
            OpeningTaskFamilyName[OpeningType.FloorRound]
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
        /// Словарь типов проемов и названий типов семейств чистовых отверстий
        /// </summary>
        public static Dictionary<OpeningType, string> OpeningRealTypeName => new Dictionary<OpeningType, string>() {
            {OpeningType.FloorRectangle, "Окн_Отв_Прямоуг_Перекрытие" },
            {OpeningType.FloorRound, "Окн_Отв_Круг_Перекрытие" },
            {OpeningType.WallRectangle, "Окн_Отв_Прямоуг_Стена" },
            {OpeningType.WallRound, "Окн_Отв_Круг_Стена" },
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
        public const string OpeningAuthor = "ФОП_Автор задания";

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
                .FirstOrDefault(item => item.Name.Equals(OpeningTaskTypeName[type]) && item.FamilyName.Equals(OpeningTaskFamilyName[type]));
        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия из репозитория
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FamilySymbol GetOpeningRealType(OpeningType type) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .FirstOrDefault(item => item.Name.Equals(OpeningRealTypeName[type]) && item.FamilyName.Equals(OpeningRealFamilyName[type]));
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
                .FirstOrDefault(item => item?.Name?.Equals(OpeningTaskFamilyName[openingType], StringComparison.CurrentCulture) == true);
        }

        /// <summary>
        /// Возвращает семейство чистового отверстия из репозитория
        /// </summary>
        /// <param name="openingType"></param>
        /// <returns></returns>
        public Family GetOpeningRealFamily(OpeningType openingType) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .OfType<Family>()
                .FirstOrDefault(item => item?.Name?.Equals(OpeningRealFamilyName[openingType], StringComparison.CurrentCulture) == true);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.ArgumentNullException"/>
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
        public IBimModelPartsService GetBimModelPartsService() {
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
        /// Возвращает коллекцию всех экземпляров семейств исходящих заданий на отверстия из текущего файла инженерных систем
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
            return DocInfos.FirstOrDefault(item => item.Name.Equals(GetDocumentName(element.Document), StringComparison.CurrentCultureIgnoreCase))?.Transform
                ?? Transform.Identity;
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
        public void DeleteElement(int elementId) {
            _document.Delete(new ElementId(elementId));
        }

        /// <summary>
        /// Объединяет задания на отверстия из активного документа и удаляет старые
        /// </summary>
        /// <param name="placer">Класс, размещающий объединенное задание</param>
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
            DeleteElements(openingTasks.Select(task => new ElementId(task.Id)).ToHashSet());
            return createdOpening;
        }

        public void DoAction(Action action) {
            _clashRevitRepository.DoAction(action);
        }

        public IEnumerable<Document> GetDocuments() {
            return _clashRevitRepository.GetDocuments();
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
            return MepCategoryNames.First(pair => pair.Value.Equals(mepCategoryName, StringComparison.CurrentCulture)).Key;
        }

        /// <summary>
        /// Возвращает массив категорий Revit, которые соответствуют заданному <see cref="MepCategoryEnum"/>
        /// </summary>
        /// <param name="mepCategory">Категория элементов инженерных систем</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Исключение, если поданная категория <paramref name="mepCategory"/> не поддерживается</exception>
        public Category[] GetCategories(MepCategoryEnum mepCategory) {
            switch(mepCategory) {
                case MepCategoryEnum.Pipe:
                return new Category[] {
                    Category.GetCategory(_document, BuiltInCategory.OST_PipeCurves),
                    Category.GetCategory(_document, BuiltInCategory.OST_PipeFitting)
                };
                case MepCategoryEnum.RectangleDuct:
                case MepCategoryEnum.RoundDuct:
                return new Category[] {
                    Category.GetCategory(_document, BuiltInCategory.OST_DuctCurves),
                    Category.GetCategory(_document, BuiltInCategory.OST_DuctFitting)
                };
                case MepCategoryEnum.CableTray:
                return new Category[] {
                    Category.GetCategory(_document, BuiltInCategory.OST_CableTray),
                    Category.GetCategory(_document, BuiltInCategory.OST_CableTrayFitting)
                };
                case MepCategoryEnum.Conduit:
                return new Category[] {
                    Category.GetCategory(_document, BuiltInCategory.OST_Conduit),
                    Category.GetCategory(_document, BuiltInCategory.OST_ConduitFitting)
                };
                default:
                throw new NotImplementedException(nameof(mepCategory));
            }
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
        /// Спрашивает у пользователя, нужно ли продолжать операцию, если семейства заданий на отверстия не самой последней версии
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
        /// Возвращает коллекцию чистовых экземпляров семейств отверстий из текущего документа Revit
        /// </summary>
        /// <returns></returns>
        public ICollection<OpeningReal> GetRealOpenings() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedOpeningsCategories())
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(famInst => famInst.Host != null)
                .Where(famInst => famInst.Symbol.FamilyName.Contains("Отв"))
                .Select(famInst => new OpeningReal(famInst))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает коллекцию Id всех элементов конструкций из текущего документа ревита, для которых создаются задания на отверстия
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementId> GetConstructureElementsIds() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(FiltersInitializer.GetFilterByAllUsedStructureCategories())
                .ToElementIds();
        }

        /// <summary>
        /// Возвращает коллекцию Id всех элементов инженерных систем из текущего документа ревита, для которых создаются задания на отверстия
        /// </summary>
        /// <returns></returns>
        public ICollection<ElementId> GetMepElementsIds() {
            return new FilteredElementCollector(_document)
                .WherePasses(FiltersInitializer.GetFilterByAllUsedMepCategories())
                .ToElementIds();
        }

        /// <summary>
        /// Возвращает коллекцию входящих заданий на отверстия из связанных файлов
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
        /// Предлагает пользователю выбрать системную стену или системное перекрытие и возвращает его выбор
        /// </summary>
        /// <returns>Выбранный пользователем элемент классов <see cref="Autodesk.Revit.DB.Wall"/> или <see cref="Autodesk.Revit.DB.Floor"/></returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public Element PickHostForRealOpening() {
            // фильтр по классам, а не по категориям ревита, так как для хоста нужна системная стена или системное перекрытие,
            // при этом необходимо исключить выбор моделей в контексте, которые могут быть стенами и перекрытиями
            ISelectionFilter filter = new SelectionFilterElementsOfClasses(new Type[] { typeof(Wall), typeof(Floor) });
            Reference reference = _uiDocument.Selection.PickObject(ObjectType.Element, filter, "Выберите стену или перекрытие");
            return _document.GetElement(reference);
        }

        /// <summary>
        /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из связанных файлов, подгруженных в активный документ, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранная пользователем коллекция элементов</returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public ICollection<OpeningMepTaskIncoming> PickManyOpeningTasksIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningTasksIncoming(_document);
            IList<Reference> references = _uiDocument.Selection.PickObjects(ObjectType.LinkedElement, filter, "Выберите задание(я) на отверстие(я) из связи(ей) и нажмите \"Готово\"");

            List<OpeningMepTaskIncoming> openingTasks = new List<OpeningMepTaskIncoming>();
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
        /// Предлагает пользователю выбрать экземпляры семейств заданий на отверстия из активного документа и возвращает его выбор
        /// </summary>
        /// <returns>Выбранная пользователем коллекция элементов - заданий на отверстия</returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public ICollection<OpeningMepTaskOutcoming> PickManyOpeningTasksOutcoming() {
            ISelectionFilter filter = new SelectionFilterOpeningTasksOutcoming();
            IList<Reference> references = _uiDocument.Selection.PickObjects(ObjectType.Element, filter, "Выберите задания на отверстия и нажмите \"Готово\"");

            HashSet<OpeningMepTaskOutcoming> openingTasks = new HashSet<OpeningMepTaskOutcoming>();
            foreach(var reference in references) {
                if((reference != null) && (_document.GetElement(reference) is FamilyInstance famInst)) {
                    openingTasks.Add(new OpeningMepTaskOutcoming(famInst));
                }
            }
            return openingTasks;
        }

        /// <summary>
        /// Предлагает пользователю выбрать один экземпляр семейства задания на отверстие из связанных файлов, подгруженных в активный документ, и возвращает его выбор
        /// </summary>
        /// <returns>Выбранный пользователем элемент</returns>
        /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException"/>
        public OpeningMepTaskIncoming PickSingleOpeningTaskIncoming() {
            ISelectionFilter filter = new SelectionFilterOpeningTasksIncoming(_document);
            Reference reference = _uiDocument.Selection.PickObject(ObjectType.LinkedElement, filter, "Выберите задание на отверстие из связи и нажмите \"Готово\"");

            if((reference != null) && (_document.GetElement(reference) is RevitLinkInstance link)) {
                Element opening = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                if((opening != null) && (opening is FamilyInstance famInst)) {
                    return new OpeningMepTaskIncoming(famInst, this, link.GetTransform());
                } else {
                    var dialog = GetMessageBoxService();
                    dialog.Show(
                        $"Выбранный элемент не является экземпляром семейства задания на отверстие",
                        "Задания на отверстия",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error,
                        System.Windows.MessageBoxResult.OK);
                    throw new OperationCanceledException();
                }
            } else {
                var dialog = GetMessageBoxService();
                dialog.Show(
                    $"Не удалось определить выбранный элемент",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            }
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
        /// Возвращает тип проема по названию семейства задания на отверстие
        /// </summary>
        /// <param name="familyName">Название семейства задания на отверстие</param>
        /// <returns></returns>
        public static OpeningType GetOpeningType(string familyName) {
            return OpeningTaskFamilyName.FirstOrDefault(pair => pair.Value.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)).Key;
        }

        /// <summary>
        /// Возвращает коллекцию заголовков файлов Revit связей, которые дублируются.
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
        /// Возвращает класс, размещающий объединенное задание на отверстие
        /// </summary>
        /// <param name="openingTasks">Задания на отверстия из активного документа, которые надо объединить</param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"/>
        private OpeningPlacer GetOpeningPlacer(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            try {
                OpeningPlacer placer;
                if(openingTasks.Any(task => (task.OpeningType == OpeningType.FloorRound) || (task.OpeningType == OpeningType.FloorRectangle))) {
                    placer = new FloorOpeningGroupPlacerInitializer().GetPlacer(this, new OpeningsGroup(openingTasks));
                } else {
                    placer = new WallOpeningGroupPlacerInitializer().GetPlacer(this, new OpeningsGroup(openingTasks));
                }
                return placer;

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
            }
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров семейств-заданий на отверстия от инженера из текущего файла ревит ("исходящие" задания).
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
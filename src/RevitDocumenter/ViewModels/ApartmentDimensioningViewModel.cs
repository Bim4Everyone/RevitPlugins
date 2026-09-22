using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.Dimensions.DimensionReferences;
using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.Models.Mapping.MapServices;
using RevitDocumenter.Models.Mapping.ViewServices;

namespace RevitDocumenter.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
/// <remarks>
/// Логика команды: в активном плане для каждого помещения стены группируются по направлению,
/// внутри направления объединяются в стороны (коллинеарные стены - одна сторона),
/// и между сторонами проставляются размеры "в свету" по их внутренним граням.
/// Помещения непрямоугольной формы обрабатываются штатно: Г-образное помещение получает
/// габарит каждого крыла плюс размеры ступеней.
/// </remarks>
internal class ApartmentDimensioningViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ViewPreparer _viewPreparer;
    private readonly ImageService _imageService;
    private readonly ViewMapService _mapService;
    private readonly PaintSquaresByMapService _paintSquaresByMapService;

    /// <summary>
    /// Имя типоразмера размера, выбираемого по умолчанию.
    /// </summary>
    private readonly string _defSelectedDimensionTypeName = "GOST Common_2.0 мм_0.9";

    /// <summary>
    /// Допуск на параллельность направлений в радианах (1 градус).
    /// </summary>
    private readonly double _angleTolerance = Math.PI / 180;

    /// <summary>
    /// Допуск, в пределах которого параллельные стены считаются лежащими на одной линии.
    /// </summary>
    /// <remarks>
    /// Стены в пределах этого допуска объединяются в одну сторону помещения,
    /// поэтому стена, разрезанная проемом на сегменты, дает один размер, а не несколько одинаковых.
    /// </remarks>
    private readonly double _collinearTolerance = UnitUtilsHelper.ConvertToInternalValue(10);

    /// <summary>
    /// Минимальное расстояние между сторонами помещения.
    /// </summary>
    /// <remarks>Стороны, стоящие ближе, считаются одной конструкцией и не образмериваются.</remarks>
    private readonly double _minDistanceBetweenSides = UnitUtilsHelper.ConvertToInternalValue(100);

    /// <summary>
    /// Минимальная длина перекрытия проекций сторон, при которой размер считается габаритным.
    /// </summary>
    /// <remarks>Ограничение отсекает стороны, которые задевают друг друга только углами.</remarks>
    private readonly double _minOverlapLength = UnitUtilsHelper.ConvertToInternalValue(300);

    /// <summary>
    /// Максимальный зазор между проекциями сторон, при котором размер еще ставится.
    /// </summary>
    /// <remarks>
    /// Стороны без перекрытия проекций образуют ступень (внутренний угол Г-образного помещения).
    /// Размер такой ступени размещается в месте сближения сторон - у общего угла.
    /// </remarks>
    private readonly double _maxGapLength = UnitUtilsHelper.ConvertToInternalValue(300);

    /// <summary>
    /// Отступ линии размера ступени от угла в миллиметрах на бумаге.
    /// </summary>
    /// <remarks>
    /// Размер ступени по построению встает в угол, где стороны сходятся, и его линия ложится
    /// на перпендикулярную стену. Отступ уводит линию от стены вдоль оси направления.
    /// Модельная величина получается умножением на масштаб вида, поэтому на листе
    /// зазор выглядит одинаково при любом масштабе.
    /// </remarks>
    private readonly double _stepOffsetInPaperMm = 2;

    /// <summary>
    /// Высота над низом помещения, на которой проверяется принадлежность точки этому помещению.
    /// </summary>
    private readonly double _roomTestHeight = UnitUtilsHelper.ConvertToInternalValue(300);

    /// <summary>
    /// Отступ линии размера от края перекрытия сторон.
    /// </summary>
    /// <remarks>Задает коридор, в пределах которого линию размера можно двигать при подборе места.</remarks>
    private readonly double _dimensionLineMargin = UnitUtilsHelper.ConvertToInternalValue(100);

    /// <summary>
    /// Шаг бинарной карты вида в миллиметрах на бумаге.
    /// </summary>
    /// <remarks>
    /// Задает гранулярность анализа занятости вида. Модельная величина получается умножением
    /// на масштаб вида, поэтому клетка относительно текста одинакова при любом масштабе.
    /// </remarks>
    private readonly double _mappingStepInPaperMm = 1;

    /// <summary>
    /// Цвет якорных линий, по которым изображение вида сопоставляется с координатами модели.
    /// </summary>
    private readonly Color _colorForAnchorLines = new(255, 0, 255);

    /// <summary>
    /// Вес якорных линий.
    /// </summary>
    private readonly int _weightForAnchorLines = 1;

    /// <summary>
    /// Ширина символа значения размера относительно высоты текста.
    /// </summary>
    /// <remarks>
    /// Оценка с запасом: недооценка ширины дает касания, которые проверка не видит.
    /// Замер на широком шрифте (Century Gothic) дал 0.7-0.85 высоты на цифру.
    /// </remarks>
    private readonly double _textWidthFactor = 0.8;

    /// <summary>
    /// Зазор между подписью размера и занятой графикой в миллиметрах на бумаге.
    /// </summary>
    /// <remarks>
    /// Подпись не должна касаться ни стен, ни подписей других размеров, поэтому проверяемая
    /// зона берется шире текста на этот зазор со всех четырех сторон. Модельная величина
    /// получается умножением на масштаб вида, поэтому на листе зазор одинаков при любом масштабе.
    /// </remarks>
    private readonly double _textClearanceInPaperMm = 2;

    /// <summary>
    /// Запас предела сдвига подписи сверх половины значения размера, в миллиметрах на бумаге.
    /// </summary>
    /// <remarks>
    /// Подпись короткого размера между гранями стен не влезает и обязана выехать за засечки.
    /// Предел не дает ей при этом уехать настолько далеко, что связь с размером теряется.
    /// </remarks>
    private readonly double _maxTextShiftExtraInPaperMm = 10;

    /// <summary>
    /// Количество направлений, по которым проставляются размеры в одном помещении.
    /// </summary>
    /// <remarks>
    /// Берутся направления с наибольшей суммарной длиной стен - для ортогональной планировки
    /// это две взаимно перпендикулярные группы. Косые стены в размеры не попадают.
    /// </remarks>
    private readonly int _directionCount = 2;

    /// <summary>
    /// Имя изображения с картой свободных зон, которое показывается после простановки.
    /// </summary>
    private readonly string _markedImageName = "ApartmentDimensioningMap";

    // Диагностика прогона: почему не построилась карта и сколько размеров куда переехало.
    // Носит временный характер, убирается вместе с показом карты
    private string _mapFailureReason;
    private int _createdCount;
    private int _positionMovedCount;
    private int _textMovedCount;
    // Размеры, подпись которых так и осталась на занятом месте: сдвиг не нашел свободной позиции
    private readonly List<string> _unresolvedDimensions = [];
    // Наибольшее расхождение прогноза положения подписи с фактом, в единицах модели
    private double _maxPredictionError;

    private string _errorText;
    private DimensionType _selectedDimensionType;
    private List<DimensionType> _dimensionTypes = [];

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    /// <param name="viewPreparer">Сервис подготовки вида к экспорту в изображение.</param>
    /// <param name="imageService">Сервис экспорта и обработки изображения вида.</param>
    /// <param name="mapService">Сервис построения и анализа бинарной карты вида.</param>
    /// <param name="paintSquaresByMapService">Сервис отрисовки карты свободных зон на изображении.</param>
    public ApartmentDimensioningViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ViewPreparer viewPreparer,
        ImageService imageService,
        ViewMapService mapService,
        PaintSquaresByMapService paintSquaresByMapService) {

        _pluginConfig = pluginConfig.ThrowIfNull();
        _revitRepository = revitRepository.ThrowIfNull();
        _localizationService = localizationService.ThrowIfNull();
        _viewPreparer = viewPreparer.ThrowIfNull();
        _imageService = imageService.ThrowIfNull();
        _mapService = mapService.ThrowIfNull();
        _paintSquaresByMapService = paintSquaresByMapService.ThrowIfNull();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }


    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Выбранный типоразмер размера.
    /// </summary>
    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set => RaiseAndSetIfChanged(ref _selectedDimensionType, value);
    }

    /// <summary>
    /// Список доступных типоразмеров размеров.
    /// </summary>
    public List<DimensionType> DimensionTypes {
        get => _dimensionTypes;
        set => RaiseAndSetIfChanged(ref _dimensionTypes, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        DimensionTypes = _revitRepository.DimensionTypes;

        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
        using var mainTransaction = new Transaction(
            _revitRepository.Document,
            _localizationService.GetLocalizedString("RebarDimensioningWindow.Title"));
        mainTransaction.Start();

        // Бинарная карта занятости вида. Строится до создания размеров, поэтому видит только то,
        // что было на виде изначально; свои размеры команда дописывает в нее по ходу
        var mapInfo = CreateViewMap();

        CreateDimensions(mapInfo);

        mainTransaction.Commit();

        // Показ карты и итогов прогона - вне транзакции, чтобы не держать ее открытой под диалогом
        ShowMapResult(mapInfo);
    }

    /// <summary>
    /// Показывает карту свободных зон и итоги прогона.
    /// </summary>
    /// <remarks>
    /// Свободные клетки закрашиваются розовым, занятые остаются как есть. Размеры, поставленные
    /// командой, к этому моменту уже отмечены в карте занятыми, поэтому по картинке видно,
    /// что именно алгоритм считал свободным и куда он мог встать.
    /// </remarks>
    private void ShowMapResult(MapInfo mapInfo) {
        string title = "Размеры квартир";

        if(mapInfo is null) {
            // Причина уходит в MainContent, а не в заголовок: она многострочная и с числами,
            // а крупный текст заголовка длинное сообщение обрезает
            var failureDialog = new Autodesk.Revit.UI.TaskDialog(title) {
                MainInstruction = "Карта свободных зон не построена - размеры расставлены только по геометрии.",
                MainContent = "Размеров создано: " + _createdCount
                              + Environment.NewLine + Environment.NewLine
                              + (_mapFailureReason ?? "Причина не определена.")
            };
            failureDialog.Show();
            return;
        }

        try {
            string markedImagePath = _paintSquaresByMapService.MarkWhiteSquaresOnImage(mapInfo, _markedImageName);
            _imageService.OpenImage(markedImagePath);

            string unresolvedText = _unresolvedDimensions.Count == 0
                ? string.Empty
                : Environment.NewLine + Environment.NewLine
                  + "Подпись не удалось развести:"
                  + Environment.NewLine
                  + string.Join(Environment.NewLine, _unresolvedDimensions);

            var resultDialog = new Autodesk.Revit.UI.TaskDialog(title) {
                MainInstruction = "Размеров создано: " + _createdCount,
                MainContent = "Линий сдвинуто с исходной позиции: " + _positionMovedCount
                              + Environment.NewLine
                              + "Сдвинуто переносом подписи: " + _textMovedCount
                              + Environment.NewLine
                              + "Осталось с наложением: " + _unresolvedDimensions.Count
                              + Environment.NewLine
                              + "Макс. расхождение прогноза подписи с фактом вдоль линии: "
                              + Math.Round(UnitUtilsHelper.ConvertFromInternalValue(_maxPredictionError)) + " мм"
                              + Environment.NewLine
                              + GetDimensionTypeDiagnostics()
                              + unresolvedText
            };
            resultDialog.Show();
        } catch(Exception exception) {
            Autodesk.Revit.UI.TaskDialog.Show(title, "Не удалось показать карту: " + exception.Message);
        } finally {
            // Исходное изображение вида больше не нужно - карта уже построена
            _imageService.Delete(mapInfo.ImagePath);
        }
    }

    /// <summary>
    /// Строит бинарную карту занятости активного вида.
    /// </summary>
    /// <remarks>
    /// Карта - это улучшение, а не обязательное условие: при любом сбое возвращается null,
    /// и размеры расставляются по одной геометрии, как раньше.
    /// </remarks>
    /// <returns>Карта вида, либо null, если построить ее не удалось.</returns>
    private MapInfo CreateViewMap() {
        var viewPreparerOption = new ViewPreparerOption() {
            MappingStepInFeet = GetMappingStep(),
            ColorForAnchorLines = _colorForAnchorLines,
            WeightForAnchorLines = _weightForAnchorLines
        };

        ExportOption exportOption = null;
        try {
            // Подготовка вида: краевые точки, кратные шагу, и якорные линии для сопоставления
            // изображения с координатами модели
            exportOption = _viewPreparer.Prepare(viewPreparerOption);

            string imagePath = _imageService.Export(exportOption);
            return _mapService.CreateMap(imagePath, exportOption);
        } catch(Exception exception) {
            _mapFailureReason = exception.Message;
            return null;
        } finally {
            // Якорные линии нужны только на время экспорта
            if(exportOption?.AnchorLineIds?.Count > 0) {
                _revitRepository.DeleteElementsById(exportOption.AnchorLineIds);
            }
        }
    }

    /// <summary>
    /// Возвращает шаг бинарной карты в единицах модели.
    /// </summary>
    private double GetMappingStep() {
        int scale = _revitRepository.Document.ActiveView.Scale;
        return UnitUtilsHelper.ConvertToInternalValue(_mappingStepInPaperMm * Math.Max(scale, 1));
    }

    /// <summary>
    /// Основной метод простановки размеров по помещениям активного вида.
    /// </summary>
    private void CreateDimensions(MapInfo mapInfo) {
        _createdCount = 0;
        _positionMovedCount = 0;
        _textMovedCount = 0;
        _unresolvedDimensions.Clear();
        _maxPredictionError = 0;

        // Стены вида, приведенные к горизонтальным базовым линиям на нулевой отметке
        var wallLines = GetWallLines();
        if(wallLines.Count == 0) {
            return;
        }

        // Сервисы создаются локально: команда ApartmentDimensioningCommand их не регистрирует в контейнере,
        // а полагаться на неявное связывание Ninject не стоит
        var dimensionCreator = new DimensionCreator(_revitRepository);
        var referenceAnalizeService = new ReferenceAnalizeService(_revitRepository, dimensionCreator);
        var existingDimensionRefs = referenceAnalizeService.GetDimensionReferences();

        // Отметка, на которой размещаются линии размеров - линия размера должна лежать в плоскости вида
        double elevation = GetViewElevation();

        var wallPairs = new List<WallPair>();
        foreach(var room in GetRooms()) {
            // Стены, ограничивающие помещение
            var roomWallLines = GetRoomWallLines(room, wallLines);
            if(roomWallLines.Count < 2) {
                continue;
            }

            // Основные направления помещения (для ортогональной планировки - два)
            foreach(var directionGroup in GetMainDirectionGroups(roomWallLines)) {
                // Стороны помещения: коллинеарные стены объединяются в одну сторону
                var wallSides = GetWallSides(directionGroup);
                if(wallSides.Count < 2) {
                    continue;
                }

                // Размер строится для каждой пары сторон - и для габаритных, и для ступеней
                wallPairs.AddRange(GetSidePairs(room, directionGroup, wallSides));
            }
        }

        // Место занимают первыми те, у кого выбора меньше. Сначала размеры, подпись которых
        // не влезает между гранями: ей деваться некуда, кроме выезда за засечку, и выезд
        // должен успеть занять место раньше соседей. Внутри группы - от коротких к длинным:
        // короткому уезжать дальше всех. Затем влезающие - они обтекают то, что уже стоит
        var orderedPairs = wallPairs
            .OrderBy(p => FitsInline(p.Distance))
            .ThenBy(p => p.Distance);

        foreach(var wallPair in orderedPairs) {
            CreateDimension(
                wallPair,
                elevation,
                mapInfo,
                dimensionCreator,
                referenceAnalizeService,
                existingDimensionRefs);
        }
    }


    #region Сбор стен и помещений

    /// <summary>
    /// Возвращает стены активного вида, пригодные для образмеривания, в виде их базовых линий.
    /// </summary>
    /// <returns>Словарь "идентификатор стены - обертка над ее базовой линией".</returns>
    private Dictionary<ElementId, WallLine> GetWallLines() {
        var document = _revitRepository.Document;
        var wallLines = new Dictionary<ElementId, WallLine>();

        var walls = new FilteredElementCollector(document, document.ActiveView.Id)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .OfType<Wall>()
            .Where(IsSuitableWall);

        foreach(var wall in walls) {
            // Стены с непрямой базовой линией (дуги, сплайны) отсеиваются здесь
            var wallLine = WallLine.Create(wall);
            if(wallLine != null) {
                wallLines[wall.Id] = wallLine;
            }
        }
        return wallLines;
    }

    /// <summary>
    /// Проверяет, что стена может участвовать в образмеривании.
    /// </summary>
    /// <remarks>Берутся только базовые стены, ограничивающие помещения (витражи и их панели отсеиваются).</remarks>
    private bool IsSuitableWall(Wall wall) {
        if(wall.WallType is null || wall.WallType.Kind != WallKind.Basic) {
            return false;
        }
        var roomBoundingParam = wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING);
        return roomBoundingParam != null && roomBoundingParam.AsInteger() == 1;
    }

    /// <summary>
    /// Возвращает помещения активного вида.
    /// </summary>
    /// <remarks>Фильтрация по виду сама отбирает помещения нужного уровня и нужной стадии.</remarks>
    private List<Room> GetRooms() {
        var document = _revitRepository.Document;
        return new FilteredElementCollector(document, document.ActiveView.Id)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>()
            .Where(r => r.Area > 0)
            .ToList();
    }

    /// <summary>
    /// Возвращает стены, образующие границу переданного помещения.
    /// </summary>
    /// <param name="room">Помещение.</param>
    /// <param name="wallLines">Все пригодные стены вида.</param>
    private List<WallLine> GetRoomWallLines(Room room, Dictionary<ElementId, WallLine> wallLines) {
        room.ThrowIfNull();
        wallLines.ThrowIfNull();

        var roomWallLines = new List<WallLine>();
        var options = new SpatialElementBoundaryOptions() {
            SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
        };

        var boundaries = room.GetBoundarySegments(options);
        if(boundaries is null) {
            return roomWallLines;
        }

        var processedIds = new HashSet<ElementId>();
        foreach(var boundaryLoop in boundaries) {
            foreach(var segment in boundaryLoop) {
                var elementId = segment.ElementId;
                if(elementId is null || elementId == ElementId.InvalidElementId) {
                    continue;
                }
                // Одна стена может давать несколько сегментов границы
                if(!processedIds.Add(elementId)) {
                    continue;
                }
                if(wallLines.TryGetValue(elementId, out var wallLine)) {
                    roomWallLines.Add(wallLine);
                }
            }
        }
        return roomWallLines;
    }

    #endregion


    #region Направления и стороны помещения

    /// <summary>
    /// Возвращает основные направления помещения.
    /// </summary>
    /// <remarks>
    /// Стены группируются по параллельности, группы сортируются по суммарной длине стен,
    /// и берется <see cref="_directionCount" /> первых. Для ортогонального помещения это дает
    /// два взаимно перпендикулярных направления, косые стены отбрасываются.
    /// </remarks>
    private List<DirectionGroup> GetMainDirectionGroups(List<WallLine> wallLines) {
        wallLines.ThrowIfNull();

        var directionGroups = new List<DirectionGroup>();
        foreach(var wallLine in wallLines) {
            var directionGroup = directionGroups.FirstOrDefault(g => IsParallel(g.Axis, wallLine.Direction));
            if(directionGroup is null) {
                directionGroup = new DirectionGroup(wallLine);
                directionGroups.Add(directionGroup);
            }
            directionGroup.Add(wallLine);
        }

        return [.. directionGroups
            .Where(g => g.WallLines.Count > 1)
            .OrderByDescending(g => g.TotalLength)
            .Take(_directionCount)];
    }

    /// <summary>
    /// Собирает стороны помещения для одного направления.
    /// </summary>
    /// <remarks>
    /// Стена описывается смещением вдоль нормали направления и диапазоном проекции на его ось.
    /// Стены с совпадающим смещением лежат на одной линии и объединяются в одну сторону:
    /// разрезанная проемом стена дает один размер, а не несколько одинаковых.
    /// </remarks>
    private List<WallSide> GetWallSides(DirectionGroup directionGroup) {
        directionGroup.ThrowIfNull();

        var segments = directionGroup.WallLines
            .Select(w => CreateSegment(directionGroup, w))
            .OrderBy(s => s.Offset)
            .ToList();

        var wallSides = new List<WallSide>();
        WallSide currentSide = null;
        foreach(var segment in segments) {
            // Сегменты отсортированы по смещению, поэтому достаточно сравнить с текущей стороной
            if(currentSide is null || Math.Abs(segment.Offset - currentSide.Offset) > _collinearTolerance) {
                currentSide = new WallSide();
                wallSides.Add(currentSide);
            }
            currentSide.Add(segment);
        }
        return wallSides;
    }

    /// <summary>
    /// Раскладывает базовую линию стены в системе координат направления.
    /// </summary>
    private WallSegment CreateSegment(DirectionGroup directionGroup, WallLine wallLine) {
        var fromOrigin = wallLine.Start - directionGroup.Origin;
        double offset = fromOrigin.DotProduct(directionGroup.Normal);

        double startParameter = fromOrigin.DotProduct(directionGroup.Axis);
        double endParameter = (wallLine.End - directionGroup.Origin).DotProduct(directionGroup.Axis);

        return startParameter <= endParameter
            ? new WallSegment(wallLine, offset, startParameter, endParameter)
            : new WallSegment(wallLine, offset, endParameter, startParameter);
    }

    /// <summary>
    /// Проверяет параллельность двух направлений с учетом допуска (направление вдоль оси не важно).
    /// </summary>
    private bool IsParallel(XYZ first, XYZ second) {
        return Math.Abs(first.DotProduct(second)) >= Math.Cos(_angleTolerance);
    }

    #endregion


    #region Поиск пар сторон

    /// <summary>
    /// Возвращает все пары сторон одного направления, пригодные для простановки размера.
    /// </summary>
    /// <remarks>
    /// Ограничения на количество размеров в направлении нет: помещение сложной формы
    /// получает столько размеров, сколько у него сторон в этом направлении.
    /// Заслоненные пары (когда между сторонами стоит третья) не отбрасываются - общий габарит нужен.
    /// </remarks>
    private List<WallPair> GetSidePairs(Room room, DirectionGroup directionGroup, List<WallSide> wallSides) {
        room.ThrowIfNull();
        directionGroup.ThrowIfNull();
        wallSides.ThrowIfNull();

        var wallPairs = new List<WallPair>();
        for(int i = 0; i < wallSides.Count - 1; i++) {
            for(int j = i + 1; j < wallSides.Count; j++) {
                var wallPair = CreateWallPair(room, directionGroup, wallSides[i], wallSides[j]);
                if(wallPair != null) {
                    wallPairs.Add(wallPair);
                }
            }
        }
        return wallPairs;
    }

    /// <summary>
    /// Создает пару сторон, если они находятся на допустимом расстоянии друг от друга
    /// и их проекции либо перекрываются, либо сближаются в пределах допустимого зазора.
    /// </summary>
    /// <returns>Пара сторон, либо null, если стороны не удовлетворяют условиям.</returns>
    private WallPair CreateWallPair(Room room, DirectionGroup directionGroup, WallSide first, WallSide second) {
        // Расстояние между сторонами постоянно - стороны параллельны
        double distance = Math.Abs(first.Offset - second.Offset);
        if(distance < _minDistanceBetweenSides) {
            return null;
        }

        // Ищем участок, на котором стороны сходятся ближе всего.
        // Положительная длина - перекрытие проекций, отрицательная - зазор между ними
        double bestLength = double.MinValue;
        double position = 0;

        // Объединение всех перекрытий: стороны, разрезанные проемами, перекрываются кусками,
        // и линии разрешено вставать и напротив проема - размер идет по граням, а грань
        // продолжается и там, где стены нет
        double unionStart = double.MaxValue;
        double unionEnd = double.MinValue;

        foreach(var firstSegment in first.Segments) {
            foreach(var secondSegment in second.Segments) {
                double start = Math.Max(firstSegment.Start, secondSegment.Start);
                double end = Math.Min(firstSegment.End, secondSegment.End);
                double length = end - start;

                if(length > 0) {
                    unionStart = Math.Min(unionStart, start);
                    unionEnd = Math.Max(unionEnd, end);
                }

                if(length > bestLength) {
                    bestLength = length;
                    // Для перекрытия это его центр, для зазора - середина между проекциями
                    position = (start + end) / 2;
                }
            }
        }

        // Перекрытие не короче минимального - габаритный размер.
        // Перекрытия нет, но стороны сходятся в пределах допустимого зазора - размер ступени.
        // Стороны разнесены вдоль оси - общего участка нет, размер не нужен
        if(bestLength < -_maxGapLength) {
            return null;
        }

        // Размер ступени по построению встает в угол - отодвигаем его от перпендикулярной стены
        bool isStep = bestLength < _minOverlapLength;
        if(isStep) {
            position = GetStepPosition(room, directionGroup, first, second, position);
        }

        // Коридор, в пределах которого линию размера можно двигать при подборе свободного места.
        // У размера ступени коридора нет - он привязан к внутреннему углу
        double rangeStart = position;
        double rangeEnd = position;
        if(!isStep && unionEnd - unionStart > 2 * _dimensionLineMargin) {
            rangeStart = unionStart + _dimensionLineMargin;
            rangeEnd = unionEnd - _dimensionLineMargin;
        }

        return new WallPair(room, directionGroup, first, second, position, distance, isStep, rangeStart, rangeEnd);
    }

    #endregion


    #region Отступ размера ступени

    /// <summary>
    /// Отодвигает линию размера ступени от угла, в котором сходятся стороны.
    /// </summary>
    /// <param name="room">Помещение, которому принадлежат стороны.</param>
    /// <param name="directionGroup">Направление, которому принадлежит пара.</param>
    /// <param name="first">Первая сторона пары.</param>
    /// <param name="second">Вторая сторона пары.</param>
    /// <param name="cornerPosition">Позиция угла на оси направления.</param>
    /// <returns>Позиция линии размера с отступом, либо позиция угла, если отодвинуть некуда.</returns>
    private double GetStepPosition(
        Room room,
        DirectionGroup directionGroup,
        WallSide first,
        WallSide second,
        double cornerPosition) {

        double stepOffset = GetStepOffset();
        if(stepOffset <= 0) {
            return cornerPosition;
        }

        // Направления вдоль оси не равноценны: с одной стороны от угла полоса между сторонами
        // лежит внутри помещения, с другой - уже снаружи. Какое именно направление верное,
        // зависит от планировки, поэтому спрашиваем у помещения
        bool isForwardInRoom = IsPositionInRoom(room, directionGroup, first, second, cornerPosition + stepOffset);
        bool isBackwardInRoom = IsPositionInRoom(room, directionGroup, first, second, cornerPosition - stepOffset);

        int direction;
        if(isForwardInRoom && isBackwardInRoom) {
            // Помещение обходит угол с двух сторон (пилон, выступ) - уходим туда, где участок длиннее
            direction = GetLongerSideDirection(first, second, cornerPosition);
        } else if(isForwardInRoom) {
            direction = 1;
        } else if(isBackwardInRoom) {
            direction = -1;
        } else {
            // Ни одно направление не остается внутри помещения - оставляем размер в углу
            return cornerPosition;
        }

        // Если участок короче двух отступов, садимся в его середину - дальше всего от обоих концов
        double availableLength = GetAvailableLength(first, second, cornerPosition, direction);
        stepOffset = Math.Min(stepOffset, availableLength / 2);

        return cornerPosition + direction * stepOffset;
    }

    /// <summary>
    /// Возвращает отступ линии размера ступени в единицах модели.
    /// </summary>
    /// <remarks>Отступ задан в миллиметрах на бумаге, поэтому пересчитывается через масштаб вида.</remarks>
    private double GetStepOffset() {
        int scale = _revitRepository.Document.ActiveView.Scale;
        return scale <= 0 ? 0 : UnitUtilsHelper.ConvertToInternalValue(_stepOffsetInPaperMm * scale);
    }

    /// <summary>
    /// Проверяет, что линия размера в переданной позиции остается внутри помещения.
    /// </summary>
    /// <remarks>
    /// Концы линии лежат на базовых линиях стен, то есть внутри их тела, и проверка концов
    /// всегда давала бы "снаружи". Поэтому проверяется середина линии.
    /// </remarks>
    private bool IsPositionInRoom(
        Room room,
        DirectionGroup directionGroup,
        WallSide first,
        WallSide second,
        double position) {

        var middlePoint = directionGroup.Origin
                          + directionGroup.Axis * position
                          + directionGroup.Normal * ((first.Offset + second.Offset) / 2);

        return room.IsPointInRoom(new XYZ(middlePoint.X, middlePoint.Y, GetRoomTestElevation(room)));
    }

    /// <summary>
    /// Возвращает отметку, на которой проверяется принадлежность точки помещению.
    /// </summary>
    /// <remarks>Берется низ помещения с небольшим подъемом, чтобы точка попала в его объем.</remarks>
    private double GetRoomTestElevation(Room room) {
        double levelElevation = room.Level?.Elevation ?? 0;
        double lowerOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET)?.AsDouble() ?? 0;
        return levelElevation + lowerOffset + _roomTestHeight;
    }

    /// <summary>
    /// Возвращает направление вдоль оси в сторону более длинного участка, примыкающего к углу.
    /// </summary>
    private int GetLongerSideDirection(WallSide first, WallSide second, double cornerPosition) {
        double forwardLength = GetAvailableLength(first, second, cornerPosition, 1);
        double backwardLength = GetAvailableLength(first, second, cornerPosition, -1);
        return forwardLength >= backwardLength ? 1 : -1;
    }

    /// <summary>
    /// Возвращает длину участка стороны, уходящего от угла в переданном направлении.
    /// </summary>
    /// <remarks>
    /// Участок должен начинаться у самого угла: стороны ступени сходятся в нем встык,
    /// поэтому допуск на примыкание берется тот же, что и на зазор между их проекциями.
    /// </remarks>
    private double GetAvailableLength(WallSide first, WallSide second, double cornerPosition, int direction) {
        double availableLength = 0;

        foreach(var segment in first.Segments.Concat(second.Segments)) {
            double nearEdge = direction > 0 ? segment.Start : segment.End;
            double farEdge = direction > 0 ? segment.End : segment.Start;

            if(Math.Abs(nearEdge - cornerPosition) > _maxGapLength) {
                continue;
            }
            double length = (farEdge - cornerPosition) * direction;
            if(length > availableLength) {
                availableLength = length;
            }
        }
        return availableLength;
    }

    #endregion


    #region Подбор места по бинарной карте

    /// <summary>
    /// Подбирает позицию размерной линии и сдвиг подписи вдоль нее так, чтобы прогнозная
    /// зона подписи оказалась свободной.
    /// </summary>
    /// <remarks>
    /// Варианты перебираются от лучшего к худшему. Линия сначала стоит в исходной позиции,
    /// а подпись сдвигается вдоль нее от середины наружу; следующая позиция линии пробуется
    /// только тогда, когда при текущей места для подписи нет. Так линия уходит с исходной
    /// позиции лишь в безвыходных случаях, и размеры не расползаются лесенкой.
    /// Подпись, которая влезает между гранями, не покидает свое помещение.
    /// </remarks>
    /// <param name="linePosition">Найденная позиция линии на оси направления.</param>
    /// <param name="textShift">Найденный сдвиг подписи вдоль линии от ее середины.</param>
    /// <returns>true, если свободный вариант найден.</returns>
    private bool TryFindPlacement(WallPair wallPair, MapInfo mapInfo, out double linePosition, out double textShift) {
        wallPair.ThrowIfNull();
        mapInfo.ThrowIfNull();

        linePosition = wallPair.Position;
        textShift = 0;

        double step = mapInfo.MappingStepInFeet;
        if(step <= 0) {
            return false;
        }

        // До создания размера значение неизвестно - берется расстояние между осями сторон
        double dimensionValue = wallPair.Distance;
        bool mustStayInRoom = FitsInline(dimensionValue);
        double shiftLimit = GetMaxTextShift(dimensionValue);
        var alongLine = wallPair.DirectionGroup.Normal;

        foreach(double candidateLine in GetLinePositions(wallPair, step)) {
            // Коридор включает участки напротив проемов - сдвинутая линия не должна при этом
            // выйти из помещения. Исходную позицию не проверяем: она в центре перекрытия
            if(Math.Abs(candidateLine - wallPair.Position) > 1e-9
               && !IsPositionInRoom(wallPair.Room, wallPair.DirectionGroup, wallPair.First, wallPair.Second, candidateLine)) {
                continue;
            }

            var textCenter = GetPredictedTextCenter(wallPair, candidateLine);

            foreach(double candidateShift in GetTextShifts(shiftLimit, step)) {
                var shiftedCenter = textCenter + alongLine * candidateShift;
                if(mustStayInRoom && !IsPointInRoom(wallPair.Room, shiftedCenter)) {
                    continue;
                }

                (var firstCorner, var secondCorner) =
                    GetTextRect(wallPair.DirectionGroup, shiftedCenter, dimensionValue);
                if(!_mapService.CheckInRectangle(mapInfo, firstCorner, secondCorner)) {
                    continue;
                }

                linePosition = candidateLine;
                textShift = candidateShift;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Возвращает позиции размерной линии на оси направления в порядке предпочтения.
    /// </summary>
    /// <remarks>
    /// Сначала исходная позиция, затем попеременно в обе стороны с шагом карты в пределах
    /// коридора. У размера ступени коридора нет - он привязан к внутреннему углу.
    /// </remarks>
    private IEnumerable<double> GetLinePositions(WallPair wallPair, double step) {
        double center = wallPair.Position;
        yield return center;

        if(wallPair.IsStep) {
            yield break;
        }

        for(int stepIndex = 1; ; stepIndex++) {
            double forward = center + stepIndex * step;
            double backward = center - stepIndex * step;
            bool isAnyInside = false;

            if(forward <= wallPair.RangeEnd) {
                isAnyInside = true;
                yield return forward;
            }
            if(backward >= wallPair.RangeStart) {
                isAnyInside = true;
                yield return backward;
            }
            if(!isAnyInside) {
                yield break;
            }
        }
    }

    /// <summary>
    /// Возвращает сдвиги подписи вдоль размерной линии в порядке предпочтения.
    /// </summary>
    /// <remarks>Сначала без сдвига, затем попеременно в обе стороны с шагом карты до предела.</remarks>
    private static IEnumerable<double> GetTextShifts(double shiftLimit, double step) {
        yield return 0;
        for(double shift = step; shift <= shiftLimit; shift += step) {
            yield return shift;
            yield return -shift;
        }
    }

    /// <summary>
    /// Ставит подпись созданного размера на запланированный сдвиг от середины линии.
    /// </summary>
    /// <remarks>
    /// Меняется только координата вдоль линии. Положение поперек линии остается тем,
    /// что выбрал Revit: сдвиг поперек он оформил бы Z-образной выноской.
    /// </remarks>
    private void ApplyPlannedTextShift(Dimension dimension, WallPair wallPair, double textShift) {
        dimension.ThrowIfNull();
        wallPair.ThrowIfNull();

        var alongLine = wallPair.DirectionGroup.Normal;
        var predictedCenter = GetPredictedTextCenter(wallPair, wallPair.Position);
        var actualPosition = dimension.TextPosition;

        double plannedAlong = predictedCenter.DotProduct(alongLine) + textShift;
        double actualAlong = actualPosition.DotProduct(alongLine);

        dimension.TextPosition = actualPosition + alongLine * (plannedAlong - actualAlong);
    }

    /// <summary>
    /// Двигает подпись готового размера, если ее фактическая зона занята.
    /// </summary>
    /// <remarks>
    /// Подпись едет только вдоль размерной линии: сдвиг поперек Revit оформляет Z-образной
    /// выноской, а она недопустима. Выезд за засечки разрешен - подпись короткого размера
    /// между гранями стен физически не влезает, ей приходится выходить наружу.
    /// Позиции перебираются от фактического положения подписи наружу, поэтому первая
    /// свободная и есть ближайшая: подпись уезжает ровно настолько, насколько пришлось.
    /// Сама размерная линия не трогается - она обязана идти между гранями стен.
    /// </remarks>
    /// <returns>true, если подпись сдвинута.</returns>
    private bool TryMoveDimensionText(Dimension dimension, WallPair wallPair, MapInfo mapInfo) {
        dimension.ThrowIfNull();
        wallPair.ThrowIfNull();
        mapInfo.ThrowIfNull();

        (var firstCorner, var secondCorner) = GetActualTextRect(dimension, wallPair);
        if(_mapService.CheckInRectangle(mapInfo, firstCorner, secondCorner)) {
            return false;
        }

        double step = mapInfo.MappingStepInFeet;
        if(step <= 0) {
            return false;
        }

        // Зона строится от центра текста, а двигается точка TextPosition - на тот же вектор
        var textPosition = dimension.TextPosition;
        var textCenter = GetActualTextCenter(dimension, wallPair);
        double dimensionValue = GetDimensionValue(dimension, wallPair);
        double shiftLimit = GetMaxTextShift(dimensionValue);
        var shiftDirection = wallPair.DirectionGroup.Normal;

        // Подпись, которая влезает между гранями, покидать свое помещение не должна: снаружи
        // квартиры карта почти пустая, и без этого ограничения подпись уезжала бы туда первой.
        // Подписи, которая не влезает, выезд за засечку необходим - ее не ограничиваем
        bool mustStayInRoom = FitsInline(dimensionValue);

        for(double shiftLength = step; shiftLength <= shiftLimit; shiftLength += step) {
            foreach(int directionFactor in new[] { 1, -1 }) {
                var shift = shiftDirection * (directionFactor * shiftLength);
                var shiftedCenter = textCenter + shift;
                if(mustStayInRoom && !IsPointInRoom(wallPair.Room, shiftedCenter)) {
                    continue;
                }

                (var shiftedFirst, var shiftedSecond) =
                    GetTextRect(wallPair.DirectionGroup, shiftedCenter, dimensionValue);

                if(!_mapService.CheckInRectangle(mapInfo, shiftedFirst, shiftedSecond)) {
                    continue;
                }

                dimension.TextPosition = textPosition + shift;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Проверяет, помещается ли подпись между гранями стен вместе с зазорами.
    /// </summary>
    /// <param name="dimensionValue">Значение размера в единицах модели.</param>
    private bool FitsInline(double dimensionValue) {
        return GetTextWidth(dimensionValue) + 2 * GetTextClearance() <= dimensionValue;
    }

    /// <summary>
    /// Проверяет, что точка в плане лежит внутри помещения.
    /// </summary>
    /// <remarks>Высота точки подменяется на отметку внутри объема помещения.</remarks>
    private bool IsPointInRoom(Room room, XYZ point) {
        if(room is null || point is null) {
            return false;
        }
        return room.IsPointInRoom(new XYZ(point.X, point.Y, GetRoomTestElevation(room)));
    }

    /// <summary>
    /// Запоминает размер, подпись которого не удалось развести, для итогового отчета.
    /// </summary>
    private void RegisterUnresolved(Dimension dimension, WallPair wallPair) {
        double valueInMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(
            GetDimensionValue(dimension, wallPair)));
        string roomName = wallPair.Room?.Name ?? "?";
        _unresolvedDimensions.Add(valueInMm + " мм, помещение \"" + roomName + "\", Id " + dimension.Id);
    }

    /// <summary>
    /// Возвращает фактическую зону подписи созданного размера.
    /// </summary>
    /// <param name="withClearance">Расширять ли зону на зазор - для проверки да, для закраски нет.</param>
    private (XYZ, XYZ) GetActualTextRect(Dimension dimension, WallPair wallPair, bool withClearance = true) {
        dimension.ThrowIfNull();
        wallPair.ThrowIfNull();

        return GetTextRect(
            wallPair.DirectionGroup,
            GetActualTextCenter(dimension, wallPair),
            GetDimensionValue(dimension, wallPair),
            withClearance);
    }

    /// <summary>
    /// Возвращает фактический центр подписи созданного размера.
    /// </summary>
    /// <remarks>
    /// Точка TextPosition - не центр текста, а опорная точка ближе к размерной линии.
    /// Поэтому координата вдоль линии берется из нее: вдоль линии подпись и двигается,
    /// и Revit это честно отражает. А поперек линии подпись стоит на постоянном расстоянии,
    /// заданном типоразмером, и никогда не двигается - там центр берется по той же формуле,
    /// что и прогноз.
    /// </remarks>
    private XYZ GetActualTextCenter(Dimension dimension, WallPair wallPair) {
        dimension.ThrowIfNull();
        wallPair.ThrowIfNull();

        var alongLine = wallPair.DirectionGroup.Normal;
        var predictedCenter = GetPredictedTextCenter(wallPair, wallPair.Position);
        double alongDifference = (dimension.TextPosition - predictedCenter).DotProduct(alongLine);

        return predictedCenter + alongLine * alongDifference;
    }

    /// <summary>
    /// Возвращает предполагаемую зону подписи размера, который еще не создан.
    /// </summary>
    /// <remarks>
    /// До создания фактическое положение подписи неизвестно, поэтому оно прогнозируется
    /// по тому же правилу, по которому Revit ставит влезающую подпись: над серединой линии.
    /// Оценка нужна для подбора позиции самой линии, окончательная проверка идет уже
    /// по фактическому положению подписи готового размера.
    /// </remarks>
    private (XYZ, XYZ) GetPredictedTextRect(WallPair wallPair, double position) {
        wallPair.ThrowIfNull();

        return GetTextRect(wallPair.DirectionGroup, GetPredictedTextCenter(wallPair, position), wallPair.Distance);
    }

    /// <summary>
    /// Прогнозирует центр подписи размера, который еще не создан.
    /// </summary>
    /// <remarks>
    /// Revit ставит подпись не на размерную линию, а над ней: низ текста отстоит от линии
    /// на отступ из типоразмера, значит центр - на отступ плюс половину высоты текста.
    /// "Над" определяется правилом чтения: текст читается слева направо, а вертикальный -
    /// снизу вверх, поэтому у горизонтального размера подпись сверху, у вертикального - слева.
    /// </remarks>
    private XYZ GetPredictedTextCenter(WallPair wallPair, double position) {
        wallPair.ThrowIfNull();

        var directionGroup = wallPair.DirectionGroup;
        var middlePoint = directionGroup.Origin
                          + directionGroup.Axis * position
                          + directionGroup.Normal * ((wallPair.First.Offset + wallPair.Second.Offset) / 2);

        var view = _revitRepository.Document.ActiveView;

        // Направление чтения: вдоль линии размера, но так, чтобы текст не читался задом наперед
        var readingDirection = directionGroup.Normal;
        double alongRight = readingDirection.DotProduct(view.RightDirection);
        bool isVerticalInView = Math.Abs(alongRight) < Math.Sin(_angleTolerance);
        if(isVerticalInView
               ? readingDirection.DotProduct(view.UpDirection) < 0
               : alongRight < 0) {
            readingDirection = readingDirection.Negate();
        }

        // Поворот направления чтения на 90 градусов в плоскости вида - сторона над текстом
        var textUp = view.ViewDirection.CrossProduct(readingDirection).Normalize();

        return middlePoint + textUp * (GetTextHeight() / 2 + GetTextOffset());
    }

    /// <summary>
    /// Возвращает сводку по параметрам текста выбранного типоразмера для итогового отчета.
    /// </summary>
    /// <remarks>
    /// Диагностика носит временный характер: проверяет, что высота и отступ текста, от которых
    /// считается положение подписи поперек линии, читаются из типоразмера верно.
    /// </remarks>
    private string GetDimensionTypeDiagnostics() {
        return "Типоразмер \"" + SelectedDimensionType.Name + "\" (на бумаге): высота текста "
               + FormatPaperMm(SelectedDimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE))
               + ", отступ текста "
               + FormatPaperMm(SelectedDimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE));
    }

    private static string FormatPaperMm(Parameter parameter) {
        return parameter is null
            ? "параметр не найден"
            : Math.Round(UnitUtilsHelper.ConvertFromInternalValue(parameter.AsDouble()), 2) + " мм";
    }

    /// <summary>
    /// Возвращает отступ подписи от размерной линии в единицах модели.
    /// </summary>
    /// <remarks>Отступ задан в типоразмере на бумаге, поэтому пересчитывается через масштаб вида.</remarks>
    private double GetTextOffset() {
        int scale = _revitRepository.Document.ActiveView.Scale;
        double offset = SelectedDimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE)?.AsDouble() ?? 0;
        return offset * Math.Max(scale, 1);
    }

    /// <summary>
    /// Возвращает два противоположных угла зоны, занимаемой подписью размера.
    /// </summary>
    /// <param name="directionGroup">Направление, которому принадлежит размер.</param>
    /// <param name="textPosition">Положение подписи - центр текста.</param>
    /// <param name="dimensionValue">Значение размера, от которого зависит ширина подписи.</param>
    /// <remarks>
    /// Подпись читается вдоль размерной линии, поэтому ширина откладывается по нормали
    /// направления, а высота - по его оси. Зона расширена на зазор со всех четырех сторон:
    /// подпись не должна касаться ни стен, ни подписей других размеров.
    /// </remarks>
    private (XYZ, XYZ) GetTextRect(
        DirectionGroup directionGroup,
        XYZ textPosition,
        double dimensionValue,
        bool withClearance = true) {

        directionGroup.ThrowIfNull();
        textPosition.ThrowIfNull();

        double clearance = withClearance ? GetTextClearance() : 0;
        var halfWidth = directionGroup.Normal * (GetTextWidth(dimensionValue) / 2 + clearance);
        var halfHeight = directionGroup.Axis * (GetTextHeight() / 2 + clearance);

        return (textPosition - halfWidth - halfHeight, textPosition + halfWidth + halfHeight);
    }

    /// <summary>
    /// Возвращает значение размера, а при его отсутствии - расстояние между осями сторон.
    /// </summary>
    /// <remarks>
    /// Значение отличается от расстояния между осями на половины толщин стен, поэтому
    /// для ширины подписи и для предела сдвига берется именно оно.
    /// </remarks>
    private double GetDimensionValue(Dimension dimension, WallPair wallPair) {
        double value = dimension.Value ?? 0;
        return value > 0 ? value : wallPair.Distance;
    }

    /// <summary>
    /// Возвращает зазор вокруг подписи размера в единицах модели.
    /// </summary>
    /// <remarks>Зазор задан в миллиметрах на бумаге, поэтому пересчитывается через масштаб вида.</remarks>
    private double GetTextClearance() {
        int scale = _revitRepository.Document.ActiveView.Scale;
        return UnitUtilsHelper.ConvertToInternalValue(_textClearanceInPaperMm * Math.Max(scale, 1));
    }

    /// <summary>
    /// Возвращает предел сдвига подписи вдоль размерной линии в единицах модели.
    /// </summary>
    /// <remarks>
    /// Половина значения размера плюс запас на бумаге: подпись может выйти за засечку,
    /// но не может уехать от своего размера на непрочитываемое расстояние.
    /// </remarks>
    private double GetMaxTextShift(double dimensionValue) {
        int scale = _revitRepository.Document.ActiveView.Scale;
        return dimensionValue / 2
               + UnitUtilsHelper.ConvertToInternalValue(_maxTextShiftExtraInPaperMm * Math.Max(scale, 1));
    }

    /// <summary>
    /// Возвращает оценку ширины подписи размера в единицах модели.
    /// </summary>
    /// <remarks>Ширина считается по количеству цифр значения, округленного до миллиметров.</remarks>
    private double GetTextWidth(double dimensionValue) {
        double valueInMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(dimensionValue));
        int digitCount = valueInMm < 10 ? 1 : (int) Math.Floor(Math.Log10(valueInMm)) + 1;
        return digitCount * GetTextHeight() * _textWidthFactor;
    }

    /// <summary>
    /// Возвращает высоту текста размера в единицах модели.
    /// </summary>
    private double GetTextHeight() {
        int scale = _revitRepository.Document.ActiveView.Scale;
        double textSize = SelectedDimensionType.GetParamValue<double>(BuiltInParameter.TEXT_SIZE);
        return textSize * Math.Max(scale, 1);
    }

    #endregion


    #region Создание размера

    /// <summary>
    /// Создает размер между внутренними гранями пары сторон.
    /// </summary>
    /// <param name="wallPair">Пара сторон.</param>
    /// <param name="elevation">Отметка размещения линии размера.</param>
    /// <param name="dimensionCreator">Сервис создания размеров.</param>
    /// <param name="referenceAnalizeService">Сервис анализа опорных плоскостей.</param>
    /// <param name="existingDimensionRefs">Опорные плоскости уже существующих на виде размеров.</param>
    private void CreateDimension(
        WallPair wallPair,
        double elevation,
        MapInfo mapInfo,
        DimensionCreator dimensionCreator,
        ReferenceAnalizeService referenceAnalizeService,
        List<ReferenceArray> existingDimensionRefs) {

        // Этап 1. Совместный подбор по прогнозу до создания размера: позиция линии и сдвиг
        // подписи вдоль нее, при которых прогнозная зона подписи свободна
        double plannedTextShift = 0;
        if(mapInfo != null
           && TryFindPlacement(wallPair, mapInfo, out double plannedLinePosition, out plannedTextShift)
           && Math.Abs(plannedLinePosition - wallPair.Position) > 1e-9) {
            wallPair.Position = plannedLinePosition;
            _positionMovedCount++;
        }

        // Стены выбираются по итоговой позиции размера
        var firstWallLine = wallPair.First.GetNearestWallLine(wallPair.Position);
        var secondWallLine = wallPair.Second.GetNearestWallLine(wallPair.Position);
        if(firstWallLine is null || secondWallLine is null) {
            return;
        }

        // Опорные плоскости - обращенные друг к другу грани стен
        var references = GetFacingReferences(firstWallLine.Wall, secondWallLine.Wall);
        if(references is null) {
            return;
        }

        // Если размер между этими же гранями уже стоит - повторно не создаем
        if(existingDimensionRefs.Count > 0
           && referenceAnalizeService.IsReferenceArrayInList(references, existingDimensionRefs)) {
            return;
        }

        var dimensionLine = GetDimensionLine(wallPair, elevation);
        if(dimensionLine is null) {
            return;
        }

        Dimension dimension;
        try {
            dimension = dimensionCreator.Create(dimensionLine, references, SelectedDimensionType);
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            // Revit отказался строить размер по этой паре граней (вырожденный случай на стыке сторон).
            // Пропускаем пару, чтобы не прерывать обработку остальных помещений
            return;
        }
        if(dimension is null) {
            return;
        }
        existingDimensionRefs.Add(references);
        _createdCount++;

        // Диагностика: насколько прогноз подписи разошелся с фактом. Считается только для
        // влезающих подписей - невлезающую Revit выносит за засечку, и прогноз к ней не относится.
        // Сравнение в плане: прогноз строится на нулевой отметке, а размер на отметке вида
        if(FitsInline(GetDimensionValue(dimension, wallPair))) {
            // Поперек линии центр теперь берется по формуле, поэтому ошибка меряется только вдоль
            var predictedCenter = GetPredictedTextCenter(wallPair, wallPair.Position);
            double predictionError = Math.Abs(
                (dimension.TextPosition - predictedCenter).DotProduct(wallPair.DirectionGroup.Normal));
            _maxPredictionError = Math.Max(_maxPredictionError, predictionError);
        }

        if(mapInfo != null) {
            // Этап 2. Подпись встает туда, куда ее запланировал подбор
            bool isTextMoved = false;
            if(Math.Abs(plannedTextShift) > 1e-9) {
                ApplyPlannedTextShift(dimension, wallPair, plannedTextShift);
                isTextMoved = true;
            }

            // Этап 3. Прогноз мог разойтись с фактом - тогда досдвигаем подпись вдоль линии
            // уже от ее фактического положения. Сама линия остается на месте
            if(TryMoveDimensionText(dimension, wallPair, mapInfo)) {
                isTextMoved = true;
            }
            if(isTextMoved) {
                _textMovedCount++;
            }

            // Контроль по факту: подпись осталась на занятом месте - сдвиг сдался.
            // Проверка до закраски, иначе размер нашел бы занятой собственную зону
            (var textFirst, var textSecond) = GetActualTextRect(dimension, wallPair);
            if(!_mapService.CheckInRectangle(mapInfo, textFirst, textSecond)) {
                RegisterUnresolved(dimension, wallPair);
            }

            // Занятые клетки закрашиваем по факту, чтобы следующие размеры видели этот
            PaintDimension(dimension, wallPair, mapInfo);
        }
    }

    /// <summary>
    /// Отмечает в карте место, занятое поставленным размером.
    /// </summary>
    /// <remarks>
    /// Закрашивается фактическая зона подписи - там, где она в итоге встала, а не середина
    /// размерной линии: иначе занятым помечалось бы место, с которого подпись уже уехала.
    /// Зона закрашивается без зазора: зазор добавляется при проверке следующей подписи,
    /// и если закрасить его еще и здесь, между подписями получится двойной зазор.
    /// Размерная линия не закрашивается: пересечение подписи чужой линией допустимо.
    /// </remarks>
    private void PaintDimension(Dimension dimension, WallPair wallPair, MapInfo mapInfo) {
        dimension.ThrowIfNull();
        wallPair.ThrowIfNull();
        mapInfo.ThrowIfNull();

        (var firstCorner, var secondCorner) = GetActualTextRect(dimension, wallPair, withClearance: false);
        _mapService.PaintInRectangle(mapInfo, firstCorner, secondCorner);
    }

    /// <summary>
    /// Подбирает пару граней стен для простановки размера.
    /// </summary>
    /// <remarks>
    /// Грани должны быть параллельны, разнонаправлены (нормали смотрят навстречу друг другу)
    /// и находиться на минимальном расстоянии - это дает размер "в свету".
    /// </remarks>
    /// <returns>Массив из двух опорных плоскостей, либо null, если подходящая пара не найдена.</returns>
    private ReferenceArray GetFacingReferences(Wall firstWall, Wall secondWall) {
        firstWall.ThrowIfNull();
        secondWall.ThrowIfNull();

        var firstFaces = GetSideFaces(firstWall);
        var secondFaces = GetSideFaces(secondWall);
        if(firstFaces.Count == 0 || secondFaces.Count == 0) {
            return null;
        }

        Reference resultFirstRef = null;
        Reference resultSecondRef = null;
        double minDistance = double.MaxValue;

        foreach(var firstFace in firstFaces) {
            foreach(var secondFace in secondFaces) {
                var firstNormal = firstFace.Face.FaceNormal;
                var secondNormal = secondFace.Face.FaceNormal;

                // Нормали параллельны и разнонаправлены
                if(firstNormal.DotProduct(secondNormal) > -Math.Cos(_angleTolerance)) {
                    continue;
                }

                var betweenFaces = secondFace.Face.Origin - firstFace.Face.Origin;
                double signedDistance = betweenFaces.DotProduct(firstNormal);

                // Грани должны смотреть друг на друга, а не в разные стороны
                if(signedDistance <= 0) {
                    continue;
                }
                if(signedDistance < minDistance) {
                    minDistance = signedDistance;
                    resultFirstRef = firstFace.Reference;
                    resultSecondRef = secondFace.Reference;
                }
            }
        }

        if(resultFirstRef is null || resultSecondRef is null) {
            return null;
        }

        var referenceArray = new ReferenceArray();
        referenceArray.Append(resultFirstRef);
        referenceArray.Append(resultSecondRef);
        return referenceArray;
    }

    /// <summary>
    /// Возвращает боковые плоские грани стены вместе с их опорными плоскостями.
    /// </summary>
    /// <remarks>
    /// Берутся крайние грани стены (с учетом отделочных слоев) - именно они образуют размер "в свету".
    /// Стены со сложным профилем, у которых боковая грань не является плоскостью, отсеиваются.
    /// </remarks>
    private List<WallFace> GetSideFaces(Wall wall) {
        wall.ThrowIfNull();

        var wallFaces = new List<WallFace>();
        var shellLayerTypes = new[] { ShellLayerType.Exterior, ShellLayerType.Interior };

        foreach(var shellLayerType in shellLayerTypes) {
            foreach(var reference in HostObjectUtils.GetSideFaces(wall, shellLayerType)) {
                if(wall.GetGeometryObjectFromReference(reference) is not PlanarFace planarFace) {
                    continue;
                }
                // Боковая грань вертикальной стены имеет горизонтальную нормаль
                if(Math.Abs(planarFace.FaceNormal.DotProduct(XYZ.BasisZ)) > Math.Sin(_angleTolerance)) {
                    continue;
                }
                wallFaces.Add(new WallFace(reference, planarFace));
            }
        }
        return wallFaces;
    }

    /// <summary>
    /// Возвращает линию размещения размера.
    /// </summary>
    /// <remarks>
    /// Линия строится перпендикулярно сторонам в найденной позиции вдоль оси направления:
    /// для габаритного размера это центр перекрытия проекций, для размера ступени - место
    /// сближения сторон у общего угла.
    /// </remarks>
    private Line GetDimensionLine(WallPair wallPair, double elevation) {
        wallPair.ThrowIfNull();

        var directionGroup = wallPair.DirectionGroup;
        var alongAxis = directionGroup.Origin + directionGroup.Axis * wallPair.Position;

        var firstPoint = alongAxis + directionGroup.Normal * wallPair.First.Offset;
        var secondPoint = alongAxis + directionGroup.Normal * wallPair.Second.Offset;

        var start = new XYZ(firstPoint.X, firstPoint.Y, elevation);
        var end = new XYZ(secondPoint.X, secondPoint.Y, elevation);

        return start.DistanceTo(end) < _revitRepository.Document.Application.ShortCurveTolerance
            ? null
            : Line.CreateBound(start, end);
    }

    /// <summary>
    /// Возвращает отметку, на которой размещаются линии размеров.
    /// </summary>
    /// <remarks>Линия размера должна лежать в плоскости вида, поэтому берется отметка уровня вида.</remarks>
    private double GetViewElevation() {
        var view = _revitRepository.Document.ActiveView;
        return view is ViewPlan viewPlan && viewPlan.GenLevel != null
            ? viewPlan.GenLevel.Elevation
            : view.Origin.Z;
    }

    #endregion


    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(SelectedDimensionType is null) {
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.DimensionTypeIsNotSelected");
            return false;
        }
        if(_revitRepository.Document.ActiveView is not ViewPlan) {
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.ViewNotViewPlan");
            return false;
        }
        ErrorText = string.Empty;
        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SelectedDimensionType = DimensionTypes.FirstOrDefault(d =>
            d.Name.Equals(setting?.SelectedDimensionTypeName ?? _defSelectedDimensionTypeName))
            ?? DimensionTypes.FirstOrDefault();
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SelectedDimensionTypeName = SelectedDimensionType.Name;

        _pluginConfig.SaveProjectConfig();
    }


    /// <summary>
    /// Обертка над стеной и ее базовой линией, спроецированной на горизонтальную плоскость на нуле.
    /// </summary>
    private class WallLine {
        private WallLine(Wall wall, Line baseLine) {
            Wall = wall;
            BaseLine = baseLine;
            Start = baseLine.GetEndPoint(0);
            End = baseLine.GetEndPoint(1);
            Direction = (End - Start).Normalize();
            Normal = Direction.CrossProduct(XYZ.BasisZ).Normalize();
        }

        /// <summary>
        /// Стена.
        /// </summary>
        public Wall Wall { get; }

        /// <summary>
        /// Базовая линия стены, спроецированная на горизонтальную плоскость на нулевой отметке.
        /// </summary>
        public Line BaseLine { get; }

        /// <summary>
        /// Начальная точка спроецированной базовой линии.
        /// </summary>
        public XYZ Start { get; }

        /// <summary>
        /// Конечная точка спроецированной базовой линии.
        /// </summary>
        public XYZ End { get; }

        /// <summary>
        /// Направление спроецированной базовой линии.
        /// </summary>
        public XYZ Direction { get; }

        /// <summary>
        /// Горизонтальная нормаль к спроецированной базовой линии.
        /// </summary>
        public XYZ Normal { get; }

        /// <summary>
        /// Длина спроецированной базовой линии.
        /// </summary>
        public double Length => Start.DistanceTo(End);

        /// <summary>
        /// Создает обертку над стеной.
        /// </summary>
        /// <returns>Обертка, либо null, если базовая линия стены не является прямой.</returns>
        public static WallLine Create(Wall wall) {
            wall.ThrowIfNull();

            if(wall.Location is not LocationCurve locationCurve || locationCurve.Curve is not Line line) {
                return null;
            }

            var start = Flatten(line.GetEndPoint(0));
            var end = Flatten(line.GetEndPoint(1));
            // После проецирования вертикальная линия выродилась бы в точку
            if(start.DistanceTo(end) < wall.Document.Application.ShortCurveTolerance) {
                return null;
            }
            return new WallLine(wall, Line.CreateBound(start, end));
        }

        /// <summary>
        /// Проецирует точку на горизонтальную плоскость на нулевой отметке.
        /// </summary>
        private static XYZ Flatten(XYZ point) {
            return new XYZ(point.X, point.Y, 0);
        }
    }


    /// <summary>
    /// Группа параллельных стен помещения - одно из его направлений.
    /// </summary>
    /// <remarks>
    /// Задает локальную систему координат: ось вдоль стен и нормаль поперек.
    /// Стена в ней описывается смещением по нормали и диапазоном проекции на ось.
    /// </remarks>
    private class DirectionGroup {
        private readonly List<WallLine> _wallLines = [];

        public DirectionGroup(WallLine firstWallLine) {
            firstWallLine.ThrowIfNull();

            Origin = firstWallLine.Start;
            Axis = firstWallLine.Direction;
            Normal = firstWallLine.Normal;
        }

        /// <summary>
        /// Точка отсчета локальной системы координат.
        /// </summary>
        public XYZ Origin { get; }

        /// <summary>
        /// Направление вдоль стен группы.
        /// </summary>
        public XYZ Axis { get; }

        /// <summary>
        /// Нормаль поперек стен группы.
        /// </summary>
        public XYZ Normal { get; }

        /// <summary>
        /// Стены группы.
        /// </summary>
        public IReadOnlyList<WallLine> WallLines => _wallLines;

        /// <summary>
        /// Суммарная длина стен группы.
        /// </summary>
        public double TotalLength { get; private set; }

        public void Add(WallLine wallLine) {
            _wallLines.Add(wallLine);
            TotalLength += wallLine.Length;
        }
    }


    /// <summary>
    /// Стена в локальной системе координат направления.
    /// </summary>
    private class WallSegment {
        public WallSegment(WallLine wallLine, double offset, double start, double end) {
            WallLine = wallLine;
            Offset = offset;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Стена с базовой линией.
        /// </summary>
        public WallLine WallLine { get; }

        /// <summary>
        /// Смещение базовой линии вдоль нормали направления.
        /// </summary>
        public double Offset { get; }

        /// <summary>
        /// Начало диапазона проекции на ось направления.
        /// </summary>
        public double Start { get; }

        /// <summary>
        /// Конец диапазона проекции на ось направления.
        /// </summary>
        public double End { get; }

        /// <summary>
        /// Возвращает расстояние от параметра на оси до диапазона проекции.
        /// </summary>
        /// <returns>Ноль, если параметр попадает в диапазон.</returns>
        public double GetDistanceTo(double position) {
            if(position < Start) {
                return Start - position;
            }
            return position > End ? position - End : 0;
        }
    }


    /// <summary>
    /// Сторона помещения в одном направлении - одна или несколько стен, лежащих на общей линии.
    /// </summary>
    private class WallSide {
        private readonly List<WallSegment> _segments = [];

        /// <summary>
        /// Стены стороны, разложенные в системе координат направления.
        /// </summary>
        public IReadOnlyList<WallSegment> Segments => _segments;

        /// <summary>
        /// Смещение стороны вдоль нормали направления.
        /// </summary>
        /// <remarks>Усреднено по стенам стороны, чтобы допуск на коллинеарность не накапливался.</remarks>
        public double Offset { get; private set; }

        public void Add(WallSegment segment) {
            _segments.Add(segment);
            Offset = _segments.Average(s => s.Offset);
        }

        /// <summary>
        /// Возвращает стену стороны, ближайшую к позиции размещения размера.
        /// </summary>
        /// <remarks>
        /// Для габаритного размера позиция попадает внутрь одной из стен,
        /// для размера ступени - может оказаться за ее краем, тогда берется ближайшая стена.
        /// </remarks>
        public WallLine GetNearestWallLine(double position) {
            return _segments
                .OrderBy(s => s.GetDistanceTo(position))
                .Select(s => s.WallLine)
                .FirstOrDefault();
        }
    }


    /// <summary>
    /// Пара сторон помещения, между которыми ставится размер.
    /// </summary>
    private class WallPair {
        public WallPair(
            Room room,
            DirectionGroup directionGroup,
            WallSide first,
            WallSide second,
            double position,
            double distance,
            bool isStep,
            double rangeStart,
            double rangeEnd) {

            Room = room;
            DirectionGroup = directionGroup;
            First = first;
            Second = second;
            Position = position;
            Distance = distance;
            IsStep = isStep;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
        }

        /// <summary>
        /// Помещение, которому принадлежит пара.
        /// </summary>
        /// <remarks>Нужно при разводке подписи: влезающая подпись не должна покидать свое помещение.</remarks>
        public Room Room { get; }

        /// <summary>
        /// Направление, которому принадлежит пара.
        /// </summary>
        public DirectionGroup DirectionGroup { get; }

        /// <summary>
        /// Первая сторона пары.
        /// </summary>
        public WallSide First { get; }

        /// <summary>
        /// Вторая сторона пары.
        /// </summary>
        public WallSide Second { get; }

        /// <summary>
        /// Параметр размещения размера на оси направления.
        /// </summary>
        /// <remarks>Может измениться при подборе свободного места по бинарной карте вида.</remarks>
        public double Position { get; set; }

        /// <summary>
        /// Расстояние между сторонами.
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Признак размера ступени - стороны не перекрываются, размер стоит у внутреннего угла.
        /// </summary>
        public bool IsStep { get; }

        /// <summary>
        /// Начало коридора, в пределах которого линию размера можно двигать.
        /// </summary>
        public double RangeStart { get; }

        /// <summary>
        /// Конец коридора, в пределах которого линию размера можно двигать.
        /// </summary>
        public double RangeEnd { get; }
    }


    /// <summary>
    /// Плоская боковая грань стены вместе с ее опорной плоскостью.
    /// </summary>
    private class WallFace {
        public WallFace(Reference reference, PlanarFace face) {
            Reference = reference;
            Face = face;
        }

        /// <summary>
        /// Опорная плоскость грани.
        /// </summary>
        public Reference Reference { get; }

        /// <summary>
        /// Плоская боковая грань стены.
        /// </summary>
        public PlanarFace Face { get; }
    }
}

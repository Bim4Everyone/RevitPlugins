using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.Dimensions.DimensionReferences;
using RevitDocumenter.Models.Dimensions.DimensionServices;

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

    /// <summary>
    /// Имя типоразмера размера, выбираемого по умолчанию.
    /// </summary>
    private readonly string _defSelectedDimensionTypeName = "GOST Common_2.5 мм_0.9";

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
    /// Максимальное расстояние между сторонами помещения.
    /// </summary>
    /// <remarks>Ограничение не дает связать размером стены на противоположных краях квартиры.</remarks>
    private readonly double _maxDistanceBetweenSides = UnitUtilsHelper.ConvertToInternalValue(8000);

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
    /// Количество направлений, по которым проставляются размеры в одном помещении.
    /// </summary>
    /// <remarks>
    /// Берутся направления с наибольшей суммарной длиной стен - для ортогональной планировки
    /// это две взаимно перпендикулярные группы. Косые стены в размеры не попадают.
    /// </remarks>
    private readonly int _directionCount = 2;

    private string _errorText;
    private DimensionType _selectedDimensionType;
    private List<DimensionType> _dimensionTypes = [];

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public ApartmentDimensioningViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig.ThrowIfNull();
        _revitRepository = revitRepository.ThrowIfNull();
        _localizationService = localizationService.ThrowIfNull();

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

        CreateDimensions();

        mainTransaction.Commit();
    }

    /// <summary>
    /// Основной метод простановки размеров по помещениям активного вида.
    /// </summary>
    private void CreateDimensions() {
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
                foreach(var sidePair in GetSidePairs(directionGroup, wallSides)) {
                    CreateDimension(
                        sidePair, elevation, dimensionCreator, referenceAnalizeService, existingDimensionRefs);
                }
            }
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
    private List<WallPair> GetSidePairs(DirectionGroup directionGroup, List<WallSide> wallSides) {
        directionGroup.ThrowIfNull();
        wallSides.ThrowIfNull();

        var wallPairs = new List<WallPair>();
        for(int i = 0; i < wallSides.Count - 1; i++) {
            for(int j = i + 1; j < wallSides.Count; j++) {
                var wallPair = CreateWallPair(directionGroup, wallSides[i], wallSides[j]);
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
    private WallPair CreateWallPair(DirectionGroup directionGroup, WallSide first, WallSide second) {
        // Расстояние между сторонами постоянно - стороны параллельны
        double distance = Math.Abs(first.Offset - second.Offset);
        if(distance < _minDistanceBetweenSides || distance > _maxDistanceBetweenSides) {
            return null;
        }

        // Ищем участок, на котором стороны сходятся ближе всего.
        // Положительная длина - перекрытие проекций, отрицательная - зазор между ними
        double bestLength = double.MinValue;
        double position = 0;

        foreach(var firstSegment in first.Segments) {
            foreach(var secondSegment in second.Segments) {
                double start = Math.Max(firstSegment.Start, secondSegment.Start);
                double end = Math.Min(firstSegment.End, secondSegment.End);
                double length = end - start;

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

        var firstWallLine = first.GetNearestWallLine(position);
        var secondWallLine = second.GetNearestWallLine(position);
        if(firstWallLine is null || secondWallLine is null) {
            return null;
        }

        return new WallPair(directionGroup, first, second, firstWallLine, secondWallLine, position, distance);
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
        DimensionCreator dimensionCreator,
        ReferenceAnalizeService referenceAnalizeService,
        List<ReferenceArray> existingDimensionRefs) {

        // Опорные плоскости - обращенные друг к другу грани стен
        var references = GetFacingReferences(wallPair);
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

        try {
            var dimension = dimensionCreator.Create(dimensionLine, references, SelectedDimensionType);
            if(dimension != null) {
                existingDimensionRefs.Add(references);
            }
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            // Revit отказался строить размер по этой паре граней (вырожденный случай на стыке сторон).
            // Пропускаем пару, чтобы не прерывать обработку остальных помещений
        }
    }

    /// <summary>
    /// Подбирает пару граней стен для простановки размера.
    /// </summary>
    /// <remarks>
    /// Грани должны быть параллельны, разнонаправлены (нормали смотрят навстречу друг другу)
    /// и находиться на минимальном расстоянии - это дает размер "в свету".
    /// </remarks>
    /// <returns>Массив из двух опорных плоскостей, либо null, если подходящая пара не найдена.</returns>
    private ReferenceArray GetFacingReferences(WallPair wallPair) {
        wallPair.ThrowIfNull();

        var firstFaces = GetSideFaces(wallPair.FirstWallLine.Wall);
        var secondFaces = GetSideFaces(wallPair.SecondWallLine.Wall);
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
            DirectionGroup directionGroup,
            WallSide first,
            WallSide second,
            WallLine firstWallLine,
            WallLine secondWallLine,
            double position,
            double distance) {

            DirectionGroup = directionGroup;
            First = first;
            Second = second;
            FirstWallLine = firstWallLine;
            SecondWallLine = secondWallLine;
            Position = position;
            Distance = distance;
        }

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
        /// Стена первой стороны, от грани которой строится размер.
        /// </summary>
        public WallLine FirstWallLine { get; }

        /// <summary>
        /// Стена второй стороны, от грани которой строится размер.
        /// </summary>
        public WallLine SecondWallLine { get; }

        /// <summary>
        /// Параметр размещения размера на оси направления.
        /// </summary>
        public double Position { get; }

        /// <summary>
        /// Расстояние между сторонами.
        /// </summary>
        public double Distance { get; }
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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;

using Frame = RevitPunchingRebar.Models.Frame;

namespace RevitPunchingRebar.Services;

internal class PunchingRebarPlacementService : IPunchingRebarPlacementService {
    // Угол между каркасом и пилоном, при котором каркас стоит у коротких граней пилона, градусы
    private const double _crossAngle = 90;

    // Расстояние до вспомогательных точек при построении плоскости отражения
    private const double _planePointOffset = 10;

    // Допуск при сравнении углов, радианы
    private const double _angleTolerance = 1e-6;

    // Допуск при сравнении отметок, футы.
    private const double _elevationTolerance = 1e-6;

    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    // Количество каркасов, созданных при текущей расстановке
    private int _createdFramesCount;

    /// <summary>
    /// Создает сервис расстановки каркасов.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к документу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public PunchingRebarPlacementService(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    public PlacementResult Run(IEnumerable<IPylon> pylons, IFrameParams frameParams) {
        FamilySymbol frameSymbol = _revitRepository.GetFrameSymbol(
            frameParams.FrameFamilyName, frameParams.FrameFamilyTypeName);

        // Тип, который еще не размещался в проекте, нужно активировать до создания экземпляров
        if(!frameSymbol.IsActive) {
            frameSymbol.Activate();
        }

        var result = new PlacementResult();
        _createdFramesCount = 0;

        foreach(IPylon pylon in pylons) {
            result.PylonsCount++;

            string skipReason = GetSkipReason(pylon);
            if(skipReason != null) {
                result.SkippedPylons.Add(FormatSkippedPylon(pylon, skipReason));
                continue;
            }

            // Каркасы каждого пилона создаются в своей подтранзакции:
            // при ошибке откатывается только этот пилон
            int createdFramesBefore = _createdFramesCount;
            using var subTransaction = new SubTransaction(_revitRepository.Document);
            subTransaction.Start();
            try {
                CreateFrames(pylon, frameSymbol, frameParams);
                subTransaction.Commit();
                result.ProcessedPylonsCount++;
            } catch(Exception ex) when(ex is Autodesk.Revit.Exceptions.ApplicationException
                                         || ex is InvalidOperationException) {
                subTransaction.RollBack();
                _createdFramesCount = createdFramesBefore;
                result.SkippedPylons.Add(FormatSkippedPylon(pylon, ex.Message));
            }
        }

        result.CreatedFramesCount = _createdFramesCount;
        return result;
    }

    /// <summary>
    /// Возвращает причину, по которой у пилона нельзя расставить каркасы, или null, если можно.
    /// </summary>
    private string GetSkipReason(IPylon pylon) {
        if(pylon.Length <= 0 || pylon.Width <= 0) {
            return _localizationService.GetLocalizedString("Report.ErrorPylonSize");
        }

        return null;
    }

    private string FormatSkippedPylon(IPylon pylon, string reason) {
        string pylonName = string.Format(
            _localizationService.GetLocalizedString("Report.PylonName"),
            pylon.PylonInstance.Document.Title,
            pylon.PylonInstance.Id);

        return $"{pylonName}: {reason}";
    }

    private void CreateFrames(IPylon pylon, FamilySymbol frameSymbol, IFrameParams frameParams) {
        XYZ pylonLocation = pylon.GetLocation();

        // Плановое положение определяется пилоном, а отметка — выбранной плитой.
        // Для нижней и верхней плиты используется ее фактический верх, независимо от высоты пилона.
        double slabTopElevation = frameParams.HostSlab.TopElevation.Value;
        var location = new XYZ(pylonLocation.X, pylonLocation.Y, slabTopElevation);

        double longSideDepth = frameParams.RebarCoverTop;
        double shortSideDepth = longSideDepth + 2 * frameParams.PlateRebarDiameter - frameParams.StirrupRebarDiameter;

        // Продольная ось каркаса перпендикулярна его FacingOrientation.
        CreateFrameRows(pylon, frameSymbol, frameParams, location, slabTopElevation - shortSideDepth,
            0, -pylon.FacingOrientation);

        var copyOrientation = new XYZ(
            -pylon.FacingOrientation.Y,
            pylon.FacingOrientation.X,
            pylon.FacingOrientation.Z);
        CreateFrameRows(pylon, frameSymbol, frameParams, location, slabTopElevation - longSideDepth,
            Math.PI / 2, -copyOrientation);
    }

    /// <summary>
    /// Создает ряд каркасов в заданном направлении и его зеркальный ряд относительно пилона.
    /// </summary>
    private void CreateFrameRows(
        IPylon pylon, FamilySymbol frameSymbol, IFrameParams frameParams,
        XYZ location, double targetElevation, double rotationOffset, XYZ copyDirection) {
        var frame = _revitRepository.CreateFrame(frameSymbol, frameParams, location);
        _createdFramesCount++;

        SetFrameElevation(frame, targetElevation);
        double angle = GetAlignAngle(frame.FrameInstance.FacingOrientation, pylon.FacingOrientation);
        RotateFrame(frame, angle + rotationOffset);
        MoveFrameCrossColumn(pylon, frame, frameParams);
        MoveFrameAlongColumn(pylon, frame, frameParams);
        _revitRepository.EnsureFrameHost(
            frame.FrameInstance, frameSymbol, frameParams.HostSlab.SlabInstance, "Report.FrameHost.Move");
        CopyFrame(pylon, frame, frameParams, copyDirection);

        var mirroredFrame = MirrorFrame(pylon, frame, frameParams);
        CopyFrame(pylon, mirroredFrame, frameParams, copyDirection);
    }

    /// <summary>
    /// Устанавливает расчётную отметку точки вставки каркаса.
    /// </summary>
    private void SetFrameElevation(Frame frame, double targetElevation) {
        var location = (LocationPoint) frame.FrameInstance.Location;
        double elevationChange = targetElevation - location.Point.Z;
        if(Math.Abs(elevationChange) <= _elevationTolerance) {
            return;
        }

        if(!location.Move(XYZ.BasisZ * elevationChange)
           || Math.Abs(((LocationPoint) frame.FrameInstance.Location).Point.Z - targetElevation) > _elevationTolerance) {
            throw new InvalidOperationException(_localizationService.GetLocalizedString("Report.ErrorFrameElevation"));
        }
    }

    /// <summary>
    /// Возвращает угол поворота против часовой стрелки вокруг вертикальной оси, после которого
    /// направление каркаса станет параллельно направлению пилона (совпадет с ним или будет противоположно).
    /// </summary>
    /// <remarks>
    /// Угол со знаком берется через AngleOnPlaneTo: AngleTo не учитывает направление поворота,
    /// и при части ориентаций пилона каркас поворачивался не туда.
    /// Результат лежит в диапазоне [0, π]: для пилонов, которые раньше обрабатывались верно,
    /// ориентация каркаса остается прежней.
    /// </remarks>
    private double GetAlignAngle(XYZ frameDirection, XYZ pylonDirection) {
        double angle = frameDirection.AngleOnPlaneTo(pylonDirection, XYZ.BasisZ);

        // Направления почти совпадают: угол чуть меньше 2π из-за погрешности вычислений
        if(angle > 2 * Math.PI - _angleTolerance) {
            return angle - 2 * Math.PI;
        }

        return angle <= Math.PI + _angleTolerance ? angle : angle - Math.PI;
    }

    /// <summary>
    /// Поворачивает семейство на заданный угол вокруг своей оси
    /// </summary>
    private void RotateFrame(Frame frame, double angle) {
        XYZ location = frame.GetLocation();
        Line axeLine = Line.CreateBound(location, new XYZ(location.X, location.Y, location.Z + 1));

        frame.FrameInstance.Location.Rotate(axeLine, angle);
    }

    /// <summary>
    /// Перемещает каркас перпендикулярно длинной стороне пилона на необходимое расстояние от грани пилона
    /// </summary>
    private void MoveFrameCrossColumn(IPylon pylon, Frame frame, IFrameParams frameParams) {
        XYZ startPoint = frame.GetLocation();
        XYZ pylonLocation = pylon.GetLocation();
        XYZ direction = pylon.FacingOrientation;
        double angle = GetAngleBetweenInstances(pylon, frame);
        double offset;

        if(angle == _crossAngle) {
            int frameCount = GetFramesCount(pylon, frame, frameParams);
            double frameWidth = frameParams.FrameWidth;
            offset = (frameCount * (2 * frameWidth) - frameWidth) / 2 - 0.5 * frameWidth;
        } else {
            double afterPylonDistance = frameParams.GetAfterPylonDistance();
            double frameLength = frameParams.GetFrameLength();
            offset = frameLength / 2 + pylon.Width / 2 + afterPylonDistance;
        }

        var endPoint = new XYZ(
            pylonLocation.X - direction.Y * offset,
            pylonLocation.Y + direction.X * offset,
            startPoint.Z);

        frame.FrameInstance.Location.Move(endPoint - startPoint);
    }

    /// <summary>
    /// Перемещает каркас вдоль длинной стороны пилона в исходную точку для копирования
    /// </summary>
    private void MoveFrameAlongColumn(IPylon pylon, Frame frame, IFrameParams frameParams) {
        XYZ startPoint = frame.GetLocation();
        XYZ direction = pylon.FacingOrientation;
        double angle = GetAngleBetweenInstances(pylon, frame);
        int frameCount = GetFramesCount(pylon, frame, frameParams);
        double offset;

        if(angle == _crossAngle) {
            double frameLength = frameParams.GetFrameLength();
            double afterPylonDistance = frameParams.GetAfterPylonDistance();
            offset = 0.5 * frameLength + 0.5 * pylon.Length + afterPylonDistance;
        } else {
            double frameWidth = frameParams.FrameWidth;
            offset = 0.5 * (frameCount * (2 * frameWidth) - 2 * frameWidth);
        }

        var endPoint = new XYZ(
            startPoint.X + direction.X * offset,
            startPoint.Y + direction.Y * offset,
            startPoint.Z);

        frame.FrameInstance.Location.Move(endPoint - startPoint);
    }

    /// <summary>
    /// Возращает количество каркасов в указанном направлении
    /// </summary>
    private int GetFramesCount(IPylon pylon, Frame frame, IFrameParams frameParams) {
        double angle = GetAngleBetweenInstances(pylon, frame);
        if(angle == _crossAngle) {
            double punchingZone = frameParams.GetPunchingZone();
            double punchingLength = pylon.Width + 2 * punchingZone;
            return Convert.ToInt32(Math.Ceiling((punchingLength / frameParams.FrameWidth + 1) / 2));
        }

        double afterColumnDistance = frameParams.GetAfterPylonDistance();
        double length = pylon.Length + 2 * afterColumnDistance;
        return Convert.ToInt32(Math.Ceiling((length / frameParams.FrameWidth - 1) / 2));
    }

    private void CopyFrame(IPylon pylon, Frame frame, IFrameParams frameParams, XYZ direction) {
        XYZ startPoint = frame.GetLocation();
        int frameCount = GetFramesCount(pylon, frame, frameParams);
        double step = frameParams.FrameWidth * 2;

        for(int i = 1; i < frameCount; i++) {
            var endPoint = new XYZ(
                startPoint.X + direction.X * step * i,
                startPoint.Y + direction.Y * step * i,
                startPoint.Z);

            ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElement(
                frame.FrameInstance.Document, frame.FrameInstance.Id, endPoint - startPoint);
            GetTransformedFrame(
                copiedIds, frame.FrameInstance, frameParams.HostSlab.SlabInstance, "Report.FrameHost.Copy");
            _createdFramesCount++;

            startPoint = endPoint;
        }
    }

    private Frame MirrorFrame(IPylon pylon, Frame frame, IFrameParams frameParams) {
        XYZ pylonLocation = pylon.GetLocation();
        XYZ direction = pylon.FacingOrientation;
        double angle = GetAngleBetweenInstances(pylon, frame);
        XYZ planeDirectionPoint;

        if(angle == _crossAngle) {
            planeDirectionPoint = new XYZ(
                pylonLocation.X - _planePointOffset * direction.Y,
                pylonLocation.Y + _planePointOffset * direction.X,
                pylonLocation.Z);
        } else {
            planeDirectionPoint = new XYZ(
                pylonLocation.X + _planePointOffset * direction.X,
                pylonLocation.Y + _planePointOffset * direction.Y,
                pylonLocation.Z);
        }

        var planeVerticalPoint = new XYZ(
            pylonLocation.X,
            pylonLocation.Y,
            pylonLocation.Z + _planePointOffset);
        Plane plane = Plane.CreateByThreePoints(pylonLocation, planeDirectionPoint, planeVerticalPoint);

        IList<ElementId> mirrorElements = ElementTransformUtils.MirrorElements(
            frame.FrameInstance.Document,
            new List<ElementId>() { frame.FrameInstance.Id },
            plane,
            true);

        FamilyInstance instance = GetTransformedFrame(
            mirrorElements, frame.FrameInstance, frameParams.HostSlab.SlabInstance, "Report.FrameHost.Mirror");
        var mirroredFrame = new Frame(instance, frameParams, _localizationService);

        _createdFramesCount++;

        return mirroredFrame;
    }

    private FamilyInstance GetTransformedFrame(
        IEnumerable<ElementId> ids, FamilyInstance original, Floor slab, string operationKey) {
        // Revit может вернуть вместе с каркасом идентификаторы его зависимых элементов.
        ElementId typeId = original.GetTypeId();
        FamilyInstance instance = ids.Select(id => original.Document.GetElement(id))
            .OfType<FamilyInstance>()
            .FirstOrDefault(candidate => candidate.GetTypeId() == typeId && candidate.SuperComponent is null);
        _revitRepository.EnsureFrameHost(instance, original.Symbol, slab, operationKey);
        return instance;
    }

    private double GetAngleBetweenInstances(IPylon pylon, Frame frame) {
        double angle = Math.Round(
            Math.Abs(pylon.FacingOrientation.AngleTo(frame.FrameInstance.FacingOrientation)) * 180 / Math.PI);

        return angle;
    }
}

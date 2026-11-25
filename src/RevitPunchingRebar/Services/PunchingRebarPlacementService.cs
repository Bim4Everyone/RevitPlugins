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
    // Допуск при сравнении углов, радианы
    private const double _angleTolerance = 1e-6;

    // Допуск при сравнении координат, футы.
    private const double _positionTolerance = 1e-6;

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
                if(_createdFramesCount == createdFramesBefore) {
                    subTransaction.RollBack();
                    result.SkippedPylons.Add(FormatSkippedPylon(
                        pylon, _localizationService.GetLocalizedString("Report.ErrorNoFrameLocationsOnSlab")));
                    continue;
                }

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

        double slabBarDiameter = frameParams.PlateRebarDiameter;
        double stirrupDiameter = frameParams.StirrupRebarDiameter;
        double shortSideDepth = frameParams.RebarCoverTop + 2 * slabBarDiameter - stirrupDiameter;
        double longSideDepth = frameParams.RebarCoverTop + slabBarDiameter - stirrupDiameter;
        if(frameParams.SwapFrameElevations) {
            (longSideDepth, shortSideDepth) = (shortSideDepth, longSideDepth);
        }

        CreateFrameRows(pylon, frameSymbol, frameParams, location, slabTopElevation - shortSideDepth, false);
        CreateFrameRows(pylon, frameSymbol, frameParams, location, slabTopElevation - longSideDepth, true);
    }

    /// <summary>
    /// Расставляет два ряда, перпендикулярных паре противоположных граней пилона.
    /// </summary>
    private void CreateFrameRows(
        IPylon pylon, FamilySymbol frameSymbol, IFrameParams frameParams, XYZ location,
        double targetElevation, bool atShortFace) {
        XYZ direction = pylon.FacingOrientation;
        var alongPylon = new XYZ(direction.X, direction.Y, 0);
        var acrossPylon = new XYZ(-direction.Y, direction.X, 0);
        XYZ alongRow = atShortFace ? acrossPylon : alongPylon;
        XYZ outward = atShortFace ? alongPylon : acrossPylon;
        double pylonSize = atShortFace ? pylon.Length : pylon.Width;

        int count = GetFramesCount(pylon, frameParams, atShortFace);
        XYZ step = alongRow * (2 * frameParams.FrameWidth);
        double distanceFromCenter = (pylonSize + frameParams.GetFrameLength()) / 2
            + frameParams.GetAfterPylonDistance();
        XYZ offset = outward * distanceFromCenter;
        XYZ firstCenter = location + step * ((count - 1) / 2.0);
        double rotationOffset = atShortFace ? Math.PI / 2 : 0;

        CreateFrameRow(frameSymbol, frameParams, firstCenter + offset, step, count,
            alongPylon, rotationOffset, targetElevation, null);
        CreateFrameRow(frameSymbol, frameParams, firstCenter - offset, step, count,
            alongPylon, rotationOffset, targetElevation, outward);
    }

    private void CreateFrameRow(
        FamilySymbol frameSymbol, IFrameParams frameParams, XYZ firstCenter, XYZ step, int count,
        XYZ pylonDirection, double rotationOffset, double targetElevation, XYZ mirrorNormal) {
        Floor slab = frameParams.HostSlab.SlabInstance;
        bool isFaceBased = frameSymbol.Family.FamilyPlacementType == FamilyPlacementType.WorkPlaneBased;
        Frame sourceFrame = null;
        for(int i = 0; i < count; i++) {
            XYZ center = firstCenter - step * i;
            if(!_revitRepository.HasSlabTopFaceAt(slab, center)) {
                continue;
            }

            // Каркас на рабочей плоскости создаём отдельно, чтобы привязать его к грани в точке размещения.
            if(sourceFrame is null || isFaceBased) {
                sourceFrame = TryPlaceFrame(
                    frameSymbol, frameParams, center, pylonDirection, rotationOffset, targetElevation, mirrorNormal);
                if(sourceFrame is null) {
                    continue;
                }
            } else if(!TryCopyFrame(sourceFrame, slab, center)) {
                continue;
            }

            _createdFramesCount++;
        }
    }

    private Frame TryPlaceFrame(
        FamilySymbol frameSymbol, IFrameParams frameParams, XYZ center, XYZ pylonDirection,
        double rotationOffset, double targetElevation, XYZ mirrorNormal) {
        Floor slab = frameParams.HostSlab.SlabInstance;
        var frame = _revitRepository.CreateFrame(frameSymbol, frameParams, center);
        FamilyInstance instance = frame.FrameInstance;
        XYZ insertion = ((LocationPoint) instance.Location).Point;
        double angle = GetAlignAngle(instance.FacingOrientation, pylonDirection) + rotationOffset;
        if(Math.Abs(angle) > _angleTolerance) {
            instance.Location.Rotate(Line.CreateBound(insertion, insertion + XYZ.BasisZ), angle);
        }

        if(mirrorNormal is not null) {
            var plane = Plane.CreateByNormalAndOrigin(mirrorNormal, insertion);
            ElementTransformUtils.MirrorElements(instance.Document, new[] { instance.Id }, plane, false);
        }

        XYZ frameCenter = frame.GetLocation();
        var translation = new XYZ(center.X - frameCenter.X, center.Y - frameCenter.Y, 0);
        XYZ actualInsertion = ((LocationPoint) instance.Location).Point;
        if(!_revitRepository.HasSlabTopFaceAt(slab, actualInsertion + translation)) {
            instance.Document.Delete(instance.Id);
            return null;
        }

        if(translation.GetLength() > _positionTolerance && !instance.Location.Move(translation)) {
            throw new InvalidOperationException(_localizationService.GetLocalizedString("Report.ErrorFramePosition"));
        }

        SetFrameElevation(frame, targetElevation);
        _revitRepository.EnsureFrameHost(instance, frameSymbol, slab, "Report.FrameHost.Move");
        return frame;
    }

    /// <summary>
    /// Устанавливает расчётную отметку точки вставки каркаса.
    /// </summary>
    private void SetFrameElevation(Frame frame, double targetElevation) {
        var location = (LocationPoint) frame.FrameInstance.Location;
        double elevationChange = targetElevation - location.Point.Z;
        if(Math.Abs(elevationChange) <= _positionTolerance) {
            return;
        }

        if(!location.Move(XYZ.BasisZ * elevationChange)
           || Math.Abs(((LocationPoint) frame.FrameInstance.Location).Point.Z - targetElevation) > _positionTolerance) {
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
    /// Возвращает количество каркасов в ряду у заданной грани пилона.
    /// </summary>
    private int GetFramesCount(IPylon pylon, IFrameParams frameParams, bool atShortFace) {
        double frameWidth = frameParams.FrameWidth;
        if(atShortFace) {
            double punchingZone = frameParams.GetPunchingZone();
            double punchingLength = pylon.Width + 2 * punchingZone;
            return (int) Math.Ceiling((punchingLength / frameWidth + 1) / 2);
        }

        double firstEndStirrupOffset = pylon.Length / 2 + frameParams.GetAfterPylonDistance();
        double endBarTipOffset = firstEndStirrupOffset - frameParams.LongRebarAnchorageLength;
        double halfWidthAcrossLongitudinalBars = (frameParams.WidthStep + frameParams.LongRebarDiameter) / 2;
        double cornerClearance = frameParams.StirrupStep;
        double maxCenterOffset = endBarTipOffset - cornerClearance - halfWidthAcrossLongitudinalBars;
        if(maxCenterOffset < -_positionTolerance) {
            throw new InvalidOperationException(_localizationService.GetLocalizedString("Report.ErrorFrameClearance"));
        }

        // При шаге 2 * FrameWidth крайний центр симметричного ряда отстоит от середины на (число - 1) * FrameWidth.
        int maxCount = (int) Math.Floor((maxCenterOffset + _positionTolerance) / frameWidth) + 1;
        int nominalCount = Math.Max(1, (int) Math.Ceiling(firstEndStirrupOffset / frameWidth - 0.5));
        return Math.Min(nominalCount, maxCount);
    }

    private bool TryCopyFrame(Frame sourceFrame, Floor slab, XYZ center) {
        FamilyInstance source = sourceFrame.FrameInstance;
        XYZ sourceCenter = sourceFrame.GetLocation();
        var translation = new XYZ(center.X - sourceCenter.X, center.Y - sourceCenter.Y, 0);
        XYZ insertion = ((LocationPoint) source.Location).Point;
        if(!_revitRepository.HasSlabTopFaceAt(slab, insertion + translation)) {
            return false;
        }

        ICollection<ElementId> ids = ElementTransformUtils.CopyElement(source.Document, source.Id, translation);
        EnsureCopiedFrameHost(ids, source, slab);
        return true;
    }

    private void EnsureCopiedFrameHost(IEnumerable<ElementId> ids, FamilyInstance original, Floor slab) {
        // Revit может вернуть вместе с каркасом идентификаторы его зависимых элементов.
        ElementId typeId = original.GetTypeId();
        FamilyInstance instance = ids.Select(id => original.Document.GetElement(id))
            .OfType<FamilyInstance>()
            .FirstOrDefault(candidate => candidate.GetTypeId() == typeId && candidate.SuperComponent is null);
        _revitRepository.EnsureFrameHost(instance, original.Symbol, slab, "Report.FrameHost.Copy");
    }
}

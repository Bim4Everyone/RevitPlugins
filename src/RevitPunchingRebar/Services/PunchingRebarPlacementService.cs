using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;

using Frame = RevitPunchingRebar.Models.Frame;

namespace RevitPunchingRebar.Services;
internal class PunchingRebarPlacementService : IPunchingRebarPlacementService {
    // Диаметр оправки загиба хомута в долях диаметра хомута для классов А240 и А500С
    private const double _stirrupBendFactorA240 = 2.5;
    private const double _stirrupBendFactorA500C = 5;

    // Угол между каркасом и пилоном, при котором каркас стоит у коротких граней пилона, градусы
    private const double _crossAngle = 90;

    // Расстояние до вспомогательных точек при построении плоскости отражения
    private const double _planePointOffset = 10;

    // Допуск при сравнении углов, радианы
    private const double _angleTolerance = 1e-6;

    private readonly RevitRepository _revitRepository;

    /// <summary>
    /// Создает сервис расстановки каркасов.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к документу Revit.</param>
    public PunchingRebarPlacementService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public void Run(IEnumerable<IPylon> pylons, IFrameParams frameParams) {
        FamilySymbol frameSymbol = _revitRepository.GetFrameSymbol(
            frameParams.FrameFamilyName, frameParams.FrameFamilyTypeName);

        // Тип, который еще не размещался в проекте, нужно активировать до создания экземпляров
        if(!frameSymbol.IsActive) {
            frameSymbol.Activate();
            _revitRepository.Document.Regenerate();
        }

        foreach(IPylon pylon in pylons) {
            CreateFrames(pylon, frameSymbol, frameParams);
        }
    }

    private void CreateFrames(IPylon pylon, FamilySymbol frameSymbol, IFrameParams frameParams) {
        XYZ location = new XYZ();

        if(frameParams.HostSlab.SlabInstance.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralFoundation) {
            location = pylon.GetLocation();
        } else {
            location = new XYZ(
                pylon.GetLocation().X,
                pylon.GetLocation().Y,
                pylon.GetLocation().Z + pylon.Height);
        }

        //Создаем каркас в первом направлении
        Frame frame1 = _revitRepository.CreateFrame(frameSymbol, frameParams, location);

        //Делаем геометрические преобразования
        MoveFrameDown(frame1, frameParams);
        RotateFrame(frame1, GetAlignAngle(frame1.FrameInstance.FacingOrientation, pylon.FacingOrientation));
        MoveFrameCrossColumn(pylon, frame1, frameParams);
        MoveFrameAlongColumn(pylon, frame1, frameParams);
        CopyFrame(pylon, frame1, frameParams, -pylon.FacingOrientation);

        //Зеркальная копия каркаса в первом направлении относительно пилона
        Frame mirroredFrame1 = MirrorFrame(pylon, frame1, frameParams);
        CopyFrame(pylon, mirroredFrame1, frameParams, -pylon.FacingOrientation);

        //Создаем каркас во втором направлении
        Frame frame2 = _revitRepository.CreateFrame(frameSymbol, frameParams, location);

        MoveFrameDown(frame2, frameParams);
        RotateFrame(frame2, GetAlignAngle(frame2.FrameInstance.FacingOrientation, pylon.FacingOrientation) + Math.PI / 2);
        MoveFrameCrossColumn(pylon, frame2, frameParams);
        MoveFrameAlongColumn(pylon, frame2, frameParams);

        XYZ copyOrientation = new XYZ
                    (
                        -pylon.FacingOrientation.Y,
                        pylon.FacingOrientation.X,
                        pylon.FacingOrientation.Z
                    );

        CopyFrame(pylon, frame2, frameParams, -copyOrientation);

        Frame mirroredFrame2 = MirrorFrame(pylon, frame2, frameParams);
        CopyFrame(pylon, mirroredFrame2, frameParams, -copyOrientation);
    }

    /// <summary>
    /// Перемещает семейство из верхней точки вставки вниз на необходимую с учетом фоновой арматуры
    /// </summary>
    private void MoveFrameDown(Frame frame, IFrameParams frameParams) {
        XYZ startPoint = frame.GetLocation();
        double bendFactor = GetStirrupBendFactor(frameParams.StirrupRebarClass);

        XYZ endPoint = new XYZ
            (
                startPoint.X,
                startPoint.Y,
                startPoint.Z + (frameParams.StirrupRebarDiameter + 0.5 * bendFactor * frameParams.StirrupRebarDiameter) -
                                0.5 * frameParams.LongRebarDiameter -
                                frameParams.RebarCoverTop -
                                frameParams.PlateRebarDiameter
            );

        frame.FrameInstance.Location.Move(endPoint - startPoint);
    }

    /// <summary>
    /// Возвращает диаметр оправки загиба хомута в долях диаметра хомута
    /// </summary>
    private double GetStirrupBendFactor(RebarClass rebarClass) {
        return rebarClass == RebarClass.A240 ? _stirrupBendFactorA240 : _stirrupBendFactorA500C;
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
        XYZ endPoint = new XYZ();

        double angle = GetAngleBetweenInstances(pylon, frame);

        if(angle == _crossAngle) {
            int frameCount = GetFramesCount(pylon, frame, frameParams);

            endPoint = new XYZ
                    (
                        pylon.GetLocation().X - pylon.FacingOrientation.Y * ((frameCount * (2 * frameParams.FrameWidth) - frameParams.FrameWidth) / 2 - 0.5 * frameParams.FrameWidth),
                        pylon.GetLocation().Y + pylon.FacingOrientation.X * ((frameCount * (2 * frameParams.FrameWidth) - frameParams.FrameWidth) / 2 - 0.5 * frameParams.FrameWidth),
                        startPoint.Z
                    );
        } else {
            double afterPylonDistance = frameParams.GetAfterPylonDistance();
            double frameLength = frameParams.GetFrameLength();

            endPoint = new XYZ
                (
                    pylon.GetLocation().X - pylon.FacingOrientation.Y * (frameLength / 2 + pylon.Width / 2 + afterPylonDistance),
                    pylon.GetLocation().Y + pylon.FacingOrientation.X * (frameLength / 2 + pylon.Width / 2 + afterPylonDistance),
                    startPoint.Z
                );
        }

        frame.FrameInstance.Location.Move(endPoint - startPoint);
    }

    /// <summary>
    /// Перемещает каркас вдоль длинной стороны пилона в исходную точку для копирования
    /// </summary>
    private void MoveFrameAlongColumn(IPylon pylon, Frame frame, IFrameParams frameParams) {
        XYZ startPoint = frame.GetLocation();
        XYZ endPoint = new XYZ();

        double angle = GetAngleBetweenInstances(pylon, frame);
        int frameCount = GetFramesCount(pylon, frame, frameParams);

        if(angle == _crossAngle) {
            double frameLength = frameParams.GetFrameLength();
            double afterPylonDistance = frameParams.GetAfterPylonDistance();

            endPoint = new XYZ
                    (
                        startPoint.X + pylon.FacingOrientation.X * (0.5 * frameLength + 0.5 * pylon.Length + afterPylonDistance),
                        startPoint.Y + pylon.FacingOrientation.Y * (0.5 * frameLength + 0.5 * pylon.Length + afterPylonDistance),
                        startPoint.Z
                    );
        } else {
            endPoint = new XYZ
                    (
                        startPoint.X + pylon.FacingOrientation.X * 0.5 * (frameCount * (2 * frameParams.FrameWidth) - 2 * frameParams.FrameWidth),
                        startPoint.Y + pylon.FacingOrientation.Y * 0.5 * (frameCount * (2 * frameParams.FrameWidth) - 2 * frameParams.FrameWidth),
                        startPoint.Z
                    );
        }

        frame.FrameInstance.Location.Move(endPoint - startPoint);
    }

    /// <summary>
    /// Возращает количество каркасов в указанном направлении
    /// </summary>
    /// <returns></returns>
    private int GetFramesCount(IPylon pylon, Frame frame, IFrameParams frameParams) {
        double punchingLength = 0;
        int framesCount = 0;

        double angle = GetAngleBetweenInstances(pylon, frame);

        if(angle == _crossAngle) {
            double punchingZone = frameParams.GetPunchingZone();
            punchingLength = pylon.Width + 2 * punchingZone;
            framesCount = Convert.ToInt32(Math.Ceiling((punchingLength / frameParams.FrameWidth + 1) / 2));
        } else {
            double afterColumnDistance = frameParams.GetAfterPylonDistance();
            punchingLength = pylon.Length + 2 * afterColumnDistance;
            framesCount = Convert.ToInt32(Math.Ceiling((punchingLength / frameParams.FrameWidth - 1) / 2));
        }

        return framesCount;
    }

    private void CopyFrame(IPylon pylon, Frame frame, IFrameParams frameParams, XYZ direction) {
        XYZ startPoint = frame.GetLocation();
        int frameCount = GetFramesCount(pylon, frame, frameParams);
        double step = frameParams.FrameWidth * 2;

        for(int i = 1; i < frameCount; i++) {
            XYZ endPoint = new XYZ
                (
                    startPoint.X + direction.X * step * i,
                    startPoint.Y + direction.Y * step * i,
                    startPoint.Z
                );

            ElementTransformUtils.CopyElement(frame.FrameInstance.Document, frame.FrameInstance.Id, endPoint - startPoint);

            startPoint = endPoint;
        }
    }

    private Frame MirrorFrame(IPylon pylon, Frame frame, IFrameParams frameParams) {
        Plane plane = null;
        double angle = GetAngleBetweenInstances(pylon, frame);

        if(angle == _crossAngle) {
            plane = Plane.CreateByThreePoints
                    (
                        pylon.GetLocation(),
                        new XYZ
                        (
                            pylon.GetLocation().X - _planePointOffset * pylon.FacingOrientation.Y,
                            pylon.GetLocation().Y + _planePointOffset * pylon.FacingOrientation.X,
                            pylon.GetLocation().Z
                        ),

                        new XYZ
                        (
                            pylon.GetLocation().X,
                            pylon.GetLocation().Y,
                            pylon.GetLocation().Z + _planePointOffset
                        )
                    );
        } else {
            plane = Plane.CreateByThreePoints
                (
                    pylon.GetLocation(),
                    new XYZ
                        (
                            pylon.GetLocation().X + _planePointOffset * pylon.FacingOrientation.X,
                            pylon.GetLocation().Y + _planePointOffset * pylon.FacingOrientation.Y,
                            pylon.GetLocation().Z
                        ),
                    new XYZ
                        (
                            pylon.GetLocation().X,
                            pylon.GetLocation().Y,
                            pylon.GetLocation().Z + _planePointOffset
                        )
                );
        }

        IList<ElementId> mirrorElements = ElementTransformUtils.MirrorElements(
            frame.FrameInstance.Document,
            new List<ElementId>() { frame.FrameInstance.Id },
            plane,
            true);

        Element element = frame.FrameInstance.Document.GetElement(mirrorElements.FirstOrDefault());
        Frame mirroredFrame = new Frame((FamilyInstance) element, frameParams);

        return mirroredFrame;
    }

    private double GetAngleBetweenInstances(IPylon pylon, Frame frame) {
        double angle = Math.Round(
            Math.Abs(pylon.FacingOrientation.AngleTo(frame.FrameInstance.FacingOrientation)) * 180 / Math.PI);

        return angle;
    }
}

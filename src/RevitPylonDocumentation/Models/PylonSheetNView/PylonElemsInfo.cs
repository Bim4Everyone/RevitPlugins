using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonElemsInfo {
    private readonly CreationSettings _settings;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;

    internal PylonElemsInfo(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        _settings = settings;
        _doc = repository.Document;
        _sheetInfo = pylonSheetInfo;
    }

    public XYZ VectorByLength { get; set; }
    public XYZ VectorByWidth { get; set; }
    public XYZ HostOrigin { get; set; }
    public double HostLength { get; set; }
    public double HostWidth { get; set; }
    public BoundingBoxXYZ ElemsBoundingBox { get; set; }
    public double ElemsBoundingBoxLength { get; set; }
    public double ElemsBoundingBoxLengthToMax { get; set; }
    public double ElemsBoundingBoxLengthToMin { get; set; }
    public double ElemsBoundingBoxWidth { get; set; }
    public double ElemsBoundingBoxWidthToMax { get; set; }
    public double ElemsBoundingBoxWidthToMin { get; set; }
    public double ElemsBoundingBoxMinZ { get; set; }
    public double ElemsBoundingBoxMaxZ { get; set; }
    public double LastPylonMinZ { get; set; }
    public double LastPylonMaxZ { get; set; }


    /// <summary>
    /// Определяет вектор направления основы пилона
    /// </summary>
    public void FindPylonHostVectors() {
        var bottomHost = _sheetInfo.HostElems.First();
        // В зависимости от принадлежности к категории определяем вектор направления основы пилона
        if(bottomHost.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
            var wall = bottomHost as Wall;
            var locationCurve = wall.Location as LocationCurve;
            var line = locationCurve.Curve as Line;

            var wallLineStart = line.GetEndPoint(0);
            var wallLineEnd = line.GetEndPoint(1);
            VectorByLength = wallLineEnd - wallLineStart;
        } else {
            var column = bottomHost as FamilyInstance;

            var locationPoint = column.Location as LocationPoint;
            double rotation = locationPoint.Rotation + 90 * Math.PI / 180;
            VectorByLength = Transform.CreateRotation(XYZ.BasisZ, rotation).OfVector(XYZ.BasisX);
        }
        VectorByLength = GetHostDirByProjectTransform(VectorByLength);
        VectorByWidth = VectorByLength.CrossProduct(XYZ.BasisZ);
    }

    /// <summary>
    /// Определяет точку вставки основы пилона
    /// </summary>
    public void FindPylonHostOrigin() {
        var bottomHost = _sheetInfo.HostElems.First();
        // В зависимости от принадлежности к категории определяем точку вставки основы пилона
        if(bottomHost.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
            var wall = bottomHost as Wall;
            var locationCurve = wall.Location as LocationCurve;
            var line = locationCurve.Curve as Line;

            var wallLineStart = line.GetEndPoint(0);
            HostOrigin = wallLineStart + 0.5 * VectorByLength;
        } else {
            var column = bottomHost as FamilyInstance;
            var locationPoint = column.Location as LocationPoint;
            HostOrigin = locationPoint.Point;

            // В случае FamilyInstance этого недостаточно, т.к. отметка по высоте будет на уровне 0
            // Поэтому берем высотную отметку уровня элемента, смещение элемента от этого уровня и складываем для Z
            var level = _doc.GetElement(column.LevelId) as Level;
            var levelElevation = level.Elevation;
            var levelOffset = column.GetParamValue<double>(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
            HostOrigin = new XYZ(HostOrigin.X, HostOrigin.Y, HostOrigin.Z + levelElevation + levelOffset);
        }
    }

    /// <summary>
    /// Получает габариты пилона в зависимости от категории, которой он был выполнен
    /// </summary>
    public void FindHostDimensions() {
        // Значения по умолчанию в случае, если произойдет ошибка
        HostLength = 6;
        HostWidth = 1;

        var elemForWork = _sheetInfo.HostElems.First();
        if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
            if(elemForWork is not Wall wall) { return; }
            if(wall.Location is not LocationCurve locationCurve) { return; }
            if(locationCurve.Curve is not Line line) { return; }

            var wallLineStart = line.GetEndPoint(0);
            var wallLineEnd = line.GetEndPoint(1);
            var hostVector = wallLineEnd - wallLineStart;
            HostLength = hostVector.GetLength();
            HostWidth = wall.WallType.Width;
        } else {
            if(elemForWork is not FamilyInstance column) { return; }

            HostLength = _sheetInfo.ParamValService
                            .GetParamValueAnywhere<double>(column, _settings.PylonSettings.PylonLengthParamName);
            HostWidth = _sheetInfo.ParamValService
                            .GetParamValueAnywhere<double>(column, _settings.PylonSettings.PylonWidthParamName);
        }
    }

    /// <summary>
    /// Получает максимум и минимум пилонов по высоте
    /// </summary>
    public void FindHostMaxMinByZ() {
        // Отметка Z точки максимума верхнего пилона
        var topElement = _sheetInfo.HostElems.Last();
        LastPylonMaxZ = topElement.get_BoundingBox(null).Max.Z;

        // Отметка Z точки минимума нижнего пилона
        var bottomElement = _sheetInfo.HostElems.First();
        LastPylonMinZ = bottomElement.get_BoundingBox(null).Min.Z;
    }

    /// <summary>
    /// Метод проверяет вектор основы на направленность и редактирует при необходимости
    /// Виды должны располагаться на плане снизу-вверх и справа-налево
    /// Некоторые основы локально повернуты не по данным направлениям, поэтому вектор нужно исправить
    /// </summary>
    private XYZ GetHostDirByProjectTransform(XYZ hostVector) {
        var hostDir = hostVector.Normalize();

        // Получаем углы между вектором основы и базисами
        var angleToX = Math.Round(hostDir.AngleTo(XYZ.BasisX) * (180.0 / Math.PI));
        var angleToY = Math.Round(hostDir.AngleTo(XYZ.BasisY) * (180.0 / Math.PI));

        // Определяем нужно ли инвертировать вектор в зависимости от его положения в системе координат проекта
        bool shouldInvert = angleToX <= 45 || angleToX >= 135 ? angleToX <= 45 : angleToY <= 45;
        return shouldInvert ? hostDir.Negate() : hostDir;
    }

    /// <summary>
    /// Определяет BoundingBox по всем элементам, которые относятся к пилону согласно заполненным параметрам
    /// </summary>
    public void FindElemsBoundingBox() {
        // Формируем ограниченный перечень категорий, которые относятся к пилону (+ армирование)
        BuiltInCategory[] categories = {
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_Rebar
            };

        // Создаем фильтр по указанным категориям
        var multiCategoryFilter = new LogicalOrFilter(
                categories
                    .Select<BuiltInCategory, ElementFilter>(bic => new ElementCategoryFilter(bic))
                    .ToList()
            );

        // Получаем список элементов, которые относятся к пилону с учетом фильтра по категориям и заполненным параметрам
        var elems = new FilteredElementCollector(_doc)
            .WherePasses(multiCategoryFilter)
            .WhereElementIsNotElementType()
            .Where(e =>
                _sheetInfo.ParamValService.GetParamValueAnywhere<string>(e, _settings.PylonSettings.ProjectSection)
                    == _sheetInfo.ProjectSection)
            .Where(e =>
                _sheetInfo.ParamValService.GetParamValueAnywhere<string>(e, _settings.PylonSettings.TypicalPylonFilterParameter)
                    == _settings.PylonSettings.TypicalPylonFilterValue)
            .Where(e => e.Category.GetBuiltInCategory().Equals(BuiltInCategory.OST_Rebar) ?
                _sheetInfo.ParamValService.GetParamValueAnywhere<string>(e, "обр_ФОП_Метка основы IFC")
                    == _sheetInfo.PylonKeyName :
                _sheetInfo.ParamValService.GetParamValueAnywhere<string>(e, _settings.PylonSettings.Mark)
                    == _sheetInfo.PylonKeyName)
            .ToList();

        // Получаем суммарный BoundingBox по элементам, которые относятся к пилону 
        BoundingBoxXYZ combinedBoundingBox = null;
        foreach(var elem in elems) {
            var elemBoundingBox = elem.get_BoundingBox(null);
            if(elemBoundingBox == null)
                continue;

            if(combinedBoundingBox == null) {
                combinedBoundingBox = elemBoundingBox;
            } else {
                // Редактируем суммарный BoundingBox, чтобы он включал BoundingBoxы по всем элементам пилона
                combinedBoundingBox.Min = new XYZ(
                    Math.Min(combinedBoundingBox.Min.X, elemBoundingBox.Min.X),
                    Math.Min(combinedBoundingBox.Min.Y, elemBoundingBox.Min.Y),
                    Math.Min(combinedBoundingBox.Min.Z, elemBoundingBox.Min.Z)
                );
                combinedBoundingBox.Max = new XYZ(
                    Math.Max(combinedBoundingBox.Max.X, elemBoundingBox.Max.X),
                    Math.Max(combinedBoundingBox.Max.Y, elemBoundingBox.Max.Y),
                    Math.Max(combinedBoundingBox.Max.Z, elemBoundingBox.Max.Z)
                );
            }
        }
        ElemsBoundingBox = combinedBoundingBox;
    }


    /// <summary>
    /// Определяет характеристики BoundingBox элементов пилона
    /// </summary>
    public void FindElemsBoundingBoxProps() {
        // Определяем стоит ли пилон параллельно какой либо оси системы координат проекта
        var angle = Math.Round(VectorByLength.AngleTo(XYZ.BasisX) * 180 / Math.PI);

        if(angle % 90 > 0) {
            // Если нет, то определять габариты будем не по BoundingBox, а по пилону
            ElemsBoundingBoxLength = HostLength;
            ElemsBoundingBoxLengthToMax = ElemsBoundingBoxLengthToMin = HostLength / 2;
            ElemsBoundingBoxWidth = HostWidth;
            ElemsBoundingBoxWidthToMax = ElemsBoundingBoxWidthToMin = HostWidth / 2;
        } else {
            var forLength = GetDistanceToProjectedMidPt(VectorByWidth);
            var forWidth = GetDistanceToProjectedMidPt(VectorByLength);

            ElemsBoundingBoxLength = forLength.toMax + forLength.toMin;
            ElemsBoundingBoxLengthToMax = ElemsBoundingBoxLengthToMin = ElemsBoundingBoxLength / 2;
            ElemsBoundingBoxWidth = forWidth.toMax + forWidth.toMin;

            if(angle % 180 > 0) {
                ElemsBoundingBoxWidthToMax = forWidth.toMax;
                ElemsBoundingBoxWidthToMin = forWidth.toMin;
            } else {
                ElemsBoundingBoxWidthToMax = forWidth.toMin;
                ElemsBoundingBoxWidthToMin = forWidth.toMax;
            }
        }
        ElemsBoundingBoxMinZ = ElemsBoundingBox.Min.Z;
        ElemsBoundingBoxMaxZ = ElemsBoundingBox.Max.Z;
    }


    /// <summary>
    /// Получаем длину проекции BoundingBox всех элементов пилона в зависимости от плоскости, на которую нужно 
    /// его спроецировать
    /// </summary>
    /// <param name="vector">Вектор, который будет являться нормалью к плоскости проекции</param>
    private (double toMax, double toMin) GetDistanceToProjectedMidPt(XYZ vector) {
        var bb = _sheetInfo.ElemsInfo.ElemsBoundingBox;
        var upDir = XYZ.BasisZ;

        // Получаем точку минимума BoundingBox, спроецированную на плоскость с нормалью по вектору и горизонталь
        var bbMinForVector = ProjectPointByPointNVector(HostOrigin, vector, bb.Min);
        bbMinForVector = ProjectPointByPointNVector(HostOrigin, upDir, bbMinForVector);

        // Получаем точку максимума BoundingBox, спроецированную на плоскость с нормалью по вектору и горизонталь
        var bbMaxForVector = ProjectPointByPointNVector(HostOrigin, vector, bb.Max);
        bbMaxForVector = ProjectPointByPointNVector(HostOrigin, upDir, bbMaxForVector);

        // Вычисляем длину проекции
        return (bbMaxForVector.DistanceTo(HostOrigin), bbMinForVector.DistanceTo(HostOrigin));
    }

    /// <summary>
    /// Возвращает точку спроецированную на плоскость
    /// </summary>
    /// <param name="origin">Точка, в которой будет находиться плоскость проекции</param>
    /// <param name="normal">Нормаль к плоскости проекции</param>
    /// <param name="point">Точка, которую нужно спроецировать</param>
    private XYZ ProjectPointByPointNVector(XYZ origin, XYZ normal, XYZ point) {
        normal = normal.Normalize();

        // Вычисляем вектор от точки на плоскости к целевой точке
        var vector = point - origin;

        // Находим расстояние вдоль нормали (скалярное произведение)
        double distance = normal.DotProduct(vector);

        // Проецируем точку на плоскость
        return point - distance * normal;
    }
}

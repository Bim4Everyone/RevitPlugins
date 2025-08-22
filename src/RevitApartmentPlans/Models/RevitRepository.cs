using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitApartmentPlans.Services;

namespace RevitApartmentPlans.Models;
internal class RevitRepository {
    private readonly SpatialElementBoundaryOptions _spatialElementBoundaryOptions;
    private readonly LinkFilterProvider _linkFilterProvider;

    public RevitRepository(UIApplication uiApplication, LinkFilterProvider linkFilterProvider) {
        _spatialElementBoundaryOptions = new SpatialElementBoundaryOptions() {
            SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
            StoreFreeBoundaryFaces = false
        };
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        _linkFilterProvider = linkFilterProvider ?? throw new ArgumentNullException(nameof(linkFilterProvider));
    }


    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;


    /// <summary>
    /// Возвращает все замкнутые контуры границ помещения в координатах активного файла
    /// </summary>
    /// <param name="room">Помещение</param>
    /// <returns>Список всех замкнутых контуров границ помещения</returns>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public IList<CurveLoop> GetBoundaryCurveLoops(RoomElement room) {
        if(room is null) { throw new ArgumentNullException(nameof(room)); }

        var boundaryLoops = room.Room.GetBoundarySegments(_spatialElementBoundaryOptions)
            .Select(loop => CurveLoop.Create(loop.Select(c => c.GetCurve()).ToArray()))
            .ToArray();
        if(room.Transform.IsIdentity && boundaryLoops.Length > 0) {
            return boundaryLoops;
        } else {
            var point = new UV();
            var bottomDir = XYZ.BasisZ.Negate();
            return room.Room.GetSolids()
                .SelectMany(s => s.Faces.OfType<PlanarFace>())
                .Where(f => f.ComputeNormal(point).IsAlmostEqualTo(bottomDir))
                .SelectMany(f => f.GetEdgesAsCurveLoops())
                .Select(c => CurveLoop.CreateViaTransform(c, room.Transform))
                .ToArray();
        }
    }

    /// <summary>
    /// Возвращает массив используемых типов видов
    /// </summary>
    public ViewType[] GetAllUsedViewTypes() {
        return new ViewType[] {
            ViewType.FloorPlan,
            ViewType.CeilingPlan
        };
    }

    /// <summary>
    /// По заданным Id находит все шаблоны видов из активного документа, по которым можно создать планы
    /// </summary>
    /// <param name="viewPlanIds">Id шаблонов видов</param>
    public ICollection<ViewPlan> GetViewTemplates(ICollection<ElementId> viewPlanIds) {
        if(viewPlanIds is null || viewPlanIds.Count == 0) {
            return Array.Empty<ViewPlan>();
        }

        var enabledViewTypes = GetAllUsedViewTypes();
        return new FilteredElementCollector(Document, viewPlanIds)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(plan => plan.IsTemplate && enabledViewTypes.Any(vt => vt == plan.ViewType))
            .ToArray();
    }

    /// <summary>
    /// Находит все шаблоны видов из активного документа, по которым можно создать планы
    /// </summary>
    public ICollection<ViewPlan> GetViewTemplates() {
        var enabledViewTypes = GetAllUsedViewTypes();
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(plan => plan.IsTemplate && enabledViewTypes.Any(vt => vt == plan.ViewType))
            .ToArray();
    }

    /// <summary>
    /// Возвращает коллекцию квартир, видимых на активном виде (плане) по значению параметра для группировки.
    /// </summary>
    /// <param name="paramName">Название параметра для группировки помещений по квартирам.</param>
    /// <exception cref="ArgumentException">Исключение, если название параметра пустое.</exception>
    public ICollection<Apartment> GetApartments(string paramName) {
        if(string.IsNullOrWhiteSpace(paramName)) { throw new ArgumentException(nameof(paramName)); }

        var rooms = GetRoomsFromActiveDoc(paramName);
        return GetApartments(rooms, paramName);
    }

    public ICollection<Apartment> GetApartments(string paramName, bool processLinks) {
        if(processLinks) {
            List<RoomElement> rooms = [.. GetRoomsFromActiveDoc(paramName)];
            var visibleLinks = GetVisibleLinks(GetActiveViewPlan());
            foreach(var link in visibleLinks) {
                rooms.AddRange(_linkFilterProvider.GetFilterOnView(link, GetActiveViewPlan())
                    .WherePasses(new RoomFilter())
                    .Cast<Room>()
                    .Where(r => r.Area > 0 && r.IsExistsParamValue(paramName))
                    .Select(r => new RoomElement(r, link)));
            }
            return GetApartments(rooms, paramName);
        } else {
            return GetApartments(paramName);
        }
    }

    /// <summary>
    /// Находит все параметры, по которым можно группировать помещения в квартиры
    /// </summary>
    public ICollection<ParameterElement> GetRoomGroupingParameters() {
        var room = new FilteredElementCollector(Document)
            .WherePasses(new RoomFilter())
            .FirstElement() as Room;
        // Текстовые параметры и параметры ключевых спецификаций
        ForgeTypeId[] paramTypes = { SpecTypeId.String.Text, ForgeTypeIdExtensions.EmptyForgeTypeId };

        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(ParameterElement))
            .Cast<ParameterElement>()
            .Where(p => room.IsExistsParam(p.Name) && paramTypes.Contains(p.GetUnitType()))
            .ToArray();
    }

    /// <summary>
    /// Метод для отладки. Создает квартиру из выбранных помещений
    /// </summary>
    public Apartment GetDebugApartment() {
        var rooms = PickRooms();
        return new Apartment(PickRooms(), "test", rooms.First().Room.Level);
    }

    /// <summary>
    /// Метод для отладки. Возвращает все шаблоны видов в активном документе
    /// </summary>
    public ICollection<ViewPlan> GetDebugTemplates() {
        var enabledViewTypes = GetAllUsedViewTypes();
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(plan => plan.IsTemplate && enabledViewTypes.Any(vt => vt == plan.ViewType))
            .ToArray();
    }

    /// <summary>
    /// Метод для отладки. Создает линии модели из заданного контура
    /// </summary>
    public void CreateDebugLines(CurveLoop curveLoop) {
        //using(Transaction t = Document.StartTransaction("Создание тестового контура")) {
        var geomPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, curveLoop.First().GetEndPoint(0));
        var sketch = SketchPlane.Create(Document, geomPlane);

        foreach(var item in curveLoop) {
            Document.Create.NewModelCurve(item, sketch);
        }

        //    t.Commit();
        //}
    }

    /// <summary>
    /// Копирует вид с детализацией.
    /// </summary>
    /// <param name="view">Вид для копирования</param>
    /// <returns>Скопированный вид с детализацией.</returns>
    /// <exception cref="InvalidOperationException">Исключение, если не удалось скопировать вид.</exception>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null.</exception>
    public ViewPlan DuplicateView(ViewPlan view) {
        if(view is null) {
            throw new ArgumentNullException(nameof(view));
        }
        // при копировании видов, с которыми работает плагин, проблем быть не должно,
        // но пусть будут исключения на всякий случай
        const ViewDuplicateOption opts = ViewDuplicateOption.WithDetailing;
        if(view.CanViewBeDuplicated(opts)) {
            var id = view.Duplicate(opts);
            if(id.IsNotNull()) {
                return Document.GetElement(id) as ViewPlan
                    ?? throw new InvalidOperationException(
                        $"Пустой id скопированного вида {view.Name} с детализацией");
            }
        }
        throw new InvalidOperationException($"Не удалось скопировать вид {view.Name} с детализацией");
    }

    /// <summary>
    /// Возвращает тип шаблона вида: план этажа/план потолка
    /// </summary>
    /// <param name="template">Шаблон вида</param>
    /// <exception cref="NotSupportedException">Исключение, если шаблон вида - не план этажа/потолка</exception>
    public ElementId GetViewFamilyTypeId(ViewPlan template) {
        return template.ViewType switch {
            ViewType.FloorPlan => GetViewFamilyTypeId(ViewFamily.FloorPlan),
            ViewType.CeilingPlan => GetViewFamilyTypeId(ViewFamily.CeilingPlan),
            _ => throw new NotSupportedException($"Тип шаблона {template.ViewType} не поддерживается. " +
                                    $"Название шаблона: \'{template.Name}\'"),
        };
    }

    /// <summary>
    /// Возвращает True, если активный вид - план, иначе False
    /// </summary>
    public bool ActiveViewIsPlan() {
        return (Document.ActiveView as ViewPlan) != null;
    }

    /// <summary>
    /// Возвращает активный вид (план)
    /// </summary>
    /// <exception cref="InvalidOperationException">Исключение, если активынй вид не является планом</exception>
    public ViewPlan GetActiveViewPlan() {
        return Document.ActiveView as ViewPlan
            ?? throw new InvalidOperationException("Активный вид не является планом");
    }

    public void ShowApartment(Apartment apartment) {
#if REVIT_2022_OR_LESS
        ActiveUIDocument.Selection.SetElementIds(apartment
            .GetRooms()
            .Where(r => r.Transform.IsIdentity)
            .Select(r => r.Room.Id)
            .ToArray());
#else
        var references = apartment.GetRooms()
            .Select(r => r.GetReference())
            .ToArray();
        ActiveUIDocument.Selection.SetReferences(references);
#endif
    }


    private ICollection<RevitLinkInstance> GetVisibleLinks(View view) {
        return new FilteredElementCollector(Document, view.Id)
            .WhereElementIsNotElementType()
            .OfClass(typeof(RevitLinkInstance))
            .ToElements()
            .OfType<RevitLinkInstance>()
            .ToArray();
    }

    private ICollection<RoomElement> GetRoomsFromActiveDoc(string paramName) {
        if(string.IsNullOrWhiteSpace(paramName)) { throw new ArgumentException(nameof(paramName)); }
        var level = GetLevelOfActivePlan();

        return new FilteredElementCollector(Document, GetActiveViewPlan().Id)
            .WherePasses(new RoomFilter())
            .Cast<Room>()
            .Where(r => r.Area > 0
                && r.LevelId == level.Id
                && r.IsExistsParamValue(paramName))
            .Select(r => new RoomElement(r))
            .ToArray();
    }

    private ICollection<Apartment> GetApartments(ICollection<RoomElement> rooms, string paramName) {
        var level = GetLevelOfActivePlan();
        return rooms.GroupBy(r => r.Room.GetParamValue<string>(paramName))
            .Select(g => new Apartment(g.ToArray(), g.Key, level))
            .ToArray();
    }

    /// <summary>
    /// Возвращает уровень, привязанный к активному виду, который должен быть планом
    /// </summary>
    /// <exception cref="InvalidOperationException">Исключение, если активный вид - не план</exception>
    private Level GetLevelOfActivePlan() {
        var view = GetActiveViewPlan();
        return view is null ? throw new InvalidOperationException("Активный вид не является планом") : view.GenLevel;
    }

    private ElementId GetViewFamilyTypeId(ViewFamily viewFamily) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .Where(v => v.ViewFamily == viewFamily)
            .First()
            .Id;
    }

    /// <summary>
    /// Метод для отладки. Возвращает помещения, выбранные в GUI Revit
    /// </summary>
    private ICollection<RoomElement> PickRooms() {
        ISelectionFilter filter = new SelectionFilterRooms(Document);
        var references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            filter,
            "Выберите помещения");

        List<RoomElement> rooms = [];
        foreach(var reference in references) {
            if((reference != null) && (Document.GetElement(reference) is Room room) && (room.Area > 0)) {
                rooms.Add(new RoomElement(room));
            }
        }
        return rooms;
    }
}

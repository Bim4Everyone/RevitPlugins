using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitStairRailing.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
internal class RevitRepository {
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }

    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;

    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Типоразмеры семейств «Каркас несущий», размещаемых по линии, — из них собирается ограждение.
    /// </summary>
    public List<FamilySymbol> GetFramingTypes() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_StructuralFraming)
            .Cast<FamilySymbol>()
            .Where(symbol => symbol.Family.FamilyPlacementType is FamilyPlacementType.CurveDrivenStructural
                or FamilyPlacementType.CurveBased)
            .OrderBy(symbol => symbol.FamilyName)
            .ThenBy(symbol => symbol.Name)
            .ToList();
    }

    /// <summary>
    /// Типоразмеры семейств «Каркас несущий», размещаемых в точке на рабочей плоскости, — для анкеров.
    /// </summary>
    public List<FamilySymbol> GetPointFramingTypes() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_StructuralFraming)
            .Cast<FamilySymbol>()
            .Where(symbol => symbol.Family.FamilyPlacementType is FamilyPlacementType.WorkPlaneBased
                or FamilyPlacementType.OneLevelBased)
            .OrderBy(symbol => symbol.FamilyName)
            .ThenBy(symbol => symbol.Name)
            .ToList();
    }

    /// <summary>
    /// Семейства пластин: «Каркас несущий» по линии с параметрами типа «t» и «h» (толщина и ширина сечения).
    /// </summary>
    public List<Family> GetPlateFamilies() {
        return GetFramingTypes()
            .Where(symbol => symbol.LookupParameter(RailingBuilder.PlateThicknessParameter) != null
                             && symbol.LookupParameter(RailingBuilder.PlateWidthParameter) != null)
            .Select(symbol => symbol.Family)
            .GroupBy(family => family.Id)
            .Select(group => group.First())
            .OrderBy(family => family.Name)
            .ToList();
    }

    /// <summary>
    /// Отрезки линий модели, выбранных до запуска плагина; пустой список — линии модели не выбраны.
    /// </summary>
    /// <exception cref="RailingException">Среди выбранных линий есть дуги или сплайны.</exception>
    public List<Line> GetSelectedModelLines() {
        return ToLines(ActiveUIDocument.Selection.GetElementIds().Select(id => Document.GetElement(id)));
    }

    /// <summary>
    /// Выбор линий модели траектории в Revit.
    /// </summary>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Пользователь отменил выбор.</exception>
    /// <exception cref="RailingException">Среди выбранных линий есть дуги или сплайны.</exception>
    public List<Line> PickModelLines(string prompt) {
        IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            new ModelLineSelectionFilter(),
            prompt);
        return ToLines(references.Select(reference => Document.GetElement(reference)));
    }

    public void SelectElements(ICollection<ElementId> elementIds) {
        ActiveUIDocument.Selection.SetElementIds(elementIds);
    }

    private static List<Line> ToLines(IEnumerable<Element> elements) {
        var curves = elements
            .OfType<ModelCurve>()
            .Select(modelCurve => modelCurve.GeometryCurve)
            .ToList();
        if(curves.Any(curve => curve is not Line)) {
            throw new RailingException("Path.NotLines");
        }

        return curves.Cast<Line>().ToList();
    }

    /// <summary>
    /// Выбор только линий модели.
    /// </summary>
    private class ModelLineSelectionFilter : ISelectionFilter {
        public bool AllowElement(Element element) {
            return element is ModelCurve;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}

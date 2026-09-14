using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitClassifierParameters.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    
    /// <summary>
    /// Категории элементов, участвующих в заполнении параметров бетона.
    /// </summary>
    private readonly List<BuiltInCategory> _categoriesForConcreteParams = [
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_StructuralColumns,
        BuiltInCategory.OST_Stairs,
        BuiltInCategory.OST_StructConnections,
        BuiltInCategory.OST_Roofs
    ];

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(
        UIApplication uiApplication,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
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


    public List<Material> GetElementMaterials() {
        var materials = GetElementInPj()
            .SelectMany(elem => elem.GetMaterialIds(false))
            .Distinct()
            .Select(id => Document.GetElement(id) as Material)
            .Where(material => material != null)
            .ToList();

        if(materials.Count == 0) {
            _messageBoxService.Show(_localizationService.GetLocalizedString("Repository.NoMaterialsForElements"));
        }
        return materials;
    }

    private List<Element> GetElementInPj() {
        var elements = new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .ToElements()
            .ToList();
        if(elements.Count != 0) {
            return elements;
        }
        _messageBoxService.Show(_localizationService.GetLocalizedString("Repository.NoElementsInProject"));
        return [];
    }

    public List<Parameter> GetParametersForFacadeType() {
        var wall = new FilteredElementCollector(Document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .FirstOrDefault();

        if(wall == null) {
            return [];
        }

        return wall.Parameters
            .Cast<Parameter>()
            .Where(p => p.StorageType == StorageType.String)
            .Where(p => !p.IsReadOnly)
            .OrderBy(p => p.Definition.Name)
            .ToList();
    }

    /// <summary>
    /// Возвращает все экземпляры стен в проекте.
    /// </summary>
    public List<Wall> GetWalls() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>()
            .ToList();
    }

    /// <summary>
    /// Возвращает экземпляры элементов нужных категорий по всему проекту.
    /// Используется для заполнения параметров бетона.
    /// </summary>
    public List<Element> GetElementsForConcreteParams() {
        var filter = new ElementMulticategoryFilter(_categoriesForConcreteParams);

        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .WherePasses(filter)
            .ToElements()
            .ToList();
    }
}

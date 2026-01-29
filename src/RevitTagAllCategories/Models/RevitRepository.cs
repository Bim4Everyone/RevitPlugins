using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTagAllCategories.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
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

    public IEnumerable<Category> GetFilterableCategories() {
        List<ElementId> allCategories = Document.Settings.Categories
            .Cast<Category>()
            .Select(x => x.Id)
            .ToList();

        var vv = ParameterFilterUtilities.RemoveUnfilterableCategories(allCategories);

        return vv.Select(x => Category.GetCategory(Document, x));
    }

    public IList<RevitLinkInstance> GetLinks() {
        IEnumerable<RevitLinkType> loadedLinkTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .Where(x => !x.IsNestedLink)
            .Where(x => x.GetLinkedFileStatus() == LinkedFileStatus.Loaded);

        var temp = loadedLinkTypes.ToList();

        ElementClassFilter filter = new ElementClassFilter(typeof(RevitLinkInstance));

        return loadedLinkTypes
            .SelectMany(x => x.GetDependentElements(filter))
            .Select(x => Document.GetElement(x))
            .Cast<RevitLinkInstance>()
            .ToList();
    }
}

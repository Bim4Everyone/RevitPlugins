using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitPackageDocumentation.Models;

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

        StructuralPlanViewFamilyTypes = GetStructuralPlanViewFamilyTypes();
        SectionViewFamilyTypes = GetSectionViewFamilyTypes();
        PlanViewTemplates = GetPlanViewTemplates();
        SectionViewTemplates = GetSectionViewTemplates();
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


    public List<ViewFamilyType> StructuralPlanViewFamilyTypes { get; }
    public List<ViewFamilyType> SectionViewFamilyTypes { get; }
    public List<ViewPlan> PlanViewTemplates { get; }
    public List<ViewSection> SectionViewTemplates { get; }

    /// <summary>
    /// Возвращает список типоразмеров видов в плане в проекте
    /// </summary>
    private List<ViewFamilyType> GetStructuralPlanViewFamilyTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewFamilyType))
        .OfType<ViewFamilyType>()
        .Where(a => ViewFamily.StructuralPlan == a.ViewFamily)
        .ToList();

    /// <summary>
    /// Возвращает список типоразмеров видов в разрезе в проекте
    /// </summary>
    private List<ViewFamilyType> GetSectionViewFamilyTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewFamilyType))
        .OfType<ViewFamilyType>()
        .Where(a => ViewFamily.Section == a.ViewFamily)
        .ToList();


    /// <summary>
    /// Возвращает список всех шаблонов планов в проекте
    /// </summary>
    public List<ViewPlan> GetPlanViewTemplates() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewPlan))
        .WhereElementIsNotElementType()
        .OfType<ViewPlan>()
        .Where(v => v.IsTemplate == true)
        .OrderBy(a => a.Name)
        .ToList();


    /// <summary>
    /// Возвращает список всех шаблонов сечений в проекте
    /// </summary>
    public List<ViewSection> GetSectionViewTemplates() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewSection))
        .WhereElementIsNotElementType()
        .OfType<ViewSection>()
        .Where(v => v.IsTemplate == true)
        .OrderBy(a => a.Name)
        .ToList();
}

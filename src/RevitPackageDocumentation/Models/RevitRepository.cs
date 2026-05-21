using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    private readonly IMessageBoxService _messageBoxService;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication, IMessageBoxService messageBoxService) {
        UIApplication = uiApplication;
        _messageBoxService = messageBoxService;

        StructuralPlanViewTypes = GetStructuralPlanViewTypes();
        SectionViewTypes = GetSectionViewTypes();
        PlanViewTemplates = GetPlanViewTemplates();
        SectionViewTemplates = GetSectionViewTemplates();
        ViewportTypes = GetViewportTypes();
        TextNoteTypes = GetTextNoteTypes();
        GenericAnnotationFamilies = GetGenericAnnotationFamilies();
        LegendsInProject = GetLegendsInProject();
        TitleBlockFamilies = GetTitleBlockFamilies();
        Sheets = GetSheets();
        Views = GetViews();
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


    public List<ViewFamilyType> StructuralPlanViewTypes { get; }
    public List<ViewFamilyType> SectionViewTypes { get; }
    public List<ViewPlan> PlanViewTemplates { get; }
    public List<ViewSection> SectionViewTemplates { get; }
    public List<ElementType> ViewportTypes { get; }
    public List<TextNoteType> TextNoteTypes { get; }
    public List<Family> GenericAnnotationFamilies { get; }
    public List<View> LegendsInProject { get; }
    public List<Family> TitleBlockFamilies { get; }
    public List<ViewSheet> Sheets { get; set; }
    public List<ViewPlan> Views { get; set; }


    /// <summary>
    /// Возвращает список типоразмеров видов в плане в проекте
    /// </summary>
    private List<ViewFamilyType> GetStructuralPlanViewTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewFamilyType))
        .OfType<ViewFamilyType>()
        .Where(a => ViewFamily.StructuralPlan == a.ViewFamily)
        .ToList();

    /// <summary>
    /// Возвращает список типоразмеров видов в разрезе в проекте
    /// </summary>
    private List<ViewFamilyType> GetSectionViewTypes() => new FilteredElementCollector(Document)
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

    /// <summary>
    /// Возвращает список всех типов видовых экранов в проекте
    /// </summary>
    public List<ElementType> GetViewportTypes() {
        var viewport = new FilteredElementCollector(Document)
            .OfClass(typeof(Viewport))
            .OfType<Viewport>()
            .FirstOrDefault();

        if(viewport is null) {
            _messageBoxService.Show("Для корректной работы плагина разместите любой один вид на любом листе!", "Ошибка!");
            return [];
        } else {
            return viewport.GetValidTypes()
                .Select(id => Document.GetElement(id) as ElementType)
                .OrderBy(a => a.Name)
                .ToList();
        }
    }

    /// <summary>
    /// Возвращает список всех типов текста в проекте
    /// </summary>
    public List<TextNoteType> GetTextNoteTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(TextNoteType))
        .OfType<TextNoteType>()
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список типовых аннотаций в проекте
    /// </summary>
    public List<Family> GetGenericAnnotationFamilies() => new FilteredElementCollector(Document)
        .OfClass(typeof(Family))
        .OfType<Family>()
        .Where(f => f.FamilyCategory.GetBuiltInCategory() == BuiltInCategory.OST_GenericAnnotation)
        .OrderBy(a => a.Name)
        .ToList();


    /// <summary>
    /// Возвращает список всех легенд, присутствующих в проекте
    /// </summary>
    public List<View> GetLegendsInProject() => new FilteredElementCollector(Document)
        .OfClass(typeof(View))
        .OfType<View>()
        .Where(view => view.ViewType == ViewType.Legend)
        .ToList();

    /// <summary>
    /// Возвращает список семейств рамок листа
    /// </summary>
    public List<Family> GetTitleBlockFamilies() => new FilteredElementCollector(Document)
        .OfClass(typeof(Family))
        .OfType<Family>()
        .Where(f => f.FamilyCategory.GetBuiltInCategory() == BuiltInCategory.OST_TitleBlocks)
        .OrderBy(a => a.Name)
        .ToList();

    public List<ViewSheet> GetSheets() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewSheet))
        .OfType<ViewSheet>()
        .ToList();

    public List<ViewPlan> GetViews() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewPlan))
        .OfType<ViewPlan>()
        .ToList();


    public ViewSheet GetSheetByName(string sheetName) {
        return Sheets
            .FirstOrDefault(o => o.Name.Equals(sheetName));
    }

    public ViewPlan GetViewByName(string viewName) {
        return Views
            .FirstOrDefault(o => o.Name.Equals(viewName));
    }

    public FamilyInstance GetTitleBlocks(ViewSheet viewSheet) {
        return new FilteredElementCollector(Document, viewSheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsNotElementType()
            .FirstOrDefault() as FamilyInstance;
    }
}

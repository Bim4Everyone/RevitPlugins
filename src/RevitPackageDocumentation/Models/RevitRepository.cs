using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPackageDocumentation.Models.ScheduleFilters;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// Списки элементов для выпадающих списков окна собираются один раз при создании репозитория.
/// Поиск элементов при генерации (Find*, GetFamilySymbols) выполняется по текущему состоянию документа,
/// поэтому учитывает элементы, созданные или загруженные в этом запуске.
/// </remarks>
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Сервис локализации (названия условий фильтров спецификаций).</param>
    public RevitRepository(
        UIApplication uiApplication,
        ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;

        PlanViewTypes = GetFloorPlanViewTypes();
        SectionViewTypes = GetSectionViewTypes();
        PlanViewTemplates = GetPlanViewTemplates();
        PlanViewsForDuplication = GetPlanViewsForDuplication();
        SectionViewTemplates = GetSectionViewTemplates();
        ViewportTypes = GetViewportTypes();
        TextNoteTypes = GetTextNoteTypes();
        LegendsInProject = GetLegendsInProject();
        TitleBlockFamilies = GetTitleBlockFamilies();
        Specs = GetSpecs();
        FilterTypes = GetFilterTypes();
        WorksetParamId = new ElementId(BuiltInParameter.ELEM_PARTITION_PARAM);
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
    /// Типоразмеры видов в плане этажа
    /// </summary>
    public List<ViewFamilyType> PlanViewTypes { get; }

    /// <summary>
    /// Типоразмеры разрезов
    /// </summary>
    public List<ViewFamilyType> SectionViewTypes { get; }

    /// <summary>
    /// Шаблоны видов в плане
    /// </summary>
    public List<ViewPlan> PlanViewTemplates { get; }

    /// <summary>
    /// Виды в плане, которые можно дублировать с детализацией
    /// </summary>
    public List<ViewPlan> PlanViewsForDuplication { get; }

    /// <summary>
    /// Шаблоны разрезов
    /// </summary>
    public List<ViewSection> SectionViewTemplates { get; }

    /// <summary>
    /// Типоразмеры видовых экранов
    /// </summary>
    public List<ElementType> ViewportTypes { get; }

    /// <summary>
    /// Типоразмеры текста
    /// </summary>
    public List<TextNoteType> TextNoteTypes { get; }

    /// <summary>
    /// Легенды проекта
    /// </summary>
    public List<View> LegendsInProject { get; }

    /// <summary>
    /// Семейства рамок листа
    /// </summary>
    public List<Family> TitleBlockFamilies { get; }

    /// <summary>
    /// Спецификации проекта
    /// </summary>
    public List<ViewSchedule> Specs { get; }

    /// <summary>
    /// Условия фильтров спецификаций, доступные в окне, с локализованными названиями
    /// </summary>
    public List<ScheduleTypeInfo> FilterTypes { get; }

    /// <summary>
    /// Id встроенного параметра "Рабочий набор"
    /// </summary>
    public ElementId WorksetParamId { get; }

    /// <summary>
    /// Возвращает список типоразмеров видов в плане в проекте
    /// </summary>
    private List<ViewFamilyType> GetFloorPlanViewTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewFamilyType))
        .OfType<ViewFamilyType>()
        .Where(a => ViewFamily.FloorPlan == a.ViewFamily)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список видов в плане проекта (кроме шаблонов), которые можно копировать с детализацией
    /// </summary>
    private List<ViewPlan> GetPlanViewsForDuplication() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewPlan))
        .WhereElementIsNotElementType()
        .OfType<ViewPlan>()
        .Where(v => !v.IsTemplate && v.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing))
        .OrderBy(v => v.Name)
        .ToList();

    /// <summary>
    /// Возвращает список типоразмеров видов в разрезе в проекте
    /// </summary>
    private List<ViewFamilyType> GetSectionViewTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewFamilyType))
        .OfType<ViewFamilyType>()
        .Where(a => ViewFamily.Section == a.ViewFamily)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список всех шаблонов планов в проекте
    /// </summary>
    private List<ViewPlan> GetPlanViewTemplates() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewPlan))
        .WhereElementIsNotElementType()
        .OfType<ViewPlan>()
        .Where(v => v.IsTemplate == true)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список всех шаблонов сечений в проекте
    /// </summary>
    private List<ViewSection> GetSectionViewTemplates() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewSection))
        .WhereElementIsNotElementType()
        .OfType<ViewSection>()
        .Where(v => v.IsTemplate == true)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список всех типов видовых экранов в проекте
    /// </summary>
    /// <remarks>
    /// Типы видовых экранов берутся как типы, на которые можно заменить тип по умолчанию (GetSimilarTypes),
    /// без перебора всех типоразмеров проекта
    /// </remarks>
    private List<ElementType> GetViewportTypes() {
        if(Document.GetElement(Document.GetDefaultElementTypeId(ElementTypeGroup.ViewportType))
           is not ElementType defaultElementType) {
            return [];
        }

        return defaultElementType.GetSimilarTypes()
            .Append(defaultElementType.Id)
            .Distinct()
            .Select(id => Document.GetElement(id) as ElementType)
            .Where(type => type != null && type.FamilyName == defaultElementType.FamilyName)
            .OrderBy(a => a.Name)
            .ToList();
    }

    /// <summary>
    /// Возвращает список всех типов текста в проекте
    /// </summary>
    private List<TextNoteType> GetTextNoteTypes() => new FilteredElementCollector(Document)
        .OfClass(typeof(TextNoteType))
        .OfType<TextNoteType>()
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список типоразмеров семейств указанной категории в проекте.
    /// Поиск выполняется напрямую в документе, т.к. семейство могло быть загружено после запуска плагина
    /// </summary>
    public List<FamilySymbol> GetFamilySymbols(BuiltInCategory builtInCategory) => new FilteredElementCollector(Document)
        .OfCategory(builtInCategory)
        .WhereElementIsElementType()
        .OfType<FamilySymbol>()
        .OrderBy(a => a.FamilyName)
        .ThenBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает типоразмер семейства указанной категории по имени семейства и типоразмера
    /// </summary>
    public FamilySymbol GetFamilySymbol(BuiltInCategory builtInCategory, string familyName, string typeName) =>
        GetFamilySymbols(builtInCategory)
            .FirstOrDefault(s => s.FamilyName.Equals(familyName) && s.Name.Equals(typeName));

    /// <summary>
    /// Возвращает локализованное имя категории в текущей версии Revit
    /// </summary>
    public string GetCategoryName(BuiltInCategory builtInCategory) =>
        Category.GetCategory(Document, builtInCategory)?.Name ?? builtInCategory.ToString();

    /// <summary>
    /// Возвращает семейство по имени
    /// </summary>
    public Family GetFamilyByName(string familyName) => new FilteredElementCollector(Document)
        .OfClass(typeof(Family))
        .OfType<Family>()
        .FirstOrDefault(f => f.Name.Equals(familyName));

    /// <summary>
    /// Возвращает список всех легенд, присутствующих в проекте
    /// </summary>
    private List<View> GetLegendsInProject() => new FilteredElementCollector(Document)
        .OfClass(typeof(View))
        .OfType<View>()
        .Where(view => view.ViewType == ViewType.Legend)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список семейств рамок листа
    /// </summary>
    private List<Family> GetTitleBlockFamilies() => new FilteredElementCollector(Document)
        .OfClass(typeof(Family))
        .OfType<Family>()
        .Where(f => f.FamilyCategory.GetBuiltInCategory() == BuiltInCategory.OST_TitleBlocks)
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает список спецификаций проекта
    /// </summary>
    private List<ViewSchedule> GetSpecs() => new FilteredElementCollector(Document)
        .OfClass(typeof(ViewSchedule))
        .OfType<ViewSchedule>()
        .OrderBy(a => a.Name)
        .ToList();

    /// <summary>
    /// Возвращает условия фильтров спецификаций, кроме служебных и связанных с глобальными параметрами
    /// </summary>
    private List<ScheduleTypeInfo> GetFilterTypes() => Enum.GetValues(typeof(ScheduleFilterType))
            .Cast<ScheduleFilterType>()
            .Where(s => s != ScheduleFilterType.Invalid)
            .Where(s => s != ScheduleFilterType.HasParameter)
            .Where(s => s != ScheduleFilterType.IsAssociatedWithGlobalParameter)
            .Where(s => s != ScheduleFilterType.IsNotAssociatedWithGlobalParameter)
            .Select(s =>
                new ScheduleTypeInfo(s, _localizationService.GetLocalizedString($"ScheduleFilterType.{s}") ?? s.ToString()))
            .ToList();

    /// <summary>
    /// Ищет в документе лист для доукомплектования. Если номер задан, лист должен совпадать и по имени, и по номеру,
    /// иначе ищется только по имени. Поиск выполняется по текущему состоянию документа,
    /// поэтому находит и листы, созданные в этом запуске
    /// </summary>
    /// <returns>Найденный лист или null</returns>
    public ViewSheet FindSheet(string sheetName, string sheetNumber) {
        bool checkNumber = !string.IsNullOrWhiteSpace(sheetNumber);
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSheet))
            .OfType<ViewSheet>()
            .FirstOrDefault(s => s.Name.Equals(sheetName)
                && (!checkNumber || s.SheetNumber.Equals(sheetNumber)));
    }

    /// <summary>
    /// Проверяет, является ли поле спецификации параметром "Рабочий набор"
    /// </summary>
    public bool IsWorksetField(ScheduleDefinition definition, ScheduleFieldId fieldId) {
        var paramId = definition.GetField(fieldId).ParameterId;
#if REVIT_2024_OR_GREATER
        return paramId.Value == WorksetParamId.Value;
#else
        return paramId.IntegerValue == WorksetParamId.IntegerValue;
#endif
    }

    /// <summary>
    /// Ищет пользовательский рабочий набор с точно таким именем
    /// </summary>
    /// <returns>Найденный рабочий набор или null</returns>
    public Workset FindWorksetByName(string worksetName) {
        if(string.IsNullOrEmpty(worksetName)) {
            return null;
        }
        return new FilteredWorksetCollector(Document)
            .OfKind(WorksetKind.UserWorkset)
            .FirstOrDefault(w => w.Name.Equals(worksetName));
    }

    /// <summary>
    /// Возвращает имя рабочего набора по его id
    /// </summary>
    /// <returns>Имя рабочего набора или null, если набор не найден</returns>
    public string GetWorksetName(int worksetId) {
        return new FilteredWorksetCollector(Document)
            .OfKind(WorksetKind.UserWorkset)
            .FirstOrDefault(w => w.Id.IntegerValue == worksetId)
            ?.Name;
    }

    /// <summary>
    /// Ищет в документе вид указанного класса с указанным именем (кроме шаблонов, листов и спецификаций).
    /// Поиск выполняется по текущему состоянию документа, поэтому находит и виды, созданные в этом запуске
    /// </summary>
    /// <param name="viewName">Имя вида</param>
    /// <param name="viewFamily">
    /// Семейство вида. Revit допускает одинаковые имена у видов разных типов (например, у плана и разреза),
    /// поэтому, если семейство задано, ищется вид только этого семейства
    /// </param>
    /// <returns>Найденный вид или null</returns>
    public T FindViewByName<T>(string viewName, ViewFamily? viewFamily = null) where T : View {
        if(string.IsNullOrEmpty(viewName)) {
            return null;
        }
        return new FilteredElementCollector(Document)
            .OfClass(typeof(T))
            .OfType<T>()
            .FirstOrDefault(v => !v.IsTemplate
                && v is not ViewSheet
                && v is not ViewSchedule
                && v.Name.Equals(viewName)
                && (viewFamily is null || GetViewFamily(v) == viewFamily));
    }

    /// <summary>
    /// Ищет в документе план на указанном уровне, на котором можно создать фрагмент указанного типоразмера:
    /// не шаблон, не фрагмент, не зависимый вид. По правилам Revit фрагмент создается на плане этажа, плане
    /// несущих конструкций или плане потолка; типоразмер семейства «Узел» допустим на любом из них, кроме плана
    /// потолка, остальные - только на виде своего семейства. Если подходящих видов несколько, возвращается первый по имени
    /// </summary>
    /// <returns>Найденный план или null</returns>
    public ViewPlan FindPlanViewOnLevel(ElementId levelId, ViewFamilyType calloutType) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))
            .OfType<ViewPlan>()
            .Where(v => !v.IsTemplate
                && !v.IsCallout
                && v.GetPrimaryViewId() == ElementId.InvalidElementId
                && v.GenLevel != null
                && v.GenLevel.Id == levelId)
            .Where(v => IsCalloutAllowed(v, calloutType))
            .OrderBy(v => v.Name)
            .FirstOrDefault();
    }

    /// <summary>
    /// Возвращает семейство вида (по его типоразмеру) или null
    /// </summary>
    public ViewFamily? GetViewFamily(View view) {
        return view is null ? null : (Document.GetElement(view.GetTypeId()) as ViewFamilyType)?.ViewFamily;
    }

    private bool IsCalloutAllowed(ViewPlan parentView, ViewFamilyType calloutType) {
        return IsCalloutAllowed(GetViewFamily(parentView), calloutType);
    }

    /// <summary>
    /// Проверяет, можно ли создать на виде указанного семейства фрагмент указанного типоразмера
    /// (правила ViewSection.CreateCallout)
    /// </summary>
    public bool IsCalloutAllowed(ViewFamily? parentFamily, ViewFamilyType calloutType) {
        if(parentFamily is not (ViewFamily.FloorPlan or ViewFamily.StructuralPlan or ViewFamily.CeilingPlan)) {
            return false;
        }
        if(calloutType is null) {
            return true;
        }
        return calloutType.ViewFamily == ViewFamily.Detail
            ? parentFamily != ViewFamily.CeilingPlan
            : parentFamily == calloutType.ViewFamily;
    }

    /// <summary>
    /// Возвращает спецификацию по имени из списка, собранного при запуске
    /// </summary>
    internal ViewSchedule GetSpecByName(string viewName) {
        return Specs
            .FirstOrDefault(o => o.Name.Equals(viewName));
    }

    /// <summary>
    /// Ищет в документе спецификацию (не шаблон) с указанным именем. Поиск выполняется по текущему состоянию
    /// документа, поэтому находит и спецификации, созданные в этом запуске
    /// </summary>
    /// <returns>Найденная спецификация или null</returns>
    public ViewSchedule FindScheduleByName(string viewName) {
        if(string.IsNullOrEmpty(viewName)) {
            return null;
        }
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .OfType<ViewSchedule>()
            .FirstOrDefault(v => !v.IsTemplate && v.Name.Equals(viewName));
    }

    /// <summary>
    /// Возвращает рамку листа (первый экземпляр категории "Основные надписи" на листе) или null
    /// </summary>
    public FamilyInstance GetTitleBlock(ViewSheet viewSheet) {
        return new FilteredElementCollector(Document, viewSheet.Id)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsNotElementType()
            .FirstOrDefault() as FamilyInstance;
    }

    /// <summary>
    /// Возвращает видовые экраны листа
    /// </summary>
    public List<Viewport> GetViewports(ViewSheet viewSheet) {
        return viewSheet.GetAllViewports()
            .Select(id => Document.GetElement(id) as Viewport)
            .ToList();
    }

    /// <summary>
    /// Возвращает спецификации, размещенные на листе
    /// </summary>
    public List<ScheduleSheetInstance> GetScheduleSheetInstances(ViewSheet viewSheet) {
        return new FilteredElementCollector(Document, viewSheet.Id)
            .OfClass(typeof(ScheduleSheetInstance))
            .WhereElementIsNotElementType()
            .Cast<ScheduleSheetInstance>()
            .ToList();
    }

    /// <summary>
    /// Возвращает текстовые примечания листа
    /// </summary>
    public List<TextNote> GetTextNotes(ViewSheet viewSheet) {
        return new FilteredElementCollector(Document, viewSheet.Id)
            .OfClass(typeof(TextNote))
            .WhereElementIsNotElementType()
            .Cast<TextNote>()
            .ToList();
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models;

internal class RevitRepository {
    private const string _defaultScheduleName = "ВС_Ведомость спецификаций";
    private readonly ILocalizationService _localizationService;
    private readonly ParamFactory _paramFactory;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService, ParamFactory paramFactory) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _paramFactory = paramFactory;
        SheetElements = GetSheetElements(Document);
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    public IList<SheetElement> SheetElements { get; }

    // Метод получения типа "Связанный файл"
    public IList<LinkTypeElement> GetLinkTypeElements() {
        var listRevitLinkTypes = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .Where(linkType => !linkType.IsNestedLink)
            .ToList();
        return new Collection<LinkTypeElement>(listRevitLinkTypes
            .Select(linkType => new LinkTypeElement(linkType))
            .ToList());
    }

    // Метод получения документа "Связанный файл"
    public Document GetLinkDocument(ElementId id) {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(instance => instance.GetTypeId() == id)
            .First()
            .GetLinkDocument();
    }

    // Метод получения списка SheetElement по Document (основной или связанный)
    public IList<SheetElement> GetSheetElements(Document document) {
        return new Collection<SheetElement>(GetViewSheets(document)
            .Select(sheet => new SheetElement(_paramFactory, sheet))
            .OrderBy(sheet => Convert.ToInt32(sheet.Number))
            .ToList());
    }

    // Метод получения списка ViewSheet по Document (основной или связанный)
    public IList<ViewSheet> GetViewSheets(Document document) {
        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Sheets)
            .Cast<ViewSheet>()
            .ToList();
    }

    // Метод получения чертежного вида, существующего или создание нового
    public ViewDrafting GetViewDrafting(string familyName) {
        string nameView = string.Format(
            _localizationService.GetLocalizedString("RevitRepository.ViewName"), familyName);
        var view = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewDrafting))
            .Where(v => v.Name.Equals(nameView, StringComparison.OrdinalIgnoreCase))
            .Cast<ViewDrafting>()
            .FirstOrDefault();
        return view ?? CreateViewDrafting(nameView);
    }

    // Метод получения экземпляров спецификация с листа
    public IList<ViewSchedule> GetScheduleInstances(Document document, ViewSheet viewSheet) {
        if(document == null || viewSheet == null) {
            return null;
        }
        var scheduleInstances = new FilteredElementCollector(document)
            .OfClass(typeof(ScheduleSheetInstance))
            .Cast<ScheduleSheetInstance>()
            .Where(schedule => schedule.OwnerViewId == viewSheet.Id)
            .Where(schedule => schedule.SegmentIndex is 0 or (-1))
            .ToList();
        return !scheduleInstances.Any()
            ? null
            : (IList<ViewSchedule>) scheduleInstances
            .Select(element => document.GetElement(element.ScheduleId))
            .OfType<ViewSchedule>()
            .ToList();
    }

    // Метод получения типа семейства
    public FamilySymbol GetFamilySymbol(Family family) {
        ElementFilter filter = new FamilySymbolFilter(family.Id);
        return new FilteredElementCollector(Document)
            .WherePasses(filter)
            .Cast<FamilySymbol>()
            .First();
    }

    // Метод удаления экземпляров семейства с вида
    public void DeleteFamilyInstances(View view) {
        var instances = new FilteredElementCollector(Document, view.Id)
            .OfType<FamilyInstance>()
            .Select(instance => instance.Id)
            .ToList();
        if(instances.Any()) {
            Document.Delete(instances);
        }
    }

    // Метод создания новой спецификации
    public void CreateSchedule(string newScheduleName) {
        if(!CheckSchedule(newScheduleName)) {
            _ = new ScheduleElement(this, _defaultScheduleName, newScheduleName);
        }
    }

    // Метод поиска и получения спецификации по имени
    public ViewSchedule GetSchedule(string scheduleName) {
        var cache = new ViewScheduleCache(Document);
        return cache.ExistViewSchedule(scheduleName);
    }

    // Метод проверки наличия нужной спецификации в проекте
    private bool CheckSchedule(string scheduleName) {
        var schedule = GetSchedule(scheduleName);
        return schedule != null;
    }

    // Метод создания нового чертежного вида
    private ViewDrafting CreateViewDrafting(string nameView) {
        var viewTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>();
        var viewFamilyType = viewTypes
            .Where(vt => vt.ViewFamily == ViewFamily.Drafting)
            .First();

        var view = ViewDrafting.Create(Document, viewFamilyType.Id);
        view.Name = nameView;
        return view;
    }
}

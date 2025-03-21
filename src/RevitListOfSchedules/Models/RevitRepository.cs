using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly ParamFactory _paramFactory;
    private readonly IList<SheetElement> _sheetElements;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService, ParamFactory paramFactory) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _paramFactory = paramFactory;
        _sheetElements = GetSheetElements(Document);
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    public IList<SheetElement> SheetElements => _sheetElements;

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
    public Document GetLinkDocument(LinkViewModel linkViewModel) {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(instance => instance.GetTypeId() == linkViewModel.Id)
            .First()
            .GetLinkDocument();
    }

    // Метод получения списка SheetElement по Document (основной или связанный)
    public IList<SheetElement> GetSheetElements(Document document) {
        return new Collection<SheetElement>(GetViewSheets(document)
            .Select(sheet => new SheetElement(sheet, _paramFactory))
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

    // Метод получения списка всех параметров листа и браузера проекта
    public List<RevitParam> GetGroupParameters() {

        ElementId sheetCategoryId = new ElementId(BuiltInCategory.OST_Sheets);
        ICollection<ElementId> categories = new List<ElementId> { sheetCategoryId };
        var filterableParameters = ParameterFilterUtilities.GetFilterableParametersInCommon(Document, categories);

        List<RevitParam> listOfParameters = new List<RevitParam>();
        List<RevitParam> viewParams = GetRevitParams(filterableParameters.ToList());

        if(viewParams.Count > 0) {
            List<RevitParam> sortedList = viewParams.OrderBy(param => param.Name).ToList();
            listOfParameters.AddRange(sortedList);
            List<ElementId> browserParamElementIds = GetBrowserParameterElementIds();
            if(browserParamElementIds.Count > 0) {
                RevitParam browserParam = GetRevitParams(browserParamElementIds).Last();
                int index = listOfParameters.FindIndex(param => param.Equals(browserParam));
                if(index != -1) {
                    listOfParameters.RemoveAt(index);
                    listOfParameters.Insert(0, browserParam);
                }
            }
        }
        return listOfParameters;
    }

    private List<ElementId> GetBrowserParameterElementIds() {
        List<ElementId> listOfElementIds = new List<ElementId>();
        var projectSheets = GetViewSheets(Document);
        if(projectSheets.Count > 0) {
            var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(Document);
            IList<FolderItemInfo> itemsInfo = browserOrganization.GetFolderItems(projectSheets.First().Id);
            foreach(var item in itemsInfo) {
                listOfElementIds.Add(item.ElementId);
            }
        }
        return listOfElementIds;
    }




    private List<RevitParam> GetRevitParams(List<ElementId> elementIds) {
        List<RevitParam> listOfParameters = new List<RevitParam>();
        if(elementIds.Count > 0) {
            foreach(ElementId elementId in elementIds) {
                if(elementId.IsSystemId()) {
                    BuiltInParameter param = elementId.AsBuiltInParameter();
                    RevitParam revitParam = SystemParamsConfig.Instance.CreateRevitParam(Document, param);
                    if(revitParam.StorageType == StorageType.String) {
                        listOfParameters.Add(revitParam);
                    }
                } else {
                    Element element = Document.GetElement(elementId);
                    if(element is SharedParameterElement) {
                        RevitParam revitParam = SharedParamsConfig.Instance.CreateRevitParam(Document, element.Name);
                        if(revitParam.StorageType == StorageType.String) {
                            listOfParameters.Add(revitParam);
                        }
                    } else {
                        RevitParam revitParam = ProjectParamsConfig.Instance.CreateRevitParam(Document, element.Name);
                        if(revitParam.StorageType == StorageType.String) {
                            listOfParameters.Add(revitParam);
                        }
                    }
                }
            }
        }
        return listOfParameters;
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
        if(view != null) {
            return view;
        } else {
            return CreateViewDrafting(nameView);
        }
    }

    // Метод создания нового чертежного вида
    private ViewDrafting CreateViewDrafting(string nameView) {
        var viewTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>();
        var viewFamilyType = viewTypes
            .Where(vt => vt.ViewFamily == ViewFamily.Drafting)
            .First();
        string transactionName = _localizationService.GetLocalizedString("RevitRepository.TransactionNameCreate");
        using(Transaction t = Document.StartTransaction(transactionName)) {
            ViewDrafting view = ViewDrafting.Create(Document, viewFamilyType.Id);
            view.Name = nameView;
            t.Commit();
            return view;
        }
    }

    public IList<ViewSchedule> GetScheduleInstances(Document document, ViewSheet viewSheet) {
        if(document == null || viewSheet == null) {
            return null;
        }
        var scheduleInstances = new FilteredElementCollector(document)
            .OfClass(typeof(ScheduleSheetInstance))
            .Cast<ScheduleSheetInstance>()
            .Where(schedule => schedule.OwnerViewId == viewSheet.Id)
            .Where(schedule => schedule.SegmentIndex == 0 || schedule.SegmentIndex == -1)
            .ToList();
        if(!scheduleInstances.Any()) {
            return null;
        }
        return scheduleInstances
            .Select(element => document.GetElement(element.ScheduleId))
            .OfType<ViewSchedule>()
            .ToList();
    }

    public FamilySymbol GetFamilySymbol(Family family) {
        ElementFilter filter = new FamilySymbolFilter(family.Id);
        return new FilteredElementCollector(Document)
            .WherePasses(filter)
            .Cast<FamilySymbol>()
            .First();
    }

    public void DeleteFamilyInstance(View view) {
        var instances = new FilteredElementCollector(Document, view.Id)
            .OfType<FamilyInstance>()
            .Select(instance => instance.Id)
            .ToList();
        string transactionName = _localizationService.GetLocalizedString("RevitRepository.TransactionNameDelete");
        using(Transaction t = Document.StartTransaction(transactionName)) {
            foreach(var instance in instances) {
                Document.Delete(instance);
            }
            t.Commit();
        }
    }

    public bool IsExistView(string name) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .Where(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Cast<ViewSchedule>()
            .ToList()
            .Count > 0;
    }

    public string LegalizeString(string original) {
        char[] invalidChars = Path.GetInvalidPathChars()
           .Concat(Path.GetInvalidFileNameChars())
           .Distinct()
           .ToArray();
        return new(original
            .Select(c => invalidChars.Contains(c) ? '_' : c)
            .ToArray());
    }
}

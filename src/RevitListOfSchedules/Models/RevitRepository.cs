using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Models;

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

    public Document GetLinkDocument(LinkViewModel linkViewModel) {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(instance => instance.GetTypeId() == linkViewModel.Id)
            .First()
            .GetLinkDocument();
    }

    public IList<SheetElement> GetSheetElements(Document document) {
        return new Collection<SheetElement>(GetViewSheets(document)
            .Select(sheet => new SheetElement(sheet))
            .OrderBy(sheet => Convert.ToInt32(sheet.Number))
            .ToList());
    }

    public IList<ViewSheet> GetViewSheets(Document document) {
        return new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Sheets)
            .Cast<ViewSheet>()
            .ToList();
    }

    public IList<Parameter> GetBrowserParameters(ViewSheet viewSheet) {

        IList<Parameter> listOfParameters = new List<Parameter>();
        if(viewSheet != null) {
            var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(Document);
            IList<FolderItemInfo> itemsInfo = browserOrganization.GetFolderItems(viewSheet.Id);
            foreach(FolderItemInfo itemInfo in itemsInfo) {
                foreach(Parameter parameter in viewSheet.Parameters) {
                    if(itemInfo.ElementId == parameter.Id) {
                        listOfParameters.Add(parameter);
                    }
                }
            }
        }
        return listOfParameters;
    }




}



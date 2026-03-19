using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCorrectNamingCheck.Services;

namespace RevitCorrectNamingCheck.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
//bimModelPartsService
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>

    public RevitRepository(UIApplication uiApplication,
        ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
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

    private List<RevitLinkType> GetRevitLinkTypes() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .ToList();
    }

    private List<RevitLinkInstance> GetRevitLinkInstances() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .WhereElementIsNotElementType()
            .Cast<RevitLinkInstance>()
            .ToList();
    }

    private WorksetInfo GetWorksetInfo(Element element, Dictionary<WorksetId, string> worksets) {
        string undefinedName = _localizationService.GetLocalizedString("MainWindow.Undefined");

        int? worksetId = element.GetParamValueOrDefault<int?>(BuiltInParameter.ELEM_PARTITION_PARAM);
        if(worksetId is null) {
            return new WorksetInfo(element.WorksetId, undefinedName);
        }

        bool success = worksets.TryGetValue(new WorksetId(worksetId.Value), out string name);
        return new WorksetInfo(element.WorksetId, success ? name : undefinedName);
    }

    public Dictionary<WorksetId, string> GetUserWorksets() {
        return WorksharingUtils.GetUserWorksetInfo(
                Document.GetWorksharingCentralModelPath()
                ?? ModelPathUtils.ConvertUserVisiblePathToModelPath(Document.PathName))
            .ToDictionary(p => p.Id, p => p.Name);
    }

    public List<LinkedFile> GetLinkedFiles() {
        var result = new List<LinkedFile>();

        var linkTypes = GetRevitLinkTypes();
        var linkInstances = GetRevitLinkInstances();
        var worksets = GetUserWorksets();
        foreach(var linkType in linkTypes) {
            var instances = linkInstances.Where(x => x.GetTypeId() == linkType.Id);

            foreach(var instance in instances) {
                var typeWorkset = GetWorksetInfo(linkType, worksets);
                var instanceWorkset = GetWorksetInfo(instance, worksets);
                var linkedFile = new LinkedFile(instance, typeWorkset, instanceWorkset);

                result.Add(linkedFile);
            }
        }
        return result;
    }

    public void UpdateLinkedFile(LinkedFile link, WorksetId typeWorkset, WorksetId instanceWorkset, bool isPinned) {
        if(link.TypeWorkset.Id != typeWorkset) {
            link.Instance.GetElementType()
                .SetParamValue(BuiltInParameter.ELEM_PARTITION_PARAM, typeWorkset.IntegerValue);
        }

        if(link.InstanceWorkset.Id != instanceWorkset) {
            link.Instance.SetParamValue(BuiltInParameter.ELEM_PARTITION_PARAM, instanceWorkset.IntegerValue);
        }

        if(link.IsPinned != isPinned) {
            link.Instance.Pinned = isPinned;
        }
    }
}

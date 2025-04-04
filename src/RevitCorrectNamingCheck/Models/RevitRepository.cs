using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCorrectNamingCheck.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
//bimModelPartsService
internal class RevitRepository {
    private readonly IBimModelPartsService _bimModelPartsService;
    private readonly ILocalizationService _localizationService;
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>

    public RevitRepository(UIApplication uiApplication,
        IBimModelPartsService bimModelPartsService,
        ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _bimModelPartsService = bimModelPartsService;
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

    public List<LinkedFile> GetLinkedFiles() {
        var result = new List<LinkedFile>();
        var linkedFileEnricher = new LinkedFileEnricher(_bimModelPartsService);

        var linkTypes = GetRevitLinkTypes();
        var linkInstances = GetRevitLinkInstances();

        foreach(var linkType in linkTypes) {
            var instances = linkInstances.Where(x => x.GetTypeId() == linkType.Id);

            foreach(var instance in instances) {
                var typeWorkset = new WorksetInfo(linkType.WorksetId, GetWorksetName(linkType));
                var instanceWorkset = new WorksetInfo(instance.WorksetId, GetWorksetName(instance));
                var linkedFile = new LinkedFile(linkType.Id, linkType.Name, typeWorkset, instanceWorkset);

                linkedFileEnricher.Enrich(linkedFile);
                result.Add(linkedFile);
            }
        }
        return result;
    }

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

    private string GetWorksetName(Element element) {
        var undefinedText = _localizationService.GetLocalizedString("MainWindow.Undefined");

        var param = element.GetParam(BuiltInParameter.ELEM_PARTITION_PARAM);
        if(param == null) {
            return undefinedText;
        }

        int worksetId = param.AsInteger();
        var workset = Document.GetWorksetTable().GetWorkset(new WorksetId(worksetId));
        return workset?.Name ?? undefinedText;
    }
}

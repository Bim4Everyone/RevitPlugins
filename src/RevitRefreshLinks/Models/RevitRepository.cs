using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitRefreshLinks.Models;
internal class RevitRepository {
    private readonly RevitLinkOptions _revitLinkOptionsLocal
        = new(true, new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets));
    private readonly RevitLinkOptions _revitLinkOptionsServer
        = new(false, new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets));
    private readonly WorksetConfiguration _worksetConfiguration
        = new(WorksetConfigurationOption.OpenAllWorksets);
    private readonly ILocalizationService _localizationService;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;


    public bool AddLink(string path, out string error) {
        var modelPath = ConvertToModelPath(path);
        error = string.Empty;
        try {
            var opts = GetLinkOptions(modelPath);
            var linkLoadResult = RevitLinkType.Create(Document, modelPath, opts);
            if(!LinkLoadResult.IsCodeSuccess(linkLoadResult.LoadResult)) {
                return false;
            }
            var linkType = Document.GetElement(linkLoadResult.ElementId);
            linkType.SetParamValue(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 1);
            RevitLinkInstance.Create(Document, linkLoadResult.ElementId, ImportPlacement.Shared);
            return true;
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            error = _localizationService.GetLocalizedString("LinkLoader.Error.Coordinates");
            return false;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            error = _localizationService.GetLocalizedString("LinkLoader.Error.Version");
            return false;
        }
    }

    public bool ReloadLink(RevitLinkType link, string source, out string error) {
        var modelPath = ConvertToModelPath(source);
        error = string.Empty;
        try {
            var linkLoadResult = link.LoadFrom(modelPath, _worksetConfiguration);
            return LinkLoadResult.IsCodeSuccess(linkLoadResult.LoadResult);
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            error = _localizationService.GetLocalizedString("LinkLoader.Error.Coordinates");
            return false;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            error = _localizationService.GetLocalizedString("LinkLoader.Error.Version");
            return false;
        }
    }

    public ICollection<RevitLinkType> GetExistingLinks() {
        return new FilteredElementCollector(Document)
            .WhereElementIsElementType()
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .ToArray();
    }

    private ModelPath ConvertToModelPath(string path) {
        return string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException(nameof(path))
            : ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
    }

    private RevitLinkOptions GetLinkOptions(ModelPath modelPath) {
        return modelPath.ServerPath ? _revitLinkOptionsServer : _revitLinkOptionsLocal;
    }
}

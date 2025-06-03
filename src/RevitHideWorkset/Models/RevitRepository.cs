using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitHideWorkset.Models;

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

    public List<LinkedFileElement> GetLinkedFiles() {
        var linkedInstances = new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Where(link => link.GetLinkDocument() != null)
            .ToList();

        var result = new List<LinkedFileElement>();

        foreach(var linkInstance in linkedInstances) {
            var linkDoc = linkInstance.GetLinkDocument();

            var worksets = new FilteredWorksetCollector(linkDoc)
                .OfKind(WorksetKind.UserWorkset)
                .Select(workset => new WorksetElement {
                    Name = workset.Name,
                    IsOpen = workset.IsOpen,
                    IsChanged = false
                })
                .ToList();

            result.Add(new LinkedFileElement {
                LinkedFile = linkInstance,
                AllWorksets = worksets
            });
        }

        return result;
    }


    public List<string> ToggleWorksetVisibility(List<LinkedFileElement> linkedFiles) {
        var failedFiles = new List<string>();

        foreach(var linkedFile in linkedFiles) {
            var linkInstance = linkedFile.LinkedFile;
            var linkType = Document.GetElement(linkInstance.GetTypeId()) as RevitLinkType;

            var externalRef = linkType.GetExternalFileReference();
            var modelPath = externalRef.GetAbsolutePath();

            var allLinkWorksets = WorksharingUtils.GetUserWorksetInfo(modelPath);

            var worksetsToOpen = allLinkWorksets
                .Where(linkWs => linkedFile.AllWorksets.Count(w => w.Name == linkWs.Name && w.IsOpen) > 0)
                .Select(ws => ws.Id)
                .ToList();

            var config = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            config.Open(worksetsToOpen);

            try {
                _ = linkType.LoadFrom(modelPath, config);
            } catch(Exception) {
                failedFiles.Add(linkInstance.Name);
            }
        }
        return failedFiles;
    }
}

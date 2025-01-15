using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitRefreshLinks.Models {
    internal class RevitRepository {
        private readonly RevitLinkOptions _revitLinkOptions
            = new RevitLinkOptions(true, new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets));
        private readonly WorksetConfiguration _worksetConfiguration
            = new WorksetConfiguration(WorksetConfigurationOption.OpenAllWorksets);


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }


        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public bool AddLink(string path, out string error) {
            ModelPath modelPath = ConvertToModelPath(path);
            error = string.Empty;
            try {
                var linkLoadResult = RevitLinkType.Create(Document, modelPath, _revitLinkOptions);
                if(!LinkLoadResult.IsCodeSuccess(linkLoadResult.LoadResult)) {
                    return false;
                }
                var linkType = Document.GetElement(linkLoadResult.ElementId);
                linkType.SetParamValue(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 1);
                RevitLinkInstance.Create(Document, linkLoadResult.ElementId, ImportPlacement.Shared);
                return true;
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                error = "Файл с другой системой координат";
                return false;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                error = "Файл создан в другой версии Revit";
                return false;
            }
        }

        public bool ReloadLink(RevitLinkType link, string path, out string error) {
            ModelPath modelPath = ConvertToModelPath(path);
            error = string.Empty;
            try {
                var linkLoadResult = link.LoadFrom(modelPath, _worksetConfiguration);
                return LinkLoadResult.IsCodeSuccess(linkLoadResult.LoadResult);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                error = "Файл с другой системой координат";
                return false;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                error = "Файл создан в другой версии Revit";
                return false;
            }
        }

        public ModelPath ConvertToModelPath(string path) {
            if(string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException(nameof(path));
            }
            return ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
        }

        public ICollection<RevitLinkType> GetExistingLinks() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .ToArray();
        }
    }
}

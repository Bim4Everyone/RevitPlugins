using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyParameterAdder.Models;
internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public DefinitionFile SharedParametersFile { get; set; }


    public List<string> GetParamGroupNames() => SharedParametersFile.Groups
        .Select(item => item.Name)
        .OrderBy(item => item)
        .ToList();

    public List<ExternalDefinition> GetParamsInShPF() {
        List<ExternalDefinition> paramsInGroup = [];
        // Проходимся по каждой группе ФОП
        foreach(DefinitionGroup sharedGroup in SharedParametersFile.Groups) {
            // Проходимся по каждому параметру в группе параметров
            foreach(var item in sharedGroup.Definitions) {
                paramsInGroup.Add(item as ExternalDefinition);
            }
        }
        paramsInGroup.Sort((x, y) => string.Compare(x.Name, y.Name));
        return paramsInGroup;
    }

    internal bool IsFamilyFile() => Document.IsFamilyDocument;

    internal bool IsSharedParametersFileConnected() {
        SharedParametersFile = Application.OpenSharedParameterFile();
        return SharedParametersFile != null;
    }
}

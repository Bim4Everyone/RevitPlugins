using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyParameterAdder.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public List<string> GetParamGroupNames() => Application.OpenSharedParameterFile().Groups
            .Select(item => item.Name)
            .OrderBy(item => item)
            .ToList();


        public List<ExternalDefinition> GetParamsInShPF() {
            List<ExternalDefinition> paramsInGroup = new List<ExternalDefinition>();

            DefinitionFile sharedParametersFile = Application.OpenSharedParameterFile();
            // Проходимся по каждой группе ФОП
            foreach(DefinitionGroup sharedGroup in sharedParametersFile.Groups) {
                // Проходимся по каждому параметру в группе параметров
                foreach(var item in sharedGroup.Definitions) {
                    paramsInGroup.Add(item as ExternalDefinition);
                }
            }

            paramsInGroup.Sort((x, y) => string.Compare(x.Name, y.Name));
            return paramsInGroup;
        }

        internal bool IsFamilyFile() {
            if(Document.IsFamilyDocument) {
                return true;
            } else {
                return false;
            }
        }
    }
}
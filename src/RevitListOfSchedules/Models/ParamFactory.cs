using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

namespace RevitListOfSchedules.Models {
    internal class ParamFactory {

        public const string FamilyParamNumber = "Номер листа";
        public const string FamilyParamName = "Наименование спецификации";
        public const string FamilyParamRevision = "Примечание";
        public const string ScheduleName = "Ведомость спецификаций и ведомостей";

        private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;

        public ParamFactory() {
            SharedParamsRevision = GetRevisionParams();
            SharedParamsRevisionValue = GetRevisionValueParams();
            SharedParamNumber = _sharedParamsConfig.StampSheetNumber;
        }

        public IList<SharedParam> SharedParamsRevision { get; }
        public IList<SharedParam> SharedParamsRevisionValue { get; }
        public SharedParam SharedParamNumber { get; }


        public List<RevitParam> GetGroupParameters(RevitRepository revitRepository) {

            var sheetCategoryId = new ElementId(BuiltInCategory.OST_Sheets);
            ICollection<ElementId> categories = [sheetCategoryId];
            ICollection<ElementId> filterableParameters = ParameterFilterUtilities.GetFilterableParametersInCommon(revitRepository.Document, categories);

            List<RevitParam> listOfParameters = [];
            List<RevitParam> viewParams = GetRevitParams(filterableParameters.ToList(), revitRepository);

            if(viewParams.Count > 0) {
                var sortedList = viewParams.OrderBy(param => param.Name).ToList();
                listOfParameters.AddRange(sortedList);
                List<ElementId> browserParamElementIds = GetBrowserParameterElementIds(revitRepository);
                if(browserParamElementIds.Count > 0) {
                    RevitParam browserParam = GetRevitParams(browserParamElementIds, revitRepository).Last();
                    int index = listOfParameters.FindIndex(param => param.Equals(browserParam));
                    if(index != -1) {
                        listOfParameters.RemoveAt(index);
                        listOfParameters.Insert(0, browserParam);
                    }
                }
            }
            return listOfParameters;
        }


        private List<ElementId> GetBrowserParameterElementIds(RevitRepository revitRepository) {
            List<ElementId> listOfElementIds = [];
            IList<ViewSheet> projectSheets = revitRepository.GetViewSheets(revitRepository.Document);
            if(projectSheets.Count > 0) {
                var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(revitRepository.Document);
                IList<FolderItemInfo> itemsInfo = browserOrganization.GetFolderItems(projectSheets.First().Id);
                foreach(FolderItemInfo item in itemsInfo) {
                    listOfElementIds.Add(item.ElementId);
                }
            }
            return listOfElementIds;
        }

        private List<RevitParam> GetRevitParams(List<ElementId> elementIds, RevitRepository revitRepository) {
            List<RevitParam> listOfParameters = [];
            if(elementIds.Count > 0) {
                foreach(ElementId elementId in elementIds) {
                    if(elementId.IsSystemId()) {
                        BuiltInParameter param = elementId.AsBuiltInParameter();
                        RevitParam revitParam = SystemParamsConfig.Instance.CreateRevitParam(revitRepository.Document, param);
                        if(revitParam.StorageType == StorageType.String) {
                            listOfParameters.Add(revitParam);
                        }
                    } else {
                        Element element = revitRepository.Document.GetElement(elementId);
                        if(element is SharedParameterElement) {
                            RevitParam revitParam = SharedParamsConfig.Instance.CreateRevitParam(revitRepository.Document, element.Name);
                            if(revitParam.StorageType == StorageType.String) {
                                listOfParameters.Add(revitParam);
                            }
                        } else {
                            RevitParam revitParam = ProjectParamsConfig.Instance.CreateRevitParam(revitRepository.Document, element.Name);
                            if(revitParam.StorageType == StorageType.String) {
                                listOfParameters.Add(revitParam);
                            }
                        }
                    }
                }
            }
            return listOfParameters;
        }

        private IList<SharedParam> GetRevisionParams() {

            return [
                _sharedParamsConfig.StampSheetRevision1,
                _sharedParamsConfig.StampSheetRevision2,
                _sharedParamsConfig.StampSheetRevision3,
                _sharedParamsConfig.StampSheetRevision4,
                _sharedParamsConfig.StampSheetRevision5,
                _sharedParamsConfig.StampSheetRevision6,
                _sharedParamsConfig.StampSheetRevision7,
                _sharedParamsConfig.StampSheetRevision8
            ];
        }

        private IList<SharedParam> GetRevisionValueParams() {

            return [
                _sharedParamsConfig.StampSheetRevisionValue1,
                _sharedParamsConfig.StampSheetRevisionValue2,
                _sharedParamsConfig.StampSheetRevisionValue3,
                _sharedParamsConfig.StampSheetRevisionValue4,
                _sharedParamsConfig.StampSheetRevisionValue5,
                _sharedParamsConfig.StampSheetRevisionValue6,
                _sharedParamsConfig.StampSheetRevisionValue7,
                _sharedParamsConfig.StampSheetRevisionValue8
            ];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

namespace RevitSetLevelSection.Models {
    internal class LinkInstanceRepository {
        public static readonly string BuildingWorksBlockName = "Блок СМР_";
        public static readonly string BuildingWorksSectionName = "Секция СМР_";
        public static readonly string BuildingWorksTypingName = "Типизация СМР_";

        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;
        
        private Document _document;
        private RevitLinkInstance _linkInstance;

        public LinkInstanceRepository(RevitRepository revitRepository, RevitLinkType revitLinkType) {
            _revitRepository = revitRepository;
            _revitLinkType = revitLinkType;

            Update();
        }

        public Transform Transform { get; private set; }

        public IEnumerable<DesignOption> GetDesignOptions() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(DesignOption))
                .OfType<DesignOption>()
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetMassElements(DesignOption designOption) {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .Where(item => item.DesignOption?.Id == designOption.Id)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public bool LinkIsLoaded() {
            return _revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
        }

        public bool LoadLinkDocument() {
            if(_revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.InClosedWorkset) {
                Workset workset = _revitRepository.GetWorkset(_revitLinkType);
                TaskDialog.Show("Предупреждение!", $"Откройте рабочий набор \"{workset.Name}\"." 
                                                   + Environment.NewLine
                                                   + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!");
                
                return false;
            }
            
            var loadResult = _revitLinkType.Load();
            if(loadResult.LoadResult == LinkLoadResultType.LinkLoaded) {
                Update();
                return true;
            }

            return false;
        }

        public IEnumerable<string> GetPartNames() {
            if(_document == null) {
                yield break;
            }
            
            yield return "Без раздела";
            foreach(ParameterElement paramElement in _document.GetProjectParams().OrderBy(item => item.Name)) {
                foreach(string paramName in GetParamNames()) {
                    if(paramElement.Name.StartsWith(paramName, StringComparison.CurrentCultureIgnoreCase)) {
                        yield return paramElement.Name.Replace(paramName, string.Empty);
                    }
                }
            }
        }

        private void Update() {
            _linkInstance = _revitRepository.GetLinkInstances()
                .FirstOrDefault(item => item.GetTypeId() == _revitLinkType.Id);

            if(_linkInstance != null) {
                Transform = _linkInstance.GetTransform();
                _document = _linkInstance.GetLinkDocument();
            }
        }

        private IEnumerable<string> GetParamNames() {
            yield return BuildingWorksTypingName;
            yield return BuildingWorksSectionName;
            yield return BuildingWorksBlockName;
        }
    }
}
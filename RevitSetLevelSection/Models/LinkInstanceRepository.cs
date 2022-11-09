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
                .Where(item=> !item.IsPrimary)
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetMassElements(IDesignOption designOption) {
            // Пропускаем все элементы, которые имеют DesignOptions.IsPrimary
            // если в документе нет DesignOptions тогда элемент возвращает null
            var elements = new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .Where(item => item.DesignOption == null || item.DesignOption.IsPrimary == false);
            
            // Фиктивный DesignOption возвращает
            // идентификатор ElementId.InvalidElementId
            if(designOption.Id != ElementId.InvalidElementId) {
                elements = elements.Where(item => GetDesignOptionId(item) == designOption.Id);
            }

            return elements
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
            return GetPartNames(GetParamNames());
        }

        public IEnumerable<string> GetPartNames(IEnumerable<string> paramNames) {
            if(_document == null) {
                yield break;
            }

            yield return "Без раздела";

            foreach(string paramName in paramNames) {
                foreach(ParameterElement paramElement in _document.GetProjectParams().OrderBy(item => item.Name)) {
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

        private ElementId GetDesignOptionId(Element element) {
            return element.DesignOption?.Id ?? ElementId.InvalidElementId;
        }
    }
}
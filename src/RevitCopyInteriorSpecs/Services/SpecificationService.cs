using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitCopyInteriorSpecs.Models;

namespace RevitCopyInteriorSpecs.Services {
    internal class SpecificationService {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        public SpecificationService(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Дублирует спецификацию и задает указанное имя.
        /// Если спецификация с таким именем уже существует, выбрасывает исключение.
        /// </summary>
        public ViewSchedule DuplicateSpec(ViewSchedule viewSchedule, string newViewSpecName) {
            ViewSchedule newViewSpec = _revitRepository.GetSpecByName(newViewSpecName);
            if(newViewSpec is null) {
                newViewSpec = _revitRepository.Document.GetElement(viewSchedule.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                newViewSpec.Name = newViewSpecName;
            } else {
                throw new ArgumentException($"{_localizationService.GetLocalizedString("MainWindow.SpecificationAlreadyExists")}: " +
                    $"\"{newViewSpecName}\"");
            }
            return newViewSpec;
        }

        /// <summary>
        /// Устанавливает значения параметрам спецификации.
        /// </summary>
        public void SetSpecParams(ViewSchedule newViewSpec, ParametersOption dispatcherOption) {
            newViewSpec.SetParamValue(dispatcherOption.FirstParamName, dispatcherOption.FirstParamValue);
            newViewSpec.SetParamValue(dispatcherOption.SecondParamName, dispatcherOption.SecondParamValue);
            newViewSpec.SetParamValue(dispatcherOption.ThirdParamName, dispatcherOption.ThirdParamValue);
            newViewSpec.SetParamValue(dispatcherOption.FourthParamName, dispatcherOption.FourthParamValue);
        }

        /// <summary>
        /// Изменяет фильтр спецификации, когда он хранит string.
        /// </summary>
        public void ChangeSpecFilter(ViewSchedule spec, string specFilterName, string filterValue) {
            ChangeSpecFilter(
                spec,
                specFilterName,
                (filter) => filter.SetValue(filterValue));
        }

        /// <summary>
        /// Изменяет фильтр спецификации, когда он хранит ElementId.
        /// </summary>
        public void ChangeSpecFilter(ViewSchedule spec, string specFilterName, ElementId filterValue) {
            ChangeSpecFilter(
                spec,
                specFilterName,
                (filter) => filter.SetValue(filterValue));
        }

        /// <summary>
        /// Изменяет фильтр спецификации с учетом переданного типа значения.  
        /// </summary>
        private void ChangeSpecFilter(ViewSchedule spec, string specFilterName, Action<ScheduleFilter> action) {
            ScheduleDefinition specificationDefinition = spec.Definition;
            List<ScheduleFilter> specificationFilters = specificationDefinition.GetFilters().ToList();
            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            for(int i = 0; i < specificationFilters.Count; i++) {
                ScheduleFilter currentFilter = specificationFilters[i];
                ScheduleField scheduleFieldFromFilter = specificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {
                    action.Invoke(currentFilter);
                }
                newScheduleFilters.Add(currentFilter);
            }
            specificationDefinition.SetFilters(newScheduleFilters);
        }
    }
}

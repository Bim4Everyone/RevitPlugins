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
        /// Устанавливает параметры спецификации.
        /// </summary>
        public void SetSpecParams(ViewSchedule newViewSpec, DispatcherOption dispatcherOption) {
            newViewSpec.SetParamValue(dispatcherOption.FirstGroupingLevelParamName, dispatcherOption.FirstGroupingLevelParamValue);
            newViewSpec.SetParamValue(dispatcherOption.SecondGroupingLevelParamName, dispatcherOption.SecondGroupingLevelParamValue);
            newViewSpec.SetParamValue(dispatcherOption.ThirdGroupingLevelParamName, dispatcherOption.ThirdGroupingLevelParamValue);
        }

        /// <summary>
        /// Изменяет фильтры спецификации.
        /// </summary>
        public void ChangeSpecFilters(ViewSchedule spec, string specFilterName, string newFilterValue) {
            ScheduleDefinition specificationDefinition = spec.Definition;
            List<ScheduleFilter> specificationFilters = specificationDefinition.GetFilters().ToList();
            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            for(int i = 0; i < specificationFilters.Count; i++) {
                ScheduleFilter currentFilter = specificationFilters[i];
                ScheduleField scheduleFieldFromFilter = specificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {
                    currentFilter.SetValue(newFilterValue);
                }
                newScheduleFilters.Add(currentFilter);
            }
            specificationDefinition.SetFilters(newScheduleFilters);
        }

        /// <summary>
        /// Изменяет фильтры спецификации для ElementId.
        /// </summary>
        public void ChangeSpecFilters(ViewSchedule spec, string specFilterName, ElementId newFilterValue) {
            ScheduleDefinition specificationDefinition = spec.Definition;
            List<ScheduleFilter> specificationFilters = specificationDefinition.GetFilters().ToList();
            List<ScheduleFilter> newScheduleFilters = new List<ScheduleFilter>();

            for(int i = 0; i < specificationFilters.Count; i++) {
                ScheduleFilter currentFilter = specificationFilters[i];
                ScheduleField scheduleFieldFromFilter = specificationDefinition.GetField(currentFilter.FieldId);

                if(scheduleFieldFromFilter.GetName() == specFilterName) {
                    currentFilter.SetValue(newFilterValue);
                }
                newScheduleFilters.Add(currentFilter);
            }
            specificationDefinition.SetFilters(newScheduleFilters);
        }
    }
}

using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ScheduleViewModel : BaseViewModel, IEquatable<ScheduleViewModel> {
        private readonly ILocalizationService _localizationService;
        private int _countOnSheets;

        public ScheduleViewModel(ScheduleModel scheduleModel, ILocalizationService localizationService) {
            ScheduleModel = scheduleModel ?? throw new ArgumentNullException(nameof(scheduleModel));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            IsPlacedStatus = localizationService.GetLocalizedString(
                scheduleModel.Exists ? "EntityState.Exist" : "EntityState.New");
        }


        public string Name => ScheduleModel.Name;

        public string IsPlacedStatus { get; }

        public ScheduleModel ScheduleModel { get; }

        /// <summary>
        /// Количество данной спецификации на всех листах
        /// </summary>
        public int CountOnSheets {
            get => _countOnSheets;
            set => RaiseAndSetIfChanged(ref _countOnSheets, value);
        }


        public bool Equals(ScheduleViewModel other) {
            return other is not null
                && ScheduleModel.Equals(other.ScheduleModel);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ScheduleViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<ScheduleModel>.Default.GetHashCode(ScheduleModel);
        }
    }
}

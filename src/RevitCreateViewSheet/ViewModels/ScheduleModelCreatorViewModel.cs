using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ScheduleModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private string _errorText;
        private ViewScheduleViewModel _selectedSchedule;

        public ScheduleModelCreatorViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            ViewSchedules = [.. _revitRepository.GetAllSchedules()
                .Select(s => new ViewScheduleViewModel(s))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            SelectedViewSchedule = ViewSchedules.FirstOrDefault();
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public IReadOnlyCollection<ViewScheduleViewModel> ViewSchedules { get; }

        public ViewScheduleViewModel SelectedViewSchedule {
            get => _selectedSchedule;
            set => RaiseAndSetIfChanged(ref _selectedSchedule, value);
        }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(ViewSchedules.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.SchedulesNotFound");
                return false;
            }

            if(SelectedViewSchedule is null) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.ScheduleNotSet");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}

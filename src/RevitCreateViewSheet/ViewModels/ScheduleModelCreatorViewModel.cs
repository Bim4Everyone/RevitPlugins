using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
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
        private readonly ObservableCollection<ViewScheduleViewModel> _allSchedules;
        private string _errorText;
        private ViewScheduleViewModel _selectedSchedule;
        private string _searchScheduleName;

        public ScheduleModelCreatorViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            _allSchedules = new ObservableCollection<ViewScheduleViewModel>(_revitRepository.GetAllSchedules()
                .Select(s => new ViewScheduleViewModel(s))
                .OrderBy(a => a.Name, new LogicalStringComparer()));
            ViewSchedules = new CollectionViewSource() { Source = _allSchedules };
            ViewSchedules.Filter += SchedulesFilterHandler;
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);

            PropertyChanged += SchedulesFilterPropertyChanged;
        }


        public CollectionViewSource ViewSchedules { get; }

        public ViewScheduleViewModel SelectedViewSchedule {
            get => _selectedSchedule;
            set => RaiseAndSetIfChanged(ref _selectedSchedule, value);
        }

        public string SearchScheduleName {
            get => _searchScheduleName;
            set => RaiseAndSetIfChanged(ref _searchScheduleName, value);
        }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(ViewSchedules?.View.IsEmpty ?? true) {
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

        private void SchedulesFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is ViewScheduleViewModel schedule) {
                if(!string.IsNullOrWhiteSpace(SearchScheduleName)) {
                    string str = SearchScheduleName.ToLower();
                    e.Accepted = schedule.Name.ToLower().Contains(str);
                    return;
                }
                e.Accepted = true;
            }
        }

        private void SchedulesFilterPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SearchScheduleName)) {
                ViewSchedules?.View.Refresh();
            }
        }
    }
}

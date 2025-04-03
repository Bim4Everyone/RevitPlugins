using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ScheduleModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;

        public ScheduleModelCreatorViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            ViewSchedules = [.. _revitRepository.GetNotPlacedSchedules()
                .Select(s => new ViewScheduleViewModel(s))];
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public ObservableCollection<ViewScheduleViewModel> ViewSchedules { get; }

        public ViewScheduleViewModel SelectedViewSchedule { get; set; }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(ViewSchedules.Count == 0) {
                ErrorText = "Все спецификации уже размещены на листах TODO";
                return false;
            }

            if(SelectedViewSchedule is null) {
                ErrorText = "Выберите спецификацию TODO";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}

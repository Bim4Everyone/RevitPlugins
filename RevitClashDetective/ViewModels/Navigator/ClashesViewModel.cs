using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private List<ClashViewModel> _clashes;
        private CollectionViewSource _clashesViewSource;

        public ClashesViewModel(RevitRepository revitRepository, IEnumerable<ClashModel> clashModels) {
            _revitRepository = revitRepository;
            Clashes = clashModels.Select(item => new ClashViewModel(item)).ToList();

            ClashesViewSource = new CollectionViewSource() { Source = Clashes };
            SelectClashCommand = new RelayCommand(SelectClash);
            SelectNextCommand = new RelayCommand(SelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious);

        }

        public ICommand SelectClashCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand SelectNextCommand { get; }

        public List<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        public CollectionViewSource ClashesViewSource {
            get => _clashesViewSource;
            set => this.RaiseAndSetIfChanged(ref _clashesViewSource, value);
        }

        private async void SelectClash(object p) {
            var clash = p as ClashViewModel;
            await _revitRepository.SelectAndShowElement(clash.GetElementId());
        }

        private async void SelectNext(object p) {
            ClashesViewSource.View.MoveCurrentToNext();
            if(ClashesViewSource.View.IsCurrentAfterLast) {
                ClashesViewSource.View.MoveCurrentToPrevious();
            } else {
                var clash = ClashesViewSource.View.CurrentItem as ClashViewModel;
                await _revitRepository.SelectAndShowElement(clash.GetElementId());

            }
        }

        private async void SelectPrevious(object p) {
            ClashesViewSource.View.MoveCurrentToPrevious();
            if(ClashesViewSource.View.IsCurrentBeforeFirst) {
                ClashesViewSource.View.MoveCurrentToNext();
            } else {
                var clash = ClashesViewSource.View.CurrentItem as ClashViewModel;
                await _revitRepository.SelectAndShowElement(clash.GetElementId());
            }
        }
    }
}

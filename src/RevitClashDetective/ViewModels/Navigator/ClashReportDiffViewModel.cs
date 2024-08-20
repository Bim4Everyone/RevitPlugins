using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashReportDiffViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<DiffClashViewModel> _clashes;

        public ClashReportDiffViewModel(RevitRepository revitRepository, IEnumerable<ClashModel> revitClashes, IEnumerable<ClashModel> pluginClashes) {
            _revitRepository = revitRepository;

            var extraRevitClashes = revitClashes.Except(pluginClashes)
                                                .Select(item => new DiffClashViewModel() { Clash = item, Source = "Revit" })
                                                .ToArray();
            var extraPluginClashes = pluginClashes.Except(revitClashes)
                                                  .Select(item => new DiffClashViewModel() { Clash = item, Source = "Плагин" })
                                                  .ToArray();

            Clashes = new ObservableCollection<DiffClashViewModel>(extraRevitClashes.Union(extraPluginClashes));

            SelectClashCommand = RelayCommand.Create<DiffClashViewModel>(Select, CanSelect);
        }

        public ICommand SelectClashCommand { get; }

        public ObservableCollection<DiffClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }

        private void Select(DiffClashViewModel clash) {
            var elements = new[] {
                clash.Clash.MainElement,
                clash.Clash.OtherElement
            };
            _revitRepository.SelectAndShowElement(elements.Where(item => item != null));
        }

        private bool CanSelect(DiffClashViewModel p) {
            return p != null;
        }
    }

    internal class DiffClashViewModel : BaseViewModel {
        private ClashModel _clash;

        public ClashModel Clash {
            get => _clash;
            set => this.RaiseAndSetIfChanged(ref _clash, value);
        }

        public string Source { get; set; }
    }
}

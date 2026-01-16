using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private RevitViewModel _revitViewModel;

    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
        _revitRepository = revitRepository;

        RevitViewModels = [
            new ViewRevitViewModel(revitRepository) { Name = "Выборка по текущему виду" },
            new SelectedRevitViewModel(revitRepository) { Name = "Выборка по выделенным элементам" }
        ];

        RevitViewModel = _revitRepository.GetSelectedRooms().Count > 0 ? RevitViewModels[1] : RevitViewModels[0];

    }

    public RevitViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitViewModel> RevitViewModels { get; }
}

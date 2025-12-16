using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels; 
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private string _errorText;
    private bool _clearWallsParameters;

    public MainViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;

        Levels = [.. GetLevelViewModels()
            .OrderBy(item => item.Element.Elevation)];
        
        SetWallParametersCommand = new RelayCommand(SetWallParameters, CanSetWallParameters);
    }

    public ICommand SetWallParametersCommand { get; }

    public List<LevelViewModel> Levels { get; }

    protected IEnumerable<LevelViewModel> GetLevelViewModels() {
        return _revitRepository.GetRooms()
            .Where(item => item.Level != null)
            .GroupBy(item => item.Level.Name)
            .Select(item => new LevelViewModel(item.Key, item.Select(room => room.Level).FirstOrDefault(), item));
    }

    private void SetWallParameters(object p) {
        if(ClearWallsParameters) {
            _revitRepository.ClearWallsParameters(Levels
                .Where(item => item.IsSelected)
                .Select(x => x.Element));
        }

        var rooms = Levels.Where(item => item.IsSelected)
            .SelectMany(x => x.Rooms)
            .ToList();

        var allWalls = _revitRepository.GetGroupedRoomsByWalls(rooms);

        using var t = _revitRepository.Document.StartTransaction("Заполнить параметры ВОР");
        foreach(var wallElement in allWalls) {
            var wall = wallElement.Wall;

            wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomName.Name,
                                        wallElement.GetRoomsParameters(nameof(RoomElement.Name)));
            wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomNumber.Name,
                                        wallElement.GetRoomsParameters(nameof(RoomElement.Number)));
            wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomID.Name,
                                        wallElement.GetRoomsParameters(nameof(RoomElement.Id)));
            wall.SetProjectParamValue(ProjectParamsConfig.Instance.RelatedRoomGroup.Name,
                                        wallElement.GetRoomsParameters(nameof(RoomElement.Group)));
        }
        t.Commit();
    }

    private bool CanSetWallParameters(object p) {
        if(Levels.Count == 0) {
            ErrorText = "Помещения отсутствуют в проекте";
            return false;
        }
        if(!Levels.Any(x => x.IsSelected)) {
            ErrorText = "Выберите уровни";
            return false;
        }

        ErrorText = "";
        return true;
    }

    public bool ClearWallsParameters {
        get => _clearWallsParameters;
        set => RaiseAndSetIfChanged(ref _clearWallsParameters, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
}

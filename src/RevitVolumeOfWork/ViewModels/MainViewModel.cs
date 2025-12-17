using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels; 
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    
    private string _errorText;
    
    private bool _clearWallsParameters;
    private IList<LevelViewModel> _levels;

    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository, 
                         ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        Levels = [.. GetLevelViewModels()
            .OrderBy(item => item.Element.Elevation)];
        
        SetWallParametersCommand = new RelayCommand(SetWallParameters, CanSetWallParameters);
        
        LoadConfig();
    }

    public ICommand SetWallParametersCommand { get; }

    public IList<LevelViewModel> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }

    public bool ClearWallsParameters {
        get => _clearWallsParameters;
        set => RaiseAndSetIfChanged(ref _clearWallsParameters, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    
    private IEnumerable<LevelViewModel> GetLevelViewModels() {
        return _revitRepository.GetRooms()
            .Where(item => item.Level != null)
            .GroupBy(item => item.Level.Name)
            .Select(item => new LevelViewModel(item.Key, item.Select(room => room.Level).FirstOrDefault(), item));
    }

    private void SetWallParameters(object p) {
        if(ClearWallsParameters) {
            _revitRepository.ClearWallsParameters(Levels
                .Where(item => item.IsChecked)
                .Select(x => x.Element));
        }

        var rooms = Levels.Where(item => item.IsChecked)
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
        
        SaveConfig();
    }

    private bool CanSetWallParameters(object p) {
        if(Levels.Count == 0) {
            ErrorText = "Помещения отсутствуют в проекте";
            return false;
        }
        if(!Levels.Any(x => x.IsChecked)) {
            ErrorText = "Выберите уровни";
            return false;
        }

        ErrorText = "";
        return true;
    }

    private void LoadConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);
        if(settings is null) {
            return;
        }
        
        var levels = Levels.Where(x => settings.Levels.Contains(x.Name));
        foreach(var level in levels) {
            level.IsChecked = true;
        }
    }

    private void SaveConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document);
        
        settings ??= _pluginConfig.AddSettings(_revitRepository.Document);
        
        settings.Levels = [.. Levels
            .Where(x => x.IsChecked)
            .Select(x => x.Name)];
        
        _pluginConfig.SaveProjectConfig();
    }
}

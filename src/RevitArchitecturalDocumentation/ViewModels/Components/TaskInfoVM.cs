using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;

namespace RevitArchitecturalDocumentation.ViewModels.Components;
internal class TaskInfoVM : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private int _taskNumber;
    private Element _selectedVisibilityScope;
    private string _startLevelNumber;
    private string _endLevelNumber;
    private string _viewNameSuffix;

    public TaskInfoVM(Regex regexForBuildingPart, Regex regexForBuildingSection, ILocalizationService localizationService,
                      int taskNumber) {
        _localizationService = localizationService;
        RegexForBuildingPart = regexForBuildingPart;
        RegexForBuildingSection = regexForBuildingSection;
        TaskNumber = taskNumber;
    }

    public StringBuilder Report { get; set; }
    public int TaskNumber {
        get => _taskNumber;
        set => RaiseAndSetIfChanged(ref _taskNumber, value);
    }

    public Regex RegexForBuildingPart { get; set; }
    public Regex RegexForBuildingSection { get; set; }
    public Element SelectedVisibilityScope {
        get => _selectedVisibilityScope;
        set => RaiseAndSetIfChanged(ref _selectedVisibilityScope, value);
    }

    public string StartLevelNumber {
        get => _startLevelNumber;
        set => RaiseAndSetIfChanged(ref _startLevelNumber, value);
    }

    public int StartLevelNumberAsInt { get; set; }
    public string EndLevelNumber {
        get => _endLevelNumber;
        set => RaiseAndSetIfChanged(ref _endLevelNumber, value);
    }

    public int EndLevelNumberAsInt { get; set; }
    public int NumberOfBuildingPartAsInt { get; set; }
    public int NumberOfBuildingSectionAsInt { get; set; }
    public string ViewNameSuffix {
        get => _viewNameSuffix;
        set => RaiseAndSetIfChanged(ref _viewNameSuffix, value);
    }

    public ObservableCollection<SpecHelper> ListSpecHelpers { get; set; } = [];


    /// <summary>
    /// Проверяет на наличие ошибок в задании - корректность заполнения номеров уровней, имени области видимости и т.д.
    /// </summary>
    public void CheckTasksForErrors() {

        // Попытка запарсить уровень с которого нужно начать создавать виды
        if(!int.TryParse(StartLevelNumber, out int startLevelNumberAsInt)) {
            throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.UnableDetermineStartLevelInTask")} {TaskNumber}");
        }
        StartLevelNumberAsInt = startLevelNumberAsInt;

        // Попытка запарсить уровень на котором нужно закончить создавать виды
        if(!int.TryParse(EndLevelNumber, out int endLevelNumberAsInt)) {
            throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.UnableDetermineEndLevelInTask")} {TaskNumber}");
        }
        EndLevelNumberAsInt = endLevelNumberAsInt;

        if(StartLevelNumberAsInt > EndLevelNumberAsInt) {
            throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.StartLevelMustBeLessEndLevelInTask")} {TaskNumber}");
        }

        // Проверка, что пользователь выбрал область видимости и ее данных
        if(SelectedVisibilityScope is null) {
            throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.VisibilityScopeNotSelectedInTask")} {TaskNumber}");
        } else {
            // Попытка запарсить номер корпуса из имени области видимости
            string numberOfBuildingPart = RegexForBuildingPart.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingPart, out int numberOfBuildingPartAsInt)) {
                throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.UnableDetectBuildingInVisibilityScopeInTask")} {TaskNumber}");
            }
            NumberOfBuildingPartAsInt = numberOfBuildingPartAsInt;

            // Попытка запарсить номер секции из имени области видимости
            string numberOfBuildingSection = RegexForBuildingSection.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingSection, out int numberOfBuildingSectionAsInt)) {
                throw new TaskException($"{_localizationService.GetLocalizedString("CreatingARDocsVM.UnableDetectSectionInVisibilityScopeInTask")} {TaskNumber}");
            }
            NumberOfBuildingSectionAsInt = numberOfBuildingSectionAsInt;
        }
    }
}

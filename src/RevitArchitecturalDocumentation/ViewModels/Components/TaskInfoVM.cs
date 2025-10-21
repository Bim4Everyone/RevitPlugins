using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;

namespace RevitArchitecturalDocumentation.ViewModels.Components;
internal class TaskInfoVM : BaseViewModel {
    private int _taskNumber;
    private Element _selectedVisibilityScope;
    private string _startLevelNumber;
    private string _endLevelNumber;
    private string _viewNameSuffix;

    public TaskInfoVM(Regex regexForBuildingPart, Regex regexForBuildingSection, int taskNumber) {

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
            throw new TaskException($"Не удалось определить начальный уровень в задании №{TaskNumber}");
        }
        StartLevelNumberAsInt = startLevelNumberAsInt;

        // Попытка запарсить уровень на котором нужно закончить создавать виды
        if(!int.TryParse(EndLevelNumber, out int endLevelNumberAsInt)) {
            throw new TaskException($"Не удалось определить конечный уровень в задании №{TaskNumber}");
        }
        EndLevelNumberAsInt = endLevelNumberAsInt;

        if(StartLevelNumberAsInt > EndLevelNumberAsInt) {
            throw new TaskException($"Начальный уровень должен быть не больше конечного в задании №{TaskNumber}");
        }

        // Проверка, что пользователь выбрал область видимости и ее данных
        if(SelectedVisibilityScope is null) {
            throw new TaskException($"Не выбрана область видимости в задании №{TaskNumber}");
        } else {
            // Попытка запарсить номер корпуса из имени области видимости
            string numberOfBuildingPart = RegexForBuildingPart.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingPart, out int numberOfBuildingPartAsInt)) {
                throw new TaskException($"Не удалось определить корпус у области видимости в задании №{TaskNumber}");
            }
            NumberOfBuildingPartAsInt = numberOfBuildingPartAsInt;

            // Попытка запарсить номер секции из имени области видимости
            string numberOfBuildingSection = RegexForBuildingSection.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingSection, out int numberOfBuildingSectionAsInt)) {
                throw new TaskException($"Не удалось определить секцию у области видимости в задании №{TaskNumber}");
            }
            NumberOfBuildingSectionAsInt = numberOfBuildingSectionAsInt;
        }
    }
}

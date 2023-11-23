using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitArchitecturalDocumentation.ViewModels;

using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace RevitArchitecturalDocumentation.Models {
    internal class TaskInfo {
        public TaskInfo(Regex regexForBuildingPart, Regex regexForBuildingSection, StringBuilder report = null) {

            RegexForBuildingPart = regexForBuildingPart;
            RegexForBuildingSection = regexForBuildingSection;
            Report = report;
        }

        public StringBuilder Report { get; set; }
        public bool CanWorkWithIt { get; set; } = true;

        public Regex RegexForBuildingPart { get; set; }
        public Regex RegexForBuildingSection { get; set; }
        public Element SelectedVisibilityScope { get; set; }
        public string StartLevelNumber { get; set; }
        public int StartLevelNumberAsInt { get; set; }
        public string EndLevelNumber { get; set; }
        public int EndLevelNumberAsInt { get; set; }
        public int NumberOfBuildingPartAsInt { get; set; }
        public int NumberOfBuildingSectionAsInt { get; set; }
        public string ViewNameSuffix { get; set; }
        public List<SpecHelper> ScheduleSheetInstances { get; set; } = new List<SpecHelper>();

        public void AnalizeTask() {

            // Проверка, что пользователь выбрал область видимости
            if(SelectedVisibilityScope is null) {
                Report?.AppendLine($"❗       Не выбрана область видимости!");
                CanWorkWithIt = false;
                return;
            }
            Report?.AppendLine($"        Работа с областью видимости: {SelectedVisibilityScope.Name}");

            // Попытка запарсить уровень с которого нужно начать создавать виды
            if(!int.TryParse(StartLevelNumber, out int startLevelNumberAsInt)) {
                Report?.AppendLine($"❗       Начальный уровень некорректен!");
                CanWorkWithIt = false;
                return;
            }
            StartLevelNumberAsInt = startLevelNumberAsInt;
            Report?.AppendLine($"        Начальный уровень: {startLevelNumberAsInt}");

            // Попытка запарсить уровень на котором нужно закончить создавать виды
            if(!int.TryParse(EndLevelNumber, out int endLevelNumberAsInt)) {
                Report?.AppendLine($"❗       Конечный уровень некорректен!");
                CanWorkWithIt = false;
                return;
            }
            EndLevelNumberAsInt = endLevelNumberAsInt;
            Report?.AppendLine($"        Конечный уровень: {endLevelNumberAsInt}");

            // Попытка запарсить номер корпуса из имени области видимости
            string numberOfBuildingPart = RegexForBuildingPart.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingPart, out int numberOfBuildingPartAsInt)) {
                Report?.AppendLine($"❗       Не удалось определить корпус у области видимости: {SelectedVisibilityScope.Name}!");
                CanWorkWithIt = false;
                return;
            }
            NumberOfBuildingPartAsInt = numberOfBuildingPartAsInt;
            Report?.AppendLine($"        Номер корпуса: {numberOfBuildingPartAsInt}");

            // Попытка запарсить номер секции из имени области видимости
            string numberOfBuildingSection = RegexForBuildingSection.Match(SelectedVisibilityScope.Name).Groups[1].Value;
            if(!int.TryParse(numberOfBuildingSection, out int numberOfBuildingSectionAsInt)) {
                Report?.AppendLine($"❗       Не удалось определить секцию у области видимости: {SelectedVisibilityScope.Name}!");
                CanWorkWithIt = false;
                return;
            }
            NumberOfBuildingSectionAsInt = numberOfBuildingSectionAsInt;
            Report?.AppendLine($"        Номер секции: {numberOfBuildingSectionAsInt}");
        }
    }
}

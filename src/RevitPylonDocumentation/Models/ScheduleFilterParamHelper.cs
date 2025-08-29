namespace RevitPylonDocumentation.Models;
/// <summary>
/// Класс, необходимый для работы с фильтрами спецификаций
/// Логика применения класса заключается в том, что пользователь указывает наименование параметра фильтрации спеки 
/// из Revit и наименование параметра, которое можно найти у элемента пилона, для которого делается спека
/// </summary>
internal class ScheduleFilterParamHelper {
    public ScheduleFilterParamHelper(string paramNameInSchedule, string paramNameInHost) {
        ParamNameInSchedule = paramNameInSchedule;
        ParamNameInHost = paramNameInHost;
    }

    public bool IsCheck { get; set; } = false;
    public string ParamNameInSchedule { get; set; }
    public string ParamNameInHost { get; set; }
}

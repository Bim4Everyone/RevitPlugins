using System.Collections.Generic;

using dosymep.Bim4Everyone.SharedParams;

namespace RevitSleeves.Services.Core;
/// <summary>
/// Класс для хранения констант
/// </summary>
internal static class NamesProvider {
    public const string FamilyNameSleeve = "АрмТр_Авт_Гильза";
    public const string SleeveSymbolName = FamilyNameSleeve;

    public const string FamilyNameOpeningArRectangleInFloor = "Окн_Отв_Прямоуг_Перекрытие";
    public const string FamilyNameOpeningArRectangleInWall = "Окн_Отв_Прямоуг_Стена";
    public const string FamilyNameOpeningArRoundInFloor = "Окн_Отв_Круг_Перекрытие";
    public const string FamilyNameOpeningArRoundInWall = "Окн_Отв_Круг_Стена";

    public const string ParameterOpeningArDiameter = "ФОП_РАЗМ_Диаметр";
    public const string ParameterOpeningArWidth = "ФОП_РАЗМ_Ширина проёма";
    public const string ParameterOpeningArHeight = "ФОП_РАЗМ_Высота проёма";
    public const string ParameterOpeningArThickness = "ФОП_РАЗМ_Глубина проёма";

    public const string FamilyNameOpeningKrRectangleInFloor = "ОбщМд_Отверстие_Перекрытие_Прямоугольное";
    public const string FamilyNameOpeningKrRectangleInWall = "ОбщМд_Отверстие_Стена_Прямоугольное";
    public const string FamilyNameOpeningKrRoundInWall = "ОбщМд_Отверстие_Стена_Круглое";

    public const string ParameterOpeningKrDiameter = "ФОП_РАЗМ_Диаметр";
    public const string ParameterOpeningKrInWallWidth = "ФОП_РАЗМ_Ширина";
    public const string ParameterOpeningKrInWallHeight = "ФОП_РАЗМ_Высота";
    public const string ParameterOpeningKrThickness = "ФОП_РАЗМ_Глубина";
    public const string ParameterOpeningKrInFloorHeight = "мод_ФОП_Габарит А";
    public const string ParameterOpeningKrInFloorWidth = "мод_ФОП_Габарит Б";

    public const string ParameterSleeveIncline = "ADSK_Размер_УголПоворота";
    public const string ParameterSleeveDiameter = "Диаметр";
    public const string ParameterSleeveLength = "Длина";

    public const string ViewNameSleeve = "BIM_Гильзы_{0}";

    public const string FilterNameMep = "BIM_Гильзы_ВИС";
    public const string FilterNameSleeves = "BIM_Гильзы";
    public const string FilterNameConstructions = "BIM_Гильзы_Конструкции";
    public const string FilterNameSecondaryCategories = "BIM_Гильзы_Вспомогательные_Категории";

    public const string BIM = "BIM";


    public static readonly SharedParam ParameterSleeveSystem = SharedParamsConfig.Instance.VISSystemNameForced;
    public static readonly SharedParam ParameterSleeveEconomic = SharedParamsConfig.Instance.VISHvacSystemForcedFunction;
    public const string ParameterSleeveThickness = "ФОП_ВИС_Толщина стенки";
    public const string ParameterSleeveDescription = "ФОП_Описание";

    public static readonly IReadOnlyCollection<string> FamilyNamesAllOpenings = [
        FamilyNameOpeningArRectangleInFloor,
        FamilyNameOpeningArRectangleInWall,
        FamilyNameOpeningArRoundInFloor,
        FamilyNameOpeningArRoundInWall,
        FamilyNameOpeningKrRectangleInFloor,
        FamilyNameOpeningKrRectangleInWall,
        FamilyNameOpeningKrRoundInWall];
}

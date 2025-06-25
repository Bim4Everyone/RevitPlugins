namespace RevitSleeves.Services.Core;
/// <summary>
/// Класс для хранения строковых констант
/// </summary>
internal static class NamesProvider {
    public const string FamilyNameSleeve = "ТрСд_Авт_Гильза";
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
}

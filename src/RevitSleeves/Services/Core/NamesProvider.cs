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

    public static readonly SharedParam ParameterMepSystem = SharedParamsConfig.Instance.VISSystemName;
    public static readonly SharedParam ParameterMepEconomic = SharedParamsConfig.Instance.EconomicFunction;

    public static readonly SharedParam ParameterOpeningArDiameter = SharedParamsConfig.Instance.SizeDiameter;
    public static readonly SharedParam ParameterOpeningArWidth = SharedParamsConfig.Instance.SizeOpeningWidth;
    public static readonly SharedParam ParameterOpeningArHeight = SharedParamsConfig.Instance.SizeOpeningHeight;
    public static readonly SharedParam ParameterOpeningArThickness = SharedParamsConfig.Instance.SizeOpeningDepth;

    public const string FamilyNameOpeningKrRectangleInFloor = "ОбщМд_Отверстие_Перекрытие_Прямоугольное";
    public const string FamilyNameOpeningKrRectangleInWall = "ОбщМд_Отверстие_Стена_Прямоугольное";
    public const string FamilyNameOpeningKrRoundInWall = "ОбщМд_Отверстие_Стена_Круглое";

    public static readonly SharedParam ParameterOpeningKrDiameter = ParameterOpeningArDiameter;
    public static readonly SharedParam ParameterOpeningKrInWallWidth = SharedParamsConfig.Instance.SizeWidth;
    public static readonly SharedParam ParameterOpeningKrInWallHeight = SharedParamsConfig.Instance.SizeHeight;
    public static readonly SharedParam ParameterOpeningKrThickness = SharedParamsConfig.Instance.SizeDepth;
    public static readonly SharedParam ParameterOpeningKrInFloorHeight = SharedParamsConfig.Instance.DimensionAModeling;
    public static readonly SharedParam ParameterOpeningKrInFloorWidth = SharedParamsConfig.Instance.DimensionBModeling;

    public static readonly SharedParam ParameterSleeveIncline = SharedParamsConfig.Instance.SizeRotationAngle;
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
    public static readonly SharedParam ParameterSleeveThickness = SharedParamsConfig.Instance.VISSideThickness;
    public static readonly SharedParam ParameterSleeveDescription = SharedParamsConfig.Instance.Description;

    public static readonly IReadOnlyCollection<string> FamilyNamesAllOpenings = [
        FamilyNameOpeningArRectangleInFloor,
        FamilyNameOpeningArRectangleInWall,
        FamilyNameOpeningArRoundInFloor,
        FamilyNameOpeningArRoundInWall,
        FamilyNameOpeningKrRectangleInFloor,
        FamilyNameOpeningKrRectangleInWall,
        FamilyNameOpeningKrRoundInWall];
}

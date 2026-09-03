using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Models;
/// <summary>
/// Предоставляет категории и правила фильтрации по минимальным габаритам,
/// из которых собираются поисковые наборы плагина
/// </summary>
internal class FiltersInitializer {

    /// <summary>
    /// Возвращает категории линейных элементов заданной дисциплины инженерных систем
    /// </summary>
    /// <param name="mepCategory">Дисциплина инженерных систем</param>
    /// <exception cref="NotSupportedException">Исключение, если дисциплина не поддерживается</exception>
    public static ICollection<BuiltInCategory> GetLinearCategories(MepCategoryEnum mepCategory) {
        return mepCategory switch {
            MepCategoryEnum.Pipe => [RevitRepository.MepPipeLinearCategory],
            MepCategoryEnum.RectangleDuct or MepCategoryEnum.RoundDuct => [RevitRepository.MepDuctLinearCategory],
            MepCategoryEnum.CableTray => [RevitRepository.MepCableTrayLinearCategory],
            MepCategoryEnum.Conduit => [RevitRepository.MepConduitLinearCategory],
            _ => throw new NotSupportedException(nameof(mepCategory)),
        };
    }

    /// <summary>
    /// Возвращает категории соединительных деталей заданной дисциплины инженерных систем
    /// </summary>
    /// <param name="fittingCategory">Дисциплина соединительных деталей</param>
    /// <exception cref="NotSupportedException">Исключение, если дисциплина не поддерживается</exception>
    public static ICollection<BuiltInCategory> GetFittingCategories(FittingCategoryEnum fittingCategory) {
        return fittingCategory switch {
            FittingCategoryEnum.PipeFitting => [.. RevitRepository.MepPipeFittingCategories],
            FittingCategoryEnum.DuctFitting => [.. RevitRepository.MepDuctFittingCategories],
            FittingCategoryEnum.CableTrayFitting => [.. RevitRepository.MepCableTrayFittingCategories],
            FittingCategoryEnum.ConduitFitting => [.. RevitRepository.MepConduitFittingCategories],
            _ => throw new NotSupportedException(nameof(fittingCategory)),
        };
    }

    /// <summary>
    /// Возвращает правила фильтрации "больше или равно" по минимальным габаритам сечения
    /// элементов заданной дисциплины инженерных систем.
    /// Значения габаритов возвращаются во внутренних единицах Revit.
    /// </summary>
    /// <param name="mepCategory">Настройки расстановки отверстий для дисциплины инженерных систем</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="NotSupportedException">Исключение, если дисциплина не поддерживается</exception>
    public static IEnumerable<(BuiltInParameter Param, double Value)> GetMinSizeRules(MepCategory mepCategory) {
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }

        var mepCategoryType = RevitRepository.MepCategoryNames
            .First(pair => pair.Value.Equals(mepCategory.Name))
            .Key;
        switch(mepCategoryType) {
            case MepCategoryEnum.Pipe:
                return GetDiameterRule(mepCategory, BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            case MepCategoryEnum.RoundDuct:
                return GetDiameterRule(mepCategory, BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
            case MepCategoryEnum.Conduit:
                return GetDiameterRule(mepCategory, BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            case MepCategoryEnum.RectangleDuct:
                return GetSectionRules(mepCategory,
                    BuiltInParameter.RBS_CURVE_HEIGHT_PARAM,
                    BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            case MepCategoryEnum.CableTray:
                return GetSectionRules(mepCategory,
                    BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM,
                    BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM);
            default:
                throw new NotSupportedException(nameof(mepCategory));
        }
    }

    /// <summary>
    /// Возвращает фильтр по всем используемым категориям элементов инженерных систем
    /// </summary>
    public static ElementMulticategoryFilter GetFilterByAllUsedMepCategories() {
        return new ElementMulticategoryFilter(GetAllUsedMepCategories());
    }

    /// <summary>
    /// Возвращает все используемые категории инженерных систем
    /// </summary>
    public static ICollection<BuiltInCategory> GetAllUsedMepCategories() {
        List<BuiltInCategory> categories =
        [
            .. RevitRepository.MepPipeCategories,
            .. RevitRepository.MepDuctCategories,
            .. RevitRepository.MepCableTrayCategories,
            .. RevitRepository.MepConduitCategories,
        ];
        return categories;
    }

    /// <summary>
    /// Возвращает фильтр по всем используемым категориям конструкций
    /// </summary>
    public static ElementMulticategoryFilter GetFilterByAllUsedStructureCategories() {
        return new ElementMulticategoryFilter(GetAllUsedStructureCategories());
    }

    /// <summary>
    /// Возвращает все используемые категории конструкций
    /// </summary>
    public static ICollection<BuiltInCategory> GetAllUsedStructureCategories() {
        return new BuiltInCategory[] {
            RevitRepository.WallCategory,
            RevitRepository.FloorCategory
        };
    }

    /// <summary>
    /// Возвращает все используемые категории проемов
    /// </summary>
    public static ICollection<BuiltInCategory> GetAllUsedOpeningsCategories() {
        return new BuiltInCategory[] {
            BuiltInCategory.OST_Windows
        };
    }


    private static IEnumerable<(BuiltInParameter Param, double Value)> GetDiameterRule(
        MepCategory mepCategory,
        BuiltInParameter diameterParam) {
        var diameter = mepCategory.MinSizes[Parameters.Diameter];
        if(diameter != null) {
            yield return (diameterParam, diameter.GetConvertedValue());
        }
    }

    private static IEnumerable<(BuiltInParameter Param, double Value)> GetSectionRules(
        MepCategory mepCategory,
        BuiltInParameter heightParam,
        BuiltInParameter widthParam) {
        var height = mepCategory.MinSizes[Parameters.Height];
        var width = mepCategory.MinSizes[Parameters.Width];
        if(height != null && width != null) {
            yield return (heightParam, height.GetConvertedValue());
            yield return (widthParam, width.GetConvertedValue());
        }
    }
}

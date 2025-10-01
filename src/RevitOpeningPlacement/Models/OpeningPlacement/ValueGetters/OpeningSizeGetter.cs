
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class OpeningSizeGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
    private readonly double _size;
    private readonly int _defaultSizeRounding;
    private readonly MepCategory[] _mepCategories;
    /// <summary>
    /// Минимальное значение габарита задания на отверстие в футах (5 мм)
    /// </summary>
    private const double _minGeometryFeetSize = 0.015;

    /// <summary>
    /// Создает объект для получения размеров задания на отверстия с учетом отступов и округлений
    /// </summary>
    /// <param name="size">Начальный размер заания на отверстие без отступов и округлений в единицах Revit</param>
    /// <param name="defaultSizeRounding">Округление размеров по умолчанию в мм, которое применится, 
    /// если <paramref name="mepCategories"/> пустая коллекция</param>
    /// <param name="mepCategories">Категории ВИС для настройки отступов и округлений</param>
    public OpeningSizeGetter(double size, int defaultSizeRounding, params MepCategory[] mepCategories) {
        _size = size;
        _defaultSizeRounding = defaultSizeRounding;
        _mepCategories = mepCategories;
    }

    public DoubleParamValue GetValue() {
        double size = _size;
        int mmRound = 0;
        if(_mepCategories != null && _mepCategories.Length > 0) {
            size += _mepCategories.Max(item => item.GetOffsetValue(_size));
            mmRound = _mepCategories.Max(x => x.Rounding);
        } else {
            mmRound = _defaultSizeRounding;
        }
        size = RoundToCeilingFeetToMillimeters(size, mmRound);
        //проверка на недопустимо малые габариты
        return size < _minGeometryFeetSize
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(size);
    }
}


using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class WallOpeningSizeInitializer {
    private readonly BoundingBoxXYZ _bb;
    private readonly int _defaultSizeRounding;
    private readonly MepCategory[] _mepCategories;

    /// <summary>
    /// Инициализатор значений параметров для габаритов задания на отверстия с учетом округлений и отступов
    /// </summary>
    /// <param name="solid">Солид для определения габаритов задания на отверстие без учета габаритов и отступов</param>
    /// <param name="defaultSizeRounding">Округление размеров задания на отверстие в мм по умолчанию</param>
    /// <param name="mepCategories">Категории ВИС с настройками отступов и округлений</param>
    public WallOpeningSizeInitializer(Solid solid, int defaultSizeRounding, params MepCategory[] mepCategories) {
        _bb = solid.GetBoundingBox();
        _defaultSizeRounding = defaultSizeRounding;
        _mepCategories = mepCategories;
    }
    public IValueGetter<DoubleParamValue> GetWidth() {
        return new OpeningSizeGetter(_bb.Max.X - _bb.Min.X, _defaultSizeRounding, _mepCategories);
    }

    public IValueGetter<DoubleParamValue> GetHeight() {
        return new OpeningSizeGetter(_bb.Max.Z - _bb.Min.Z, _defaultSizeRounding, _mepCategories);
    }

    public IValueGetter<DoubleParamValue> GetThickness() {
        return new OpeningSizeGetter(_bb.Max.Y - _bb.Min.Y, 0);
    }
}

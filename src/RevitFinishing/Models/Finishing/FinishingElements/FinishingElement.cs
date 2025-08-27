using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitFinishing.Services;

namespace RevitFinishing.Models.Finishing;
/// <summary>
/// Абстрактный класс для экземпляра отделки.
/// Каждый элемент отделки хранит список всех помещений, к которым он относится.
/// </summary>
internal abstract class FinishingElement {
    protected readonly Element _revitElement;
    protected readonly ParamCalculationService _paramService;

    protected readonly bool _isInPlace;
    protected readonly bool _isCustomFamily;
    protected readonly FamilySymbol _familySymbol;
    protected readonly Element _revitElementType;

    protected readonly SharedParamsConfig _paramConfig = SharedParamsConfig.Instance;

    public FinishingElement(Element element,
                            ParamCalculationService paramService) {
        _revitElement = element;
        _revitElementType = _revitElement.Document.GetElement(_revitElement.GetTypeId());
        if(_revitElementType is FamilySymbol) {
            _familySymbol = _revitElementType as FamilySymbol;
            _isCustomFamily = _familySymbol != null;
            _isInPlace = _familySymbol.Family.IsInPlace;
        }

        _paramService = paramService;
    }

    public Element RevitElement => _revitElement;
    public bool IsCustomFamily => _isCustomFamily;
    public bool IsInPlace => _isInPlace;
    public List<RoomElement> Rooms { get; set; }

    // Перенос значения из общего параметра помещения в аналогичный параметр отделки
    protected void UpdateFromSharedParam(SharedParam param) {
        _revitElement.SetParamValue(param, _paramService.GetRoomsParameters(Rooms, param));
    }

    // Перенос значения из системного параметра помещения в общий параметр отделки
    protected void UpdateFromSystemParam(IEnumerable<RoomElement> rooms,
                                                 SharedParam param,
                                                 BuiltInParameter bltnParam) {
        _revitElement.SetParamValue(param, _paramService.GetRoomsParameters(rooms, bltnParam));
    }

    // Перенос значения из ключевого параметра помещения в общий параметр отделки
    protected void UpdateFromKeyParam(IEnumerable<RoomElement> rooms,
                                              SharedParam param,
                                              ProjectParam keyParam) {
        _revitElement.SetParamValue(param, _paramService.GetRoomsKeyParameters(rooms, keyParam));
    }

    // Перенос значения из системного параметра экземпляра отделки в общий параметр отделки
    // У отделки могут отсутствовать системеный параметры, поэтому выполняется проверка.
    protected void UpdateFromInstParam(SharedParam param, BuiltInParameter bltnParam) {
        // Проверка является ли семейство загружаемым или моделью в контексте
        if(_isCustomFamily) {
            // Параметр заполняется только для моделей в контексте.
            // Для загружаемых семейств предполагается, что параметр рассчитан уже внутри семейства.
            if(_isInPlace && _revitElement.IsExistsParam(bltnParam)) {
                _revitElement.SetParamValue(param, _revitElement.GetParamValue<double>(bltnParam));
            }
        } else {
            _revitElement.SetParamValue(param, _revitElement.GetParamValue<double>(bltnParam));
        }
    }

    protected void UpdateOrderParam(SharedParam param, int value) {
        _revitElement.SetParamValue(param, value);
    }

    /// <summary>
    /// Проверка типов отделки помещений.
    /// Все помещения, к которым относятся экземпляр отделки должны иметь одинаковый тип отделки.
    /// </summary>
    public bool CheckFinishingTypes() {
        List<string> finishingTypes = Rooms
            .Select(x => x.RoomFinishingType)
            .Distinct()
            .ToList();

        return finishingTypes.Count == 1;
    }

    /// <summary>
    /// Заполнение параметров отделки, универсальных для всех семейств отделки.
    /// </summary>
    public void UpdateFinishingParameters(FinishingCalculator calculator) {
        FinishingType finishingType = calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

        UpdateFromSharedParam(_paramConfig.FloorFinishingType1);
        UpdateFromSharedParam(_paramConfig.FloorFinishingType2);
        UpdateFromSharedParam(_paramConfig.FloorFinishingType3);
        UpdateFromSharedParam(_paramConfig.FloorFinishingType4);
        UpdateFromSharedParam(_paramConfig.FloorFinishingType5);

        UpdateFromSharedParam(_paramConfig.CeilingFinishingType1);
        UpdateFromSharedParam(_paramConfig.CeilingFinishingType2);
        UpdateFromSharedParam(_paramConfig.CeilingFinishingType3);
        UpdateFromSharedParam(_paramConfig.CeilingFinishingType4);
        UpdateFromSharedParam(_paramConfig.CeilingFinishingType5);

        UpdateFromSharedParam(_paramConfig.WallFinishingType1);
        UpdateFromSharedParam(_paramConfig.WallFinishingType2);
        UpdateFromSharedParam(_paramConfig.WallFinishingType3);
        UpdateFromSharedParam(_paramConfig.WallFinishingType4);
        UpdateFromSharedParam(_paramConfig.WallFinishingType5);
        UpdateFromSharedParam(_paramConfig.WallFinishingType6);
        UpdateFromSharedParam(_paramConfig.WallFinishingType7);
        UpdateFromSharedParam(_paramConfig.WallFinishingType8);
        UpdateFromSharedParam(_paramConfig.WallFinishingType9);
        UpdateFromSharedParam(_paramConfig.WallFinishingType10);

        UpdateFromSharedParam(_paramConfig.BaseboardFinishingType1);
        UpdateFromSharedParam(_paramConfig.BaseboardFinishingType2);
        UpdateFromSharedParam(_paramConfig.BaseboardFinishingType3);
        UpdateFromSharedParam(_paramConfig.BaseboardFinishingType4);
        UpdateFromSharedParam(_paramConfig.BaseboardFinishingType5);

        // Тип отделки
        UpdateFromKeyParam(Rooms, _paramConfig.FinishingType, ProjectParamsConfig.Instance.RoomFinishingType);

        // Имена и номера помещений
        UpdateFromSystemParam(Rooms, _paramConfig.FinishingRoomName, BuiltInParameter.ROOM_NAME);
        UpdateFromSystemParam(Rooms, _paramConfig.FinishingRoomNumber, BuiltInParameter.ROOM_NUMBER);
        UpdateFromSystemParam(finishingType.Rooms, _paramConfig.FinishingRoomNames, BuiltInParameter.ROOM_NAME);
        UpdateFromSystemParam(finishingType.Rooms, _paramConfig.FinishingRoomNumbers, BuiltInParameter.ROOM_NUMBER);

        // Габариты отделки 
        UpdateFromInstParam(_paramConfig.SizeArea, BuiltInParameter.HOST_AREA_COMPUTED);
        UpdateFromInstParam(_paramConfig.SizeVolume, BuiltInParameter.HOST_VOLUME_COMPUTED);
    }

    /// <summary>
    /// Заполнение параметров отделки, уникальных для определенного типа отделки.
    /// </summary>
    public abstract void UpdateCategoryParameters(FinishingCalculator calculator);

    public void ClearFinishingParameters() {
        _revitElement.RemoveParamValue(_paramConfig.FloorFinishingOrder);
        _revitElement.RemoveParamValue(_paramConfig.CeilingFinishingOrder);
        _revitElement.RemoveParamValue(_paramConfig.WallFinishingOrder);
        _revitElement.RemoveParamValue(_paramConfig.BaseboardFinishingOrder);
    }
}

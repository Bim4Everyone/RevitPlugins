using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitRooms.Models;

internal class WarningInfo {
    public static WarningInfo RedundantRooms { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = "Избыточные или не окруженные помещения.",
        Description = "Для запуска плагина следует исправить или удалить все неокруженные и избыточные зоны."
    };

    public static WarningInfo RedundantAreas { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = "Избыточные или не окруженные зоны.",
        Description = "Для запуска плагина следует исправить или удалить все неокруженные и избыточные помещения."
    };

    public static WarningInfo RequiredParams { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = "Не заполнен обязательный параметр \"{0}\".",
        Description = "У данных помещений не заполнен ключевой параметр. Для запуска плагина необходимо заполнить значение этого параметра у каждого помещения в списке."
    };

    public static WarningInfo NotEqualGroupType { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = "Не совпадают значения параметров групп \"{0} - {1}\"и типа групп.",
        Description = $"Для помещений  данной группы на указанных этажах не совпадает значение параметра \"{ProjectParamsConfig.Instance.RoomTypeGroupName.Name}\"."
            + Environment.NewLine
            + $"Для всех помещений в пределах одной группы и одного этажа параметры \"{ProjectParamsConfig.Instance.RoomTypeGroupName.Name}\" и \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\" должны соответствовать друг другу."
    };

    public static WarningInfo NotEqualMultiLevel { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = "Не совпадают значения параметров у многоуровневых квартир.",
        Description = $"Для помещений не совпадают значения параметров \"{SharedParamsConfig.Instance.RoomMultilevelGroup.Name}\" и \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\"."
    };

    public static WarningInfo ErrorMultiLevelRoom { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Error,
        Message = $"Ошибка в заполнении параметра \"{ProjectParamsConfig.Instance.IsRoomMainLevel.Name}\".",
        Description = $"Ошибка в заполнении параметра \"{ProjectParamsConfig.Instance.IsRoomMainLevel.Name}\". Параметр должен быть заполнен только для помещений одного этажа группы."
    };

    public static WarningInfo NotEqualSectionDoors { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Warning,
        Message = $"Проверка параметра \"{ProjectParamsConfig.Instance.RoomSectionName.Name}\" по дверям",
        Description = $"Обнаружены помещения, принадлежащие разным секциям, но имеющие доступ друг к другу через двери, окна (балконные блоки) или разделители помещений. Проверьте корректность параметра \"{ProjectParamsConfig.Instance.RoomSectionName.Name}\" для помещений, соединенных данными элементами."
    };

    public static WarningInfo NotEqualGroup { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Warning,
        Message = $"Проверка параметра \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\" по дверям и окнам",
        Description = $"Обнаружены помещения, принадлежащие разным квартирам, но имеющие доступ друг к другу через двери, окна (балконные блоки) или разделители помещений. Проверьте корректность параметра \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\" для помещений, соединенных данными элементами.."
    };

    public static WarningInfo CountourIntersectRooms { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Warning,
        Message = "Найдены самопересечения помещений.",
        Description = "Для данных помещений обнаружены самопересечения контуров."
    };

    public static WarningInfo BigChangesRoomAreas { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Info,
        Message = "Большие изменения в площади помещений."
    };

    public static WarningInfo BigChangesFlatAreas { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Info,
        Message = "Большие изменения в площади квартир."
    };


    public static WarningInfo BigChangesAreas { get; } = new WarningInfo() {
        TypeInfo = WarningTypeInfo.Info,
        Message = "Большие изменения в площади зон."
    };

    public string Message { get; set; }
    public string Description { get; set; }

    public WarningTypeInfo TypeInfo { get; set; }

    public WarningInfo FormatMessage(params string[] args) {
        return new WarningInfo() {
            TypeInfo = TypeInfo,
            Description = Description,
            Message = string.Format(Message, args)
        };
    }
}

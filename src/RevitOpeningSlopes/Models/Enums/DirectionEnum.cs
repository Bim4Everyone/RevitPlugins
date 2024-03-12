using System.ComponentModel;

namespace RevitOpeningSlopes.Models.Enums {
    internal enum DirectionEnum {
        [Description("Запуск линии вправо")]
        Right,
        [Description("Запуск линии влево")]
        Left,
        [Description("Запуск линии вперед")]
        Forward,
        [Description("Запуск линии назад")]
        Back,
        [Description("Запуск линии вверх")]
        Top,
        [Description("Запуск линии вниз")]
        Down
    }
}

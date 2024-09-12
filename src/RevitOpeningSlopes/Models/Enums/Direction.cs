using System.ComponentModel;

namespace RevitOpeningSlopes.Models.Enums {
    internal enum Direction {
        [Description("Запуск линии вправо")]
        Right,
        [Description("Запуск линии влево")]
        Left,
        [Description("Запуск линии вперед")]
        Forward,
        [Description("Запуск линии назад")]
        Backward,
        [Description("Запуск линии вверх")]
        Top,
        [Description("Запуск линии вниз")]
        Down
    }
}

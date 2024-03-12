using System.ComponentModel;

namespace RevitOpeningSlopes.Models.Enums {
    internal enum WindowsGetterMode {
        [Description("По предварительно выбранным окнам")]
        AlreadySelectedWindows,
        [Description("По выбранным вручную окнам")]
        ManuallySelectedWindows,
        [Description("Запуск линии вправо")]
        WindowsOnActiveView
    }
}

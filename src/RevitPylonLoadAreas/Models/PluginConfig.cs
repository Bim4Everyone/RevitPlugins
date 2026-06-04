using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models;

/// <summary>
/// Конфигурация плагина. В v1 UI не предусмотрен — все настройки берутся из дефолтов.
/// Когда появится UI, поля будут читаться/записываться через сериализатор Bim4Everyone.
/// </summary>
internal sealed class PluginConfig {
    /// <summary>
    /// Минимальная площадь дыры, которая еще считается значащей (в футах^2).
    /// Дыры меньшего размера сглаживаются и не участвуют в клиппинге ячеек.
    /// Дефолт — 1 м^2.
    /// </summary>
    public double HoleAreaThresholdSqFeet { get; set; } = GeometryTolerance.SqMetersToSqFeet(1.0);

    /// <summary>
    /// Шаг семплирования точек по осевой линии стены (в футах).
    /// Дефолт — 300 мм.
    /// </summary>
    public double WallSampleStepFeet { get; set; } = GeometryTolerance.MmToFeet(300.0);

    /// <summary>
    /// Запас для построения "мирового прямоугольника" вокруг плиты для обрезки бесконечных ячеек.
    /// Доля от диагонали ограничивающей рамки плиты. Дефолт — 0.5 (50%).
    /// </summary>
    public double WorldRectMarginRatio { get; set; } = 0.5;

    /// <summary>
    /// Создает дефолтную конфигурацию.
    /// </summary>
    public static PluginConfig CreateDefault() {
        return new PluginConfig();
    }
}

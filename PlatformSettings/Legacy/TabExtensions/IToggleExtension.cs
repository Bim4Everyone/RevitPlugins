#region Namespaces

#endregion

namespace PlatformSettings.Legacy.TabExtensions {
    /// <summary>
    /// Интерфейс переключателя расширения (включение/отключение)
    /// </summary>
    public interface IToggleExtension {
        /// <summary>
        /// Переключает расширение.
        /// </summary>
        /// <param name="enabled">Состояние расширения.</param>
        void Toggle(bool enabled);
    }
}

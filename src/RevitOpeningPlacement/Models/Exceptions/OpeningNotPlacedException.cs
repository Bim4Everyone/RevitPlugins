using System;

namespace RevitOpeningPlacement.Models.Exceptions {
    /// <summary>
    /// Исключение <see cref="OpeningNotPlacedException"/> выбрасывается, когда не удалось создать семейство задания на отверстие
    /// </summary>
    internal class OpeningNotPlacedException : Exception {
        /// <summary>
        /// Создает экземпляр <see cref="OpeningNotPlacedException"/>
        /// </summary>
        public OpeningNotPlacedException() { }

        /// <summary>
        /// Создает экземпляр <see cref="OpeningNotPlacedException"/> с заданным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public OpeningNotPlacedException(string message) : base(message) { }
    }
}

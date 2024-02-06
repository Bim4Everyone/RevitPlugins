using System;

namespace RevitOpeningPlacement.Models.Exceptions {
    /// <summary>
    /// Исключение выбрасывается, если какой-то размер геометрического элемента слишком мал
    /// </summary>
    internal class SizeTooSmallException : Exception {
        /// <summary>
        /// Инициализирует экземпляр <see cref="SizeTooSmallException"/>
        /// </summary>
        public SizeTooSmallException() { }

        /// <summary>
        /// Инициализирует экземпляр <see cref="SizeTooSmallException"/> с заданным сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public SizeTooSmallException(string message) : base(message) { }
    }
}

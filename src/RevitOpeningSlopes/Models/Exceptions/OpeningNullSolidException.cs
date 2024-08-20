using System;

namespace RevitOpeningSlopes.Models.Exceptions {
    /// <summary>
    /// Исключение <see cref="OpeningNullSolidException"/> выбрасывается, когда у окна нет твердотельной геометрии
    /// </summary>
    internal class OpeningNullSolidException : Exception {
        /// <summary>
        /// Создает экземпляр <see cref="OpeningNullSolidException"/>
        /// </summary>
        public OpeningNullSolidException() { }

        /// <summary>
        /// Создает экземпляр <see cref="OpeningNullSolidException"/> с заданным сообщением
        /// </summary> 
        /// <param name="message"></param>
        public OpeningNullSolidException(string message) : base(message) { }
    }
}

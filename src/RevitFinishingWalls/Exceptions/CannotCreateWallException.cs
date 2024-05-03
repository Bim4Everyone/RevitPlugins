using System;

namespace RevitFinishingWalls.Exceptions {
    /// <summary>
    /// Исключение, когда не удалось создать отделочную стену
    /// </summary>
    internal class CannotCreateWallException : Exception {
        public CannotCreateWallException() {
        }

        public CannotCreateWallException(string message) : base(message) {
        }
    }
}

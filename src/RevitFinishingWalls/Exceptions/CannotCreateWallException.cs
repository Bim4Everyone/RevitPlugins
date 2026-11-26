using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.Exceptions {
    /// <summary>
    /// Исключение, когда не удалось создать отделочную стену
    /// </summary>
    internal class CannotCreateWallException : Exception {
        public CannotCreateWallException() {
        }

        public CannotCreateWallException(string message) : base(message) {
        }

        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="elementIds">Элементы, из-за которых не удалось создать стену</param>
        public CannotCreateWallException(string message, ICollection<ElementId> elementIds)
            : base(message) {
            ElementIds = elementIds?
                             .Where(id => id != null && id != ElementId.InvalidElementId)
                             .ToHashSet()
                         ?? [];
        }

        /// <summary>
        /// Элементы, из-за которых не удалось создать стену
        /// </summary>
        public IReadOnlyCollection<ElementId> ElementIds { get; } = [];
    }
}

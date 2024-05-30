using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    /// <summary>
    /// Модель представления ошибки плагина
    /// </summary>
    internal class ErrorViewModel : BaseViewModel, IElementsContainer {
        public ErrorViewModel(string title, string message, HashSet<ElementId> dependentElements) {
            if(string.IsNullOrWhiteSpace(title)) {
                throw new System.ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
            }

            if(string.IsNullOrWhiteSpace(message)) {
                throw new System.ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
            }

            Title = title;
            Message = message;
            DependentElements = dependentElements ?? new HashSet<ElementId>();
        }

        public ErrorViewModel(string title, string message, ElementId dependentElement)
            : this(title, message, new HashSet<ElementId>(new ElementId[] { dependentElement })) { }


        /// <summary>Заголовок ошибки</summary>
        public string Title { get; }

        /// <summary>Сообщение об ошибке</summary>
        public string Message { get; }

        public IReadOnlyCollection<ElementId> DependentElements { get; }
    }
}

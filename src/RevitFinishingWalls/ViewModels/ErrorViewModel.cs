using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    /// <summary>
    /// Модель представления ошибки плагина
    /// </summary>
    internal class ErrorViewModel : BaseViewModel {
        private readonly IReadOnlyCollection<ElementId> _dependentElements;

        public ErrorViewModel(string message, HashSet<ElementId> dependentElements) {
            if(string.IsNullOrWhiteSpace(message)) {
                throw new System.ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
            }

            Message = message;
            _dependentElements = dependentElements ?? new HashSet<ElementId>();
        }

        public ErrorViewModel(string message, ElementId dependentElement)
            : this(message, new HashSet<ElementId>(new ElementId[] { dependentElement })) { }


        public string Message { get; }

        public IReadOnlyCollection<ElementId> DependentElements { get; }
    }
}

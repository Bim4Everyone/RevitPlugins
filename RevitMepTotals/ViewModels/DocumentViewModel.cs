using System;

using dosymep.WPF.ViewModels;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.ViewModels {
    /// <summary>
    /// Модель представления выбранного документа ревита для обработки
    /// </summary>
    internal class DocumentViewModel : BaseViewModel {
        private readonly IDocument _document;

        /// <summary>
        /// Конструктор класса модели представления выбранного документа ревита для обработки
        /// </summary>
        /// <param name="document">Документ</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DocumentViewModel(IDocument document) {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }


        public string Name => _document.Name;


        public override string ToString() {
            return Name;
        }

        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is DocumentViewModel vmOther)
                && string.Equals(vmOther.Name, Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public IDocument GetDocument() {
            return _document;
        }
    }
}

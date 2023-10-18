using System;

using dosymep.WPF.ViewModels;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.ViewModels {
    /// <summary>
    /// Модель представления выбранного документа ревита для обработки
    /// </summary>
    internal class DocumentViewModel : BaseViewModel, IEquatable<DocumentViewModel> {
        private readonly IDocument _document;

        /// <summary>
        /// Конструктор класса модели представления выбранного документа ревита для обработки
        /// </summary>
        /// <param name="document">Документ</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DocumentViewModel(IDocument document) {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }


        public string ShortName => _document.Name;


        public string Name => _document.Path;


        public override string ToString() {
            return Name;
        }

        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is DocumentViewModel vmOther)
                && Equals(vmOther);

        }

        public override int GetHashCode() {
            return ShortName.ToLower().GetHashCode();
        }

        public IDocument GetDocument() {
            return _document;
        }

        public bool Equals(DocumentViewModel other) {
            return (other != null) && string.Equals(other.ShortName, ShortName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

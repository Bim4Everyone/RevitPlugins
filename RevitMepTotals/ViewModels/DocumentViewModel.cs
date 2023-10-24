using System;
using System.Collections.Generic;

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
            return Equals(obj as DocumentViewModel);

        }

        public override int GetHashCode() {
            int hashCode = 1671284716;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortName);
            return hashCode;
        }

        public bool Equals(DocumentViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return ShortName == other.ShortName;
        }

        public IDocument GetDocument() {
            return _document;
        }
    }
}

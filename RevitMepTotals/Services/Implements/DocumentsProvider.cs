using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements {
    internal class DocumentsProvider : IDocumentsProvider {
        private readonly IOpenFileDialogService _openFileDialogService;

        public DocumentsProvider(IOpenFileDialogService openFileDialogService) {
            _openFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        }


        public ICollection<IDocument> GetDocuments() {
            var openWindow = _openFileDialogService;
            openWindow.Filter = "Revit projects |*.rvt";
            openWindow.Multiselect = true;
            if(openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) {
                return new HashSet<IDocument>(openWindow.Files.Select(file => new RevitDocument(file)));
            } else {
                return Array.Empty<IDocument>();
            }
        }
    }
}

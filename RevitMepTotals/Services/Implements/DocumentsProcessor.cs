using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements {
    internal class DocumentsProcessor : IDocumentsProcessor {
        private readonly Application _application;

        public DocumentsProcessor(Application application) {
            _application = application ?? throw new System.ArgumentNullException(nameof(application));
        }


        public void ProcessDocuments(ICollection<IDocument> documents) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyExplorer.Models {
    internal class RevitRepository {
        public event Action ApplicationIdle;

        private readonly UIApplication _uiApplication;

        private readonly RevitExternalEvent _loadFamilyHandler;
        private readonly ExternalEvent _loadFamilyExternalEvent;


        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;

            _loadFamilyHandler = new RevitExternalEvent("Загрузка семейства");
            _loadFamilyExternalEvent = ExternalEvent.Create(_loadFamilyHandler);

            _uiApplication.Idling += (s, e) => ApplicationIdle?.Invoke();
        }

        public Application Application {
            get { return _uiApplication.Application; }
        }

        public Document Document {
            get { return _uiApplication.ActiveUIDocument.Document; }
        }

        public void LoadFamily(FileInfo familyFile) {
            _loadFamilyHandler.TransactionName = $"Загрузка семейства \"{familyFile.Name}\"";
            _loadFamilyHandler.ExternalAction = app => app.ActiveUIDocument.Document.LoadFamily(familyFile.FullName);

            _loadFamilyExternalEvent.Raise();
        }

        public void PlaceFamilySymbol(string familySymbolName) {
            try {
                FamilySymbol familySymbol = (FamilySymbol) new FilteredElementCollector(Document)
                    .OfClass(typeof(FamilySymbol))
                    .FirstOrDefault(item => item.Name.Equals(familySymbolName));

                if(familySymbol != null) {
                    _uiApplication.ActiveUIDocument.PromptForFamilyInstancePlacement(familySymbol);
                }
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {

            }
        }

        public bool IsInsertedFamilyFile(FileInfo familyFile) {
            var families = new FilteredElementCollector(Document)
                .OfClass(typeof(Family))
                .ToElements();

            return families.Any(item => item.Name.Equals(Path.GetFileNameWithoutExtension(familyFile.Name)));
        }

        public IEnumerable<string> GetFamilyTypes(FileInfo familyFile) {
            var familyDocument = Application.OpenDocumentFile(familyFile.FullName);
            try {
                if(!familyDocument.IsFamilyDocument) {
                    throw new ArgumentException($"Переданный файл не является документом семейства. {familyFile}");
                }

                return familyDocument.FamilyManager.Types
                    .Cast<FamilyType>()
                    .Select(item => string.IsNullOrEmpty(item.Name) ? familyDocument.Title : item.Name)
                    .ToList();
            } finally {
                familyDocument.Close(false);
            }
        }
    }
}

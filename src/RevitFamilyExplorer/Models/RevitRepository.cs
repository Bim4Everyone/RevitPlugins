using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyExplorer.Models {
    internal class RevitRepository {
        private readonly UIApplication _uiApplication;
        private readonly RevitExternalEvent _loadFamilyHandler;
        private readonly RevitExternalEvent _loadFamilySymbolHandler;


        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
            _loadFamilyHandler = new RevitExternalEvent("Загрузка семейства");
            _loadFamilySymbolHandler = new RevitExternalEvent("Загрузка типоразмера");
        }

        public Application Application {
            get { return _uiApplication.Application; }
        }

        public Document Document {
            get { return _uiApplication.ActiveUIDocument.Document; }
        }

        public async Task LoadFamilyAsync(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            _loadFamilyHandler.TransactionName = $"Загрузка семейства \"{familyFile.Name}\"";
            _loadFamilyHandler.ExternalAction = app => app.ActiveUIDocument.Document.LoadFamily(familyFile.FullName);

            await _loadFamilyHandler.Raise();
        }

        public async Task LoadFamilySymbolAsync(FileInfo familyFile, string familySymbolName) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            if(string.IsNullOrEmpty(familySymbolName)) {
                throw new ArgumentException($"'{nameof(familySymbolName)}' cannot be null or empty.", nameof(familySymbolName));
            }

            _loadFamilySymbolHandler.TransactionName = $"Загрузка типоразмера \"{familyFile.Name}\"";
            _loadFamilySymbolHandler.ExternalAction = app => app.ActiveUIDocument.Document.LoadFamilySymbol(familyFile.FullName, familySymbolName);

            await _loadFamilySymbolHandler.Raise();
        }

        public bool IsInsertedFamilyFile(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            return GetFamily(familyFile) != null;
        }

        public bool IsInsertedFamilySymbol(FileInfo familyFile, string familySymbolName) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            return GetFamilySymbol(familyFile, familySymbolName) != null;
        }

        public IEnumerable<string> GetFamilyTypes(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            if(HasTableFamilySymbols(familyFile)) {
                FileInfo tableFamilySymbols = GetTableFamilySymbols(familyFile);
                return GetFamilySymbolsByTable(tableFamilySymbols);
            }

            Document familyDocument = Application.OpenDocumentFile(familyFile.FullName);
            try {
                if(!familyDocument.IsFamilyDocument) {
                    throw new ArgumentException($"Переданный файл не является документом семейства. {familyFile}");
                }

                List<string> familyTypes = familyDocument.FamilyManager.Types
                    .Cast<FamilyType>()
                    .Where(item => !string.IsNullOrEmpty(item.Name.Trim()))
                    .Select(item => item.Name)
                    .ToList();

                return familyTypes.Count == 0 ? (new[] { GetFamilyName(familyFile) }) : (IEnumerable<string>) familyTypes;
            } finally {
                familyDocument.Close(false);
            }
        }

        public string GetFamilyName(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            return Path.GetFileNameWithoutExtension(familyFile.Name);
        }

        public Family GetFamily(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            var families = new FilteredElementCollector(Document)
                .OfClass(typeof(Family))
                .ToElements();

            string familyName = GetFamilyName(familyFile);
            return (Family) families.FirstOrDefault(item => item.Name.Equals(familyName));
        }

        public IEnumerable<FamilySymbol> GetFamilySymbols(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            Family family = GetFamily(familyFile);
            if(family == null) {
                return Enumerable.Empty<FamilySymbol>();
            }

            return family.GetFamilySymbolIds()
                .Select(item => Document.GetElement(item))
                .OfType<FamilySymbol>();
        }

        public FamilySymbol GetFamilySymbol(FileInfo familyFile, string familySymbolName) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            if(string.IsNullOrEmpty(familySymbolName)) {
                throw new ArgumentException($"'{nameof(familySymbolName)}' cannot be null or empty.", nameof(familySymbolName));
            }

            return GetFamilySymbols(familyFile).FirstOrDefault(item => item.Name.Equals(familySymbolName));
        }

        public bool CanPlaceFamilySymbol(FileInfo familyFile, string familySymbolName) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            if(string.IsNullOrEmpty(familySymbolName)) {
                throw new ArgumentException($"'{nameof(familySymbolName)}' cannot be null or empty.", nameof(familySymbolName));
            }

            FamilySymbol familySymbol = GetFamilySymbol(familyFile, familySymbolName);
            if(familySymbol == null) {
                return false;
            }

            return _uiApplication.ActiveUIDocument.CanPlaceElementType(familySymbol);
        }

        public async Task PlaceFamilySymbolAsync(FileInfo familyFile, string familySymbolName) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            if(string.IsNullOrEmpty(familySymbolName)) {
                throw new ArgumentException($"'{nameof(familySymbolName)}' cannot be null or empty.", nameof(familySymbolName));
            }

            await LoadFamilySymbolAsync(familyFile, familySymbolName);
            FamilySymbol familySymbol = GetFamilySymbol(familyFile, familySymbolName);
            if(familySymbol != null) {
                _uiApplication.ActiveUIDocument.PromptForFamilyInstancePlacement(familySymbol);
            }
        }

        public bool HasTableFamilySymbols(FileInfo familyFile) {
            if(familyFile is null) {
                throw new ArgumentNullException(nameof(familyFile));
            }

            return GetTableFamilySymbols(familyFile).Exists;
        }

        private FileInfo GetTableFamilySymbols(FileInfo familyFile) {
            return new FileInfo(Path.ChangeExtension(familyFile.FullName, ".txt"));
        }

        private IEnumerable<string> GetFamilySymbolsByTable(FileInfo tableFamilySymbols) {
            return File.ReadAllLines(tableFamilySymbols.FullName)
                .Select(item => item.Split(','))
                .Select(item => item.FirstOrDefault())
                .Where(item => !string.IsNullOrEmpty(item))
                .OrderBy(item => item);
        }
    }
}
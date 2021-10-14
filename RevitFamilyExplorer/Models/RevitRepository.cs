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


        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
            _loadFamilyHandler = new RevitExternalEvent("Загрузка семейства");
        }

        public Application Application {
            get { return _uiApplication.Application; }
        }

        public Document Document {
            get { return _uiApplication.ActiveUIDocument.Document; }
        }

        public async Task LoadFamilyAsync(FileInfo familyFile) {
            _loadFamilyHandler.TransactionName = $"Загрузка семейства \"{familyFile.Name}\"";
            _loadFamilyHandler.ExternalAction = app => app.ActiveUIDocument.Document.LoadFamily(familyFile.FullName);
            
            await _loadFamilyHandler.Raise();
        }

        public IEnumerable<FamilySymbol> GetFamilySymbols(FileInfo familyFile) {
            var families = new FilteredElementCollector(Document)
                .OfClass(typeof(Family))
                .ToElements();

            var family = (Family) families.FirstOrDefault(item => item.Name.Equals(Path.GetFileNameWithoutExtension(familyFile.Name)));
            return family.GetFamilySymbolIds().Select(item => Document.GetElement(item)).OfType<FamilySymbol>();
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

        public BitmapSource GetFamilySymbolIcon(FamilySymbol familySymbol) {
            Bitmap bitmap = familySymbol.GetPreviewImage(new Size(96, 96));
            var bitmapImage = new BitmapImage();

            using(var memoryStream = new MemoryStream()) {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                
                memoryStream.Position = default;                
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }
}

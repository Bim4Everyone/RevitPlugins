using System.IO;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitToMongoDB.Connections;
using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels;

namespace RevitToMongoDB {
    [Transaction(TransactionMode.Manual)]
    public class RevitToMongoDBCommand : IExternalCommand {
        private string _configPath = "C:\\Users\\ramazanov_s\\Desktop\\config.txt";
        private string _connectionString;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            using(StreamReader reader = new StreamReader(_configPath)) {
                _connectionString = reader.ReadLine();
            }
            var elementFactory = new ElementCreator();
            var mongoSettings = new MongoDBSettings(_connectionString, commandData.Application.ActiveUIDocument.Document);
            var elementRepository = new MongoDBConnection(_connectionString, mongoSettings);
            var revitExporter = new RevitExporter(commandData.Application.ActiveUIDocument.Document, elementFactory, elementRepository);
            revitExporter.Export();
            return Result.Succeeded;
        }
    }
}

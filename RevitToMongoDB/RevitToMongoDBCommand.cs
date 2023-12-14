using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using MongoDB.Driver;

using RevitToMongoDB.Connections;
using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels;
using RevitToMongoDB.ViewModels.Interfaces;

namespace RevitToMongoDB {
    [Transaction(TransactionMode.Manual)]
    public class RevitToMongoDBCommand : IExternalCommand { 
        string configPath = "C:\\Users\\ramazanov_s\\Desktop\\config.txt";
        string connectionString;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            using(StreamReader reader = new StreamReader(configPath))
                connectionString = reader.ReadLine();
            var elementFactory = new ElementCreator();
            var mongoSettings = new MongoDBSettings(connectionString, commandData.Application.ActiveUIDocument.Document); 
            var elementRepository = new MongoDBConnection(connectionString, mongoSettings);
            var revitExporter = new RevitExporter(commandData.Application.ActiveUIDocument.Document,elementFactory, elementRepository);
            revitExporter.Export();
            return Result.Succeeded;
        }
    }
}

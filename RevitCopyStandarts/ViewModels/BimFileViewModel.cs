using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitCopyStandarts.Commands;

namespace RevitCopyStandarts.ViewModels {
    public class BimFileViewModel {
        private static Dictionary<string, string> _commandsMap = new Dictionary<string, string>() {
            { "BrowserOrganization", "Autodesk.Revit.DB.BrowserOrganization" },
            //{ "Стадии", "Autodesk.Revit.DB.Phase" },
            { "ObjectStyles", "RevitCopyStandarts.Commands.CopyObjectStylesCommand" },
            { "ProjectInfo", "Autodesk.Revit.DB.ProjectInfo" },
            { "WallTypes", "RevitCopyStandarts.Commands.CopyWallTypesCommand" },
            { "CurtainTypes", "RevitCopyStandarts.Commands.CopyCurtainTypesCommand" },
            { "FloorType", "Autodesk.Revit.DB.FloorType" },
        };


        private readonly FileInfo _fileInfo;

        private readonly Application _application;
        private readonly Document _targetDocument;

        public BimFileViewModel(FileInfo fileInfo, Application application, Document targetDocument) {
            _fileInfo = fileInfo;
            _application = application;
            _targetDocument = targetDocument;

            CopyObjectsCommand = new RelayCommand(CopyObjects);
        }

        public string Name {
            get { return _fileInfo.Name; }
        }

        public DateTime CreationTime {
            get { return _fileInfo.CreationTime; }
        }

        public DateTime ModifiedTime {
            get { return _fileInfo.LastWriteTime; }
        }

        public ICommand CopyObjectsCommand { get; }

        private void CopyObjects(object p) {
            Document sourceDocument = _application.OpenDocumentFile(_fileInfo.FullName);
            try {
                var commands = new List<ICopyStandartsCommand>() {
                    //new CopyViewTemplatesCommand(sourceDocument, _targetDocument),
                    ////new CopyFamiliesCommand(sourceDocument, _targetDocument),
                    //new CopyViewSchedulesCommand(sourceDocument, _targetDocument),
                    //new CopyMaterialsCommand(sourceDocument, _targetDocument),
                    //new CopyViewLegendsCommand(sourceDocument, _targetDocument),
                    //new CopyFiltersCommand(sourceDocument, _targetDocument),
                    //new CopyObjectStylesCommand(sourceDocument, _targetDocument)
                    new CopyColorFillSchemesCommand(sourceDocument, _targetDocument)
                };                

                //commands.AddRange(GetOptionalStandarts(sourceDocument));
                commands.ForEach(command => command.Execute());
            } finally {
                sourceDocument.Close(false);
            }
        }

        private IEnumerable<ICopyStandartsCommand> GetOptionalStandarts(Document sourceDocument) {
            if(string.IsNullOrEmpty(sourceDocument.ProjectInformation.Status)) {
                return Enumerable.Empty<ICopyStandartsCommand>();
            }

            return sourceDocument.ProjectInformation.Status
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => GetCopyStandartsCommand(sourceDocument, item));
        }

        private ICopyStandartsCommand GetCopyStandartsCommand(Document sourceDocument, string className) {
            Type type = Type.GetType(_commandsMap[className]);
            if(type == null) {
                return new CopyOptionalStandartsCommand(sourceDocument, _targetDocument) { BuiltInCategoryName = _commandsMap[className] };
            }

            return (ICopyStandartsCommand) Activator.CreateInstance(type, sourceDocument, _targetDocument);
        }
    }

    public class CustomCopyHandler : IDuplicateTypeNamesHandler {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(
          DuplicateTypeNamesHandlerArgs args) {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}

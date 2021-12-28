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

using dosymep.WPF.Commands;

using RevitCopyStandarts.Commands;

namespace RevitCopyStandarts.ViewModels {
    public class BimFileViewModel {
        private static readonly Dictionary<string, string> _commandsMap = new Dictionary<string, string>() {
            { "BrowserOrganization", "Autodesk.Revit.DB.BrowserOrganization" },
            { "ObjectStyles", "RevitCopyStandarts.Commands.CopyObjectStylesCommand" },
            { "ProjectInfo", "Autodesk.Revit.DB.ProjectInfo" },
            { "GlobalParameter", "Autodesk.Revit.DB.GlobalParameter" },
            { "PrintSettings", "Autodesk.Revit.DB.PrintSetting" },
            //{ "LinePattern", "Autodesk.Revit.DB.LinePattern" },
            { "WallTypes", "RevitCopyStandarts.Commands.CopyWallTypesCommand" },
            { "CurtainTypes", "RevitCopyStandarts.Commands.CopyCurtainTypesCommand" },
            { "RoofType", "Autodesk.Revit.DB.RoofType" },
            { "CeilingType", "Autodesk.Revit.DB.CeilingType" },
            { "StairsType", "Autodesk.Revit.DB.Architecture.StairsType" },
            { "RailingType", "Autodesk.Revit.DB.Architecture.RailingType" },
            { "ColorFillSchemes", "RevitCopyStandarts.Commands.CopyColorFillSchemesCommand" },
            { "TopRailType", "Autodesk.Revit.DB.Architecture.TopRailType" },
            { "HandRailType", "Autodesk.Revit.DB.Architecture.HandRailType" },
            { "PipeType", "Autodesk.Revit.DB.Plumbing.PipeType" },
            { "FlexPipeType", "Autodesk.Revit.DB.Plumbing.FlexPipeType" },
            { "PipeInsulationType", "Autodesk.Revit.DB.Plumbing.PipeInsulationType" },
            { "PipingSystem", "Autodesk.Revit.DB.Plumbing.PipingSystem" },
            { "PipeSettings", "Autodesk.Revit.DB.Plumbing.PipeSettings" },
            { "DuctType", "Autodesk.Revit.DB.Mechanical.DuctType" },
            { "FlexDuctType", "Autodesk.Revit.DB.Mechanical.FlexDuctType" },
            { "DuctInsulationType", "Autodesk.Revit.DB.Mechanical.DuctInsulationType" },
            { "DuctSettings", "Autodesk.Revit.DB.Mechanical.DuctSettings" },
            { "CableTraySettings", "Autodesk.Revit.DB.Electrical.CableTraySettings" },
            { "CableTraySizes", "Autodesk.Revit.DB.Electrical.CableTraySizes" },
            { "ElectricalSetting", "Autodesk.Revit.DB.Electrical.ElectricalSetting" },
            { "ElectricalLoadClassification", "Autodesk.Revit.DB.Electrical.ElectricalLoadClassification" },
            { "VoltageType", "Autodesk.Revit.DB.Electrical.VoltageType" },
            { "ConduitSettings", "Autodesk.Revit.DB.Electrical.ConduitSettings" },
            //{ "ConduitSize", "Autodesk.Revit.DB.Electrical.ConduitSize" },
            { "RebarBarType", "Autodesk.Revit.DB.Structure.RebarBarType" },
            { "WallFoundationType", "Autodesk.Revit.DB.WallFoundationType" },
            { "PanelScheduleTemplate", "Autodesk.Revit.DB.Electrical.PanelScheduleTemplate" },
            { "StructuralSettings", "Autodesk.Revit.DB.Structure.StructuralSettings" },
            { "FloorType", "RevitCopyStandarts.Commands.CopyFloorTypeCommand" },
            { "FoundationSlab", "RevitCopyStandarts.Commands.CopyFoundationSlabCommand" },
            { "SlabEdgeType", "Autodesk.Revit.DB.SlabEdgeType" },
            { "DuctSystemType", "Autodesk.Revit.DB.Mechanical.MechanicalSystemType" },
            { "GutterType", "Autodesk.Revit.DB.Architecture.GutterType" },
            { "BuildingPadType", "Autodesk.Revit.DB.BuildingPadType" },
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
                    new CopyViewTemplatesCommand(sourceDocument, _targetDocument),
                    //new CopyFamiliesCommand(sourceDocument, _targetDocument),
                    new CopyViewSchedulesCommand(sourceDocument, _targetDocument),
                    new CopyMaterialsCommand(sourceDocument, _targetDocument),
                    new CopyViewLegendsCommand(sourceDocument, _targetDocument),
                    new CopyFiltersCommand(sourceDocument, _targetDocument)
                };

                commands.AddRange(GetOptionalStandarts(sourceDocument));
                commands.ForEach(command => command.Execute());
                
                System.Windows.MessageBox.Show("Копирование выполнено успешно!", "Сообщение", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
                .Select(item => GetCopyStandartsCommand(sourceDocument, item.Trim()));
        }

        private ICopyStandartsCommand GetCopyStandartsCommand(Document sourceDocument, string className) {
            if(_commandsMap.TryGetValue(className, out string commandName)) {
                Type type = Type.GetType(commandName);
                if(type == null) {
                    return new CopyOptionalStandartsCommand(sourceDocument, _targetDocument) { Name = className, BuiltInCategoryName = commandName };
                }

                return (ICopyStandartsCommand) Activator.CreateInstance(type, sourceDocument, _targetDocument);
            }

            throw new ArgumentException($"Неизвестное наименование команды \"{className}\".");
        }
    }

    public class CustomCopyHandler : IDuplicateTypeNamesHandler {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(
          DuplicateTypeNamesHandlerArgs args) {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}

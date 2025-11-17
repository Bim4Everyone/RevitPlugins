using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyStandarts.Models;
using RevitCopyStandarts.Models.Commands;

namespace RevitCopyStandarts.ViewModels;

internal class BimFileViewModel : BaseViewModel {
    private static readonly Dictionary<string, string> _commandsMap = new() {
        { "BrowserOrganization", "Autodesk.Revit.DB.BrowserOrganization" },
        { "ObjectStyles", "RevitCopyStandarts.Models.Commands.CopyObjectStylesCommand" },
        { "ProjectInfo", "Autodesk.Revit.DB.ProjectInfo" },
        { "GlobalParameter", "Autodesk.Revit.DB.GlobalParameter" },
        { "PrintSettings", "Autodesk.Revit.DB.PrintSetting" },
        // { "LinePattern", "Autodesk.Revit.DB.LinePattern" },
        { "WallTypes", "RevitCopyStandarts.Models.Commands.CopyWallTypesCommand" },
        { "CurtainTypes", "RevitCopyStandarts.Models.Commands.CopyCurtainTypesCommand" },
        { "RoofType", "Autodesk.Revit.DB.RoofType" },
        { "CeilingType", "Autodesk.Revit.DB.CeilingType" },
        { "StairsType", "Autodesk.Revit.DB.Architecture.StairsType" },
        { "RailingType", "Autodesk.Revit.DB.Architecture.RailingType" },
        { "ColorFillSchemes", "RevitCopyStandarts.Models.Commands.CopyColorFillSchemesCommand" },
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
        // { "ConduitSize", "Autodesk.Revit.DB.Electrical.ConduitSize" },
        { "RebarBarType", "Autodesk.Revit.DB.Structure.RebarBarType" },
        { "WallFoundationType", "Autodesk.Revit.DB.WallFoundationType" },
        { "PanelScheduleTemplate", "Autodesk.Revit.DB.Electrical.PanelScheduleTemplate" },
        { "StructuralSettings", "Autodesk.Revit.DB.Structure.StructuralSettings" },
        { "FloorType", "RevitCopyStandarts.Models.Commands.CopyFloorTypeCommand" },
        { "FoundationSlab", "RevitCopyStandarts.Models.Commands.CopyFoundationSlabCommand" },
        { "SlabEdgeType", "Autodesk.Revit.DB.SlabEdgeType" },
        { "DuctSystemType", "Autodesk.Revit.DB.Mechanical.MechanicalSystemType" },
        { "GutterType", "Autodesk.Revit.DB.Architecture.GutterType" },
        { "BuildingPadType", "Autodesk.Revit.DB.BuildingPadType" },
        { "Parameters", "RevitCopyStandarts.Models.Commands.CopyParametersCommand" },
        { "GlobalParameters", "Autodesk.Revit.DB.GlobalParameter" },
        { "SharedParameters", "RevitCopyStandarts.Models.Commands.CopySharedParametersCommand" },
        { "ProjectParameters", "RevitCopyStandarts.Models.Commands.CopyProjectParametersCommand" }
    };

    private readonly FileInfo _fileInfo;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IProgressDialogFactory _progressDialogFactory;

    public BimFileViewModel(
        string name, 
        FileInfo fileInfo, 
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IProgressDialogFactory progressDialogFactory) {
        Name = name;

        _fileInfo = fileInfo;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _progressDialogFactory = progressDialogFactory;

        CopyObjectsCommand = RelayCommand.CreateAsync(CopyObjectsAsync);
    }

    public string Name { get; }

    public DateTime CreationTime => _fileInfo.CreationTime;

    public DateTime ModifiedTime => _fileInfo.LastWriteTime;

    public ICommand CopyObjectsCommand { get; }

    private async Task CopyObjectsAsync() {
        var sourceDocument = _revitRepository.OpenDocumentFile(_fileInfo.FullName);
        try {
            var commands = new List<ICopyStandartsCommand> {
                new CopyViewTemplatesCommand(sourceDocument, _revitRepository.Document, _localizationService),
                new CopyViewSchedulesCommand(sourceDocument, _revitRepository.Document, _localizationService),
                new CopyMaterialsCommand(sourceDocument, _revitRepository.Document, _localizationService),
                new CopyViewLegendsCommand(sourceDocument, _revitRepository.Document, _localizationService),
                new CopyFiltersCommand(sourceDocument, _revitRepository.Document, _localizationService)
            };

            commands.AddRange(GetOptionalStandarts(sourceDocument));
            commands.AddRange(GetIdOptionalStandarts(sourceDocument));
            commands.AddRange(GetFamilyStandarts(sourceDocument));

            using(var window = _progressDialogFactory.CreateDialog()) {
                window.MaxValue = commands.Count;
                window.DisplayTitleFormat = _localizationService.GetLocalizedString("ProgressDialog.Title");;
                window.Show();

                int counter = 1;
                var progress = window.CreateProgress();
                var cancellationToken = window.CreateCancellationToken();
                foreach(var command in commands) {
                    progress.Report(counter++);
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    command.Execute();
                }
            }

            await _notificationService.CreateNotification(
                    _localizationService.GetLocalizedString("Notification.Title"),
                    _localizationService.GetLocalizedString("Notification.Message"))
                .ShowAsync();
        } finally {
            sourceDocument.Close(false);
        }
    }

    /// <summary>
    /// Метод для добавления команд по загрузке семейств в случае, если в Адресс прописаны пути
    /// </summary>
    private IEnumerable<ICopyStandartsCommand> GetFamilyStandarts(Document sourceDocument) {
        if(string.IsNullOrEmpty(sourceDocument.ProjectInformation.Address) 
            || !sourceDocument.ProjectInformation.Address.Contains(':')) {
            return [];
        }

        var address = sourceDocument.ProjectInformation.Address;
        var t = address.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

        var test = File.Exists(t.First());
        var test2 = t.First().Trim();

        var f = GetLoadFamilyCommand(sourceDocument, t.First().Trim());

        //var temp = sourceDocument.ProjectInformation.Address
        //    .Split(['\n'], StringSplitOptions.RemoveEmptyEntries)
        //    .Select(item => GetCopyStandartsCommand(sourceDocument, item.Trim()))
        //    .ToList();

        return [f];
    }



    private ICopyStandartsCommand GetLoadFamilyCommand(Document sourceDocument, string path) {
        if(File.Exists(path)) {
            return new LoadFamilyCommand(sourceDocument, _revitRepository.Document, _localizationService) {
                Path = path
            };
        }

        throw new ArgumentException(_localizationService.GetLocalizedString("Exceptions.PathToFamlyFilyNotExists", path));
    }



    private IEnumerable<ICopyStandartsCommand> GetOptionalStandarts(Document sourceDocument) {
        if(string.IsNullOrEmpty(sourceDocument.ProjectInformation.Status)) {
            return [];
        }

        return sourceDocument.ProjectInformation.Status
            .Split([';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(item => GetCopyStandartsCommand(sourceDocument, item.Trim()));
    }

    private ICopyStandartsCommand GetCopyStandartsCommand(Document sourceDocument, string className) {
        if(_commandsMap.TryGetValue(className, out string commandName)) {
            var type = Type.GetType(commandName);
            if(type == null) {
                return new CopyOptionalStandartsCommand(sourceDocument, _revitRepository.Document, _localizationService) {
                    Name = className,
                    BuiltInCategoryName = commandName
                };
            }

            return (ICopyStandartsCommand) Activator.CreateInstance(type, sourceDocument, _revitRepository.Document, _localizationService);
        }

        throw new ArgumentException(_localizationService.GetLocalizedString("Exceptions.UnknownClassName", className));
    }

    private IEnumerable<ICopyStandartsCommand> GetIdOptionalStandarts(Document sourceDocument) {
        if(string.IsNullOrEmpty(sourceDocument.ProjectInformation.ClientName)) {
            return [];
        }

        var elements = sourceDocument.ProjectInformation.ClientName
            .Split([';'], StringSplitOptions.RemoveEmptyEntries)
            .Where(item => int.TryParse(item, out _))
            .Select(item => GetElement(sourceDocument, item))
            .Where(item => item != null);

        return [
            new CopyElementIdsCommand(sourceDocument, _revitRepository.Document, _localizationService) { CopyElements = elements.ToList() }
        ];
    }

    private Element GetElement(Document sourceDocument, string elementId) {
        if(string.IsNullOrEmpty(elementId)) {
            throw new ArgumentException(_localizationService.GetLocalizedString("Exceptions.NullElementId"), nameof(elementId));
        }
#if REVIT_2023_OR_LESS
            if(!int.TryParse(elementId, out int result)) {
                throw new ArgumentException("Идентификатор элемента должен быть числом.", nameof(elementId));
            }

            return sourceDocument.GetElement(new ElementId(result));
#else
        if(!long.TryParse(elementId, out long result)) {
            throw new ArgumentException(_localizationService.GetLocalizedString("Exceptions.NumElementId"), nameof(elementId));
        }

        return sourceDocument.GetElement(new ElementId(result));
#endif
    }
}

public class CustomCopyHandler : IDuplicateTypeNamesHandler {
    public DuplicateTypeAction OnDuplicateTypeNamesFound(
        DuplicateTypeNamesHandlerArgs args) {
        return DuplicateTypeAction.UseDestinationTypes;
    }
}

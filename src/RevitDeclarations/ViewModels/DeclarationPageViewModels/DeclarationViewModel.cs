using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;

using System.Windows.Input;

using RevitDeclarations.Models;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using dosymep.SimpleServices;

namespace RevitDeclarations.ViewModels;

internal class DeclarationViewModel : BaseViewModel {
    protected readonly RevitRepository _revitRepository;
    protected readonly DeclarationSettings _settings;
    protected readonly IMessageBoxService _messageBoxService;

    protected readonly IList<RevitDocumentViewModel> _revitDocuments;
    protected readonly IReadOnlyList<Phase> _phases;
    protected Phase _selectedPhase;

    protected string _filePath;
    protected string _fileName;

    protected List<ExportViewModel> _exportFormats;
    protected ExportViewModel _selectedFormat;

    protected string _accuracy;

    protected bool _loadUtp;
    protected bool _canLoadUtp;
    protected string _canLoadUtpText;    

    public DeclarationViewModel(RevitRepository revitRepository, 
                                DeclarationSettings settings,
                                ILocalizationService localizationService,
                                IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository;
        _settings = settings;
        _messageBoxService = messageBoxService;

        _phases = _revitRepository.GetPhases();
        _selectedPhase = _phases[_phases.Count() - 1];

        _revitDocuments = _revitRepository
            .GetLinks()
            .Select(x => new RevitDocumentViewModel(x, _settings))
            .Where(x => x.HasRooms())
            .OrderBy(x => x.Name)
            .ToList();

        var currentDocumentVM =
            new RevitDocumentViewModel(_revitRepository.Document, _settings, localizationService);

        if(currentDocumentVM.HasRooms()) {
            _revitDocuments.Insert(0, currentDocumentVM);
        }

        _accuracy = "1";

        SelectFolderCommand = new RelayCommand(SelectFolder);
    }

    public ICommand SelectFolderCommand { get; }

    public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;
    public IReadOnlyList<Phase> Phases => _phases;

    public Phase SelectedPhase {
        get => _selectedPhase;
        set => RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    public string FilePath {
        get => _filePath;
        set => RaiseAndSetIfChanged(ref _filePath, value);
    }
    public string FileName {
        get => _fileName;
        set => RaiseAndSetIfChanged(ref _fileName, value);
    }
    public string FullPath => FilePath + "\\" + FileName;

    public IReadOnlyList<ExportViewModel> ExportFormats => _exportFormats;
    public ExportViewModel SelectedFormat {
        get => _selectedFormat;
        set => RaiseAndSetIfChanged(ref _selectedFormat, value);
    }

    public string Accuracy {
        get => _accuracy;
        set => RaiseAndSetIfChanged(ref _accuracy, value);
    }

    public bool LoadUtp {
        get => _loadUtp;
        set => RaiseAndSetIfChanged(ref _loadUtp, value);
    }

    public bool CanLoadUtp {
        get => _canLoadUtp;
        set => RaiseAndSetIfChanged(ref _canLoadUtp, value);
    }
    public string CanLoadUtpText {
        get => _canLoadUtpText;
        set => RaiseAndSetIfChanged(ref _canLoadUtpText, value);
    }


    public void SelectFolder(object obj) {
        var dialog = new CommonOpenFileDialog() {
            IsFolderPicker = true
        };

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            FilePath = dialog.FileName;
        }
    }
}

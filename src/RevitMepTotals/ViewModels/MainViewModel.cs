using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;
using RevitMepTotals.Services;

namespace RevitMepTotals.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly IDocumentsProcessor _documentsProcessor;
    private readonly IDataExporter _dataExporter;
    private readonly ILocalizationService _localizationService;

    public MainViewModel(
        IMessageBoxService messageBoxService,
        IDocumentsProcessor documentsProcessor,
        IDataExporter dataExporter,
        IProgressDialogFactory progressDialogFactory,
        IOpenFileDialogService openFileDialogService,
        IOpenFolderDialogService openFolderDialogService,
        ILocalizationService localizationService
        ) {
        MessageBoxService = messageBoxService
            ?? throw new ArgumentNullException(nameof(messageBoxService));
        _documentsProcessor = documentsProcessor
            ?? throw new ArgumentNullException(nameof(documentsProcessor));
        _dataExporter = dataExporter
            ?? throw new ArgumentNullException(nameof(dataExporter));
        ProgressDialogFactory = progressDialogFactory
            ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        OpenFileDialogService = openFileDialogService
            ?? throw new ArgumentNullException(nameof(openFileDialogService));
        OpenFolderDialogService = openFolderDialogService
            ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        AddDocumentCommand = RelayCommand.Create(AddDocument);
        RemoveDocumentCommand = RelayCommand.Create<DocumentViewModel>(RemoveDocument, CanRemoveDocument);
        ProcessDocumentsCommand = RelayCommand.Create(ProcessDocuments, CanProcessDocuments);
    }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public IOpenFolderDialogService OpenFolderDialogService { get; }

    public IMessageBoxService MessageBoxService { get; }

    public ICommand AddDocumentCommand { get; }

    public ICommand RemoveDocumentCommand { get; }

    public ICommand ProcessDocumentsCommand { get; }

    public ObservableCollection<DocumentViewModel> Documents { get; } = [];


    private DocumentViewModel _selectedDocument;
    public DocumentViewModel SelectedDocument {
        get => _selectedDocument;
        set => RaiseAndSetIfChanged(ref _selectedDocument, value);
    }


    private string _errorText;
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    private void AddDocument() {
        if(!OpenFileDialogService.ShowDialog()) {
            return;
        }
        var docViewModels = OpenFileDialogService.Files
             .Select(file => new RevitDocument(file))
             .Select(doc => new DocumentViewModel(doc));
        string error = string.Empty;
        foreach(var docViewModel in docViewModels) {
            if(!Documents.Contains(docViewModel)) {
                Documents.Add(docViewModel);
            } else {
                error = _localizationService.GetLocalizedString("Errors.DocumentAlreadyAdded", docViewModel.Name);
            }
        }
        if(!string.IsNullOrWhiteSpace(error)) {
            ShowMessageBoxError(error);
        }
    }


    private void RemoveDocument(DocumentViewModel document) {
        if(MessageBoxService.Show(
            _localizationService.GetLocalizedString("Warnings.DocumentWillBeRemoved", document.Name),
            "BIM",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning,
            MessageBoxResult.Cancel) == MessageBoxResult.OK) {

            Documents.Remove(document);
            SelectedDocument = Documents.FirstOrDefault();
        }
    }

    private bool CanRemoveDocument(DocumentViewModel document) {
        return document is not null;
    }

    private void ProcessDocuments() {
        var documents = Documents.Select(vm => vm.GetDocument()).ToHashSet();
        if(documents.Count > 0) {
            var directory = OpenFolderDialogService.ShowDialog()
                ? OpenFolderDialogService.Folder
                : throw new OperationCanceledException();
            string errorMsg;
            IList<IDocumentData> processedData;
            using(var progressDialogService = ProgressDialogFactory.CreateDialog()) {
                progressDialogService.StepValue = 1;
                progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("ProgressTitle");
                var progress = progressDialogService.CreateProgress();
                progressDialogService.MaxValue = documents.Count;
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                processedData = _documentsProcessor.ProcessDocuments(documents, out string processError, progress, ct);
                errorMsg = processError;
            }
            ShowMessageBoxError(errorMsg);

            if(processedData.Count > 0) {
                _dataExporter.ExportData(directory, processedData, out string exportError);
                errorMsg = exportError;
                ShowMessageBoxError(errorMsg);
            }
        }
    }

    private bool CanProcessDocuments() {
        return Documents.Count > 0;
    }

    private void ShowMessageBoxError(string error) {
        if(!string.IsNullOrWhiteSpace(error)) {
            MessageBoxService.Show(error, "BIM",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
        }
    }
}

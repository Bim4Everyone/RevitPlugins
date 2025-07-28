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
    private readonly IDirectoryProvider _directoryProvider;

    public MainViewModel(
        IMessageBoxService messageBoxService,
        IDocumentsProcessor documentsProcessor,
        IDataExporter dataExporter,
        IDirectoryProvider directoryProvider,
        IProgressDialogFactory progressDialogFactory,
        IOpenFileDialogService openFileDialogService
        ) {
        MessageBoxService = messageBoxService
            ?? throw new System.ArgumentNullException(nameof(messageBoxService));
        _documentsProcessor = documentsProcessor
            ?? throw new System.ArgumentNullException(nameof(documentsProcessor));
        _dataExporter = dataExporter
            ?? throw new System.ArgumentNullException(nameof(dataExporter));
        _directoryProvider = directoryProvider
            ?? throw new System.ArgumentNullException(nameof(directoryProvider));
        ProgressDialogFactory = progressDialogFactory
            ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
        OpenFileDialogService = openFileDialogService
            ?? throw new System.ArgumentNullException(nameof(openFileDialogService));

        AddDocumentCommand = RelayCommand.Create(AddDocument);
        RemoveDocumentCommand = RelayCommand.Create(RemoveDocument, CanRemoveDocument);
        ProcessDocumentsCommand = RelayCommand.Create(ProcessDocuments, CanProcessDocuments);
    }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public IMessageBoxService MessageBoxService { get; }

    public ICommand AddDocumentCommand { get; }

    public ICommand RemoveDocumentCommand { get; }

    public ICommand ProcessDocumentsCommand { get; }

    public ObservableCollection<DocumentViewModel> Documents { get; }
        = [];


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
        if(!OpenFileDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) {
            return;
        }
        var docViewModels = OpenFileDialogService.Files
             .Select(file => new RevitDocument(file))
             .Select(doc => new DocumentViewModel(doc));
        List<string> errors = [];
        foreach(var docViewModel in docViewModels) {
            if(!Documents.Contains(docViewModel)) {
                Documents.Add(docViewModel);
            } else {
                errors.Add($"{docViewModel} уже добавлен в список");
            }
        }
        if(errors.Count > 0) {
            ShowMessageBoxError(string.Join("\n", errors));
        }
    }


    private void RemoveDocument() {
        if(MessageBoxService.Show(
            $"Из списка будет удален документ:\n{SelectedDocument.Name}\nПродолжить?",
            "BIM",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning,
            MessageBoxResult.Cancel) == MessageBoxResult.OK) {

            Documents.Remove(SelectedDocument);
            SelectedDocument = Documents.FirstOrDefault();
        }
    }

    private bool CanRemoveDocument() {
        return SelectedDocument != null;
    }

    private void ProcessDocuments() {
        var documents = Documents.Select(vm => vm.GetDocument()).ToHashSet();
        if(documents.Count > 0) {
            var directory = _directoryProvider.GetDirectory();
            string errorMsg;
            IList<IDocumentData> processedData;
            using(var progressDialogService = ProgressDialogFactory.CreateDialog()) {
                progressDialogService.StepValue = 1;
                progressDialogService.DisplayTitleFormat = "Обработка документов... [{0}]\\[{1}]";
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

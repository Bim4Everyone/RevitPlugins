using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchSpecExport.Models;
using RevitBatchSpecExport.Services;

namespace RevitBatchSpecExport.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _config;
    private readonly IDocumentsProcessor _documentsProcessor;
    private readonly ILocalizationService _localizationService;
    private DocumentViewModel _selectedDocument;

    private string _paramName;
    private string _errorText;

    public MainViewModel(
        PluginConfig config,
        IMessageBoxService messageBoxService,
        IDocumentsProcessor documentsProcessor,
        IProgressDialogFactory progressDialogFactory,
        IOpenFileDialogService openFileDialogService,
        IOpenFolderDialogService openFolderDialogService,
        ILocalizationService localizationService
    ) {
        MessageBoxService = messageBoxService
                            ?? throw new ArgumentNullException(nameof(messageBoxService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _documentsProcessor = documentsProcessor
                              ?? throw new ArgumentNullException(nameof(documentsProcessor));
        ProgressDialogFactory = progressDialogFactory
                                ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        OpenFileDialogService = openFileDialogService
                                ?? throw new ArgumentNullException(nameof(openFileDialogService));
        OpenFolderDialogService = openFolderDialogService
                                  ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        _localizationService = localizationService
                               ?? throw new ArgumentNullException(nameof(localizationService));
        AddDocumentCommand = RelayCommand.Create(AddDocument);
        LoadViewCommand = RelayCommand.Create(LoadView);
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

    public ICommand LoadViewCommand { get; }

    public ObservableCollection<DocumentViewModel> Documents { get; } = [];

    public DocumentViewModel SelectedDocument {
        get => _selectedDocument;
        set => RaiseAndSetIfChanged(ref _selectedDocument, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
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
               MessageBoxResult.Cancel)
           == MessageBoxResult.OK) {
            Documents.Remove(document);
            SelectedDocument = Documents.FirstOrDefault();
        }
    }

    private bool CanRemoveDocument(DocumentViewModel document) {
        return document is not null;
    }

    private void ProcessDocuments() {
        SaveConfig();
        var documents = Documents.Select(vm => vm.GetDocument()).ToHashSet();
        if(documents.Count > 0) {
            var directory = OpenFolderDialogService.ShowDialog()
                ? OpenFolderDialogService.Folder
                : throw new OperationCanceledException();
            string errorMsg;
            using(var progressDialogService = ProgressDialogFactory.CreateDialog()) {
                progressDialogService.StepValue = 1;
                progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("ProgressTitle");
                var progress = progressDialogService.CreateProgress();
                progressDialogService.MaxValue = documents.Count;
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                errorMsg = _documentsProcessor.ProcessDocuments(directory, documents, _config, progress, ct);
            }

            ShowMessageBoxError(errorMsg);
        }
    }

    private void SaveConfig() {
        _config.SheetParamName = ParamName;
        _config.SaveProjectConfig();
    }

    private void LoadView() {
        ParamName = _config.SheetParamName;
    }

    private bool CanProcessDocuments() {
        return Documents.Count > 0;
    }

    private void ShowMessageBoxError(string error) {
        if(!string.IsNullOrWhiteSpace(error)) {
            MessageBoxService.Show(
                error,
                "BIM",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
        }
    }
}

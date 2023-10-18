using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMepTotals.Services;

namespace RevitMepTotals.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly IDocumentsProvider _documentsProvider;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IDocumentsProcessor _documentsProcessor;

        public MainViewModel(
            IDocumentsProvider documentsProvider,
            IMessageBoxService messageBoxService,
            IDocumentsProcessor documentsProcessor) {

            _documentsProvider = documentsProvider ?? throw new System.ArgumentNullException(nameof(documentsProvider));
            _messageBoxService = messageBoxService ?? throw new System.ArgumentNullException(nameof(messageBoxService));
            _documentsProcessor = documentsProcessor ?? throw new System.ArgumentNullException(nameof(documentsProcessor));

            AddDocumentCommand = new RelayCommand(AddDocument);
            RemoveDocumentCommand = new RelayCommand(RemoveDocument, CanRemoveDocument);
            ProcessDocumentsCommand = new RelayCommand(ProcessDocuments, CanProcessDocuments);
        }


        public ICommand AddDocumentCommand { get; }

        public ICommand RemoveDocumentCommand { get; }

        public ICommand ProcessDocumentsCommand { get; }

        public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>() { };


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


        private void AddDocument(object p) {
            var docViewModels = _documentsProvider.GetDocuments().Select(doc => new DocumentViewModel(doc));
            List<string> errors = new List<string>();
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


        private void RemoveDocument(object p) {
            if(_messageBoxService.Show(
                $"Из списка будет удален документ:\n{SelectedDocument.Name}\nПродолжить?",
                "BIM",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK) {

                Documents.Remove(SelectedDocument);
                SelectedDocument = Documents.FirstOrDefault();
            }
        }

        private bool CanRemoveDocument(object p) => SelectedDocument != null;


        private void ProcessDocuments(object p) {
            _documentsProcessor.ProcessDocuments(Documents.Select(vm => vm.GetDocument()).ToHashSet());
        }

        private bool CanProcessDocuments(object p) => Documents.Count > 0;


        private void ShowMessageBoxError(string error) {
            _messageBoxService.Show(error, "BIM",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
        }
    }
}

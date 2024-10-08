using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal abstract class MainViewModel : BaseViewModel {
        private protected readonly RevitRepository _revitRepository;
        private protected readonly DeclarationSettings _settings;

        private protected readonly IList<RevitDocumentViewModel> _revitDocuments;
        private protected readonly IReadOnlyList<Phase> _phases;

        private protected string _filePath;
        private protected string _fileName;
        private protected Phase _selectedPhase;

        private protected string _accuracy;

        private string _errorText;

        public MainViewModel(RevitRepository revitRepository, DeclarationSettings settings) {
            _revitRepository = revitRepository;
            _settings = settings;

            _phases = _revitRepository.GetPhases();

            _revitDocuments = _revitRepository
                .GetLinks()
                .Select(x => new RevitDocumentViewModel(x, _settings))
                .Where(x => x.HasRooms())
                .OrderBy(x => x.Name)
                .ToList();

            _accuracy = "1";

            SelectFolderCommand = new RelayCommand(SelectFolder);
            ExportDeclarationCommand = new RelayCommand(ExportDeclaration, CanExport);
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand ExportDeclarationCommand { get; }

        public IList<RevitDocumentViewModel> RevitDocuments => _revitDocuments;

        public string FilePath {
            get => _filePath;
            set => RaiseAndSetIfChanged(ref _filePath, value);
        }
        public string FileName {
            get => _fileName;
            set => RaiseAndSetIfChanged(ref _fileName, value);
        }
        public string FullPath => FilePath + "\\" + FileName;

        public string Accuracy {
            get => _accuracy;
            set => RaiseAndSetIfChanged(ref _accuracy, value);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void SelectFolder(object obj) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
                IsFolderPicker = true
            };

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                FilePath = dialog.FileName;
            }
        }

        public virtual void ExportDeclaration(object obj) { }

        public bool CanExport(object obj) {
            IEnumerable<RevitDocumentViewModel> checkedDocuments = _revitDocuments
                .Where(x => x.IsChecked);

            bool hasCheckedDocuments = _revitDocuments
                .Where(x => x.IsChecked)
                .Any();

            bool hasPhases = checkedDocuments
                .All(x => x.HasPhase(_selectedPhase));

            if(string.IsNullOrEmpty(_filePath)) {
                ErrorText = "Не выбрана папка";
                return false;
            }
            if(string.IsNullOrEmpty(_fileName)) {
                ErrorText = "Не заполнено имя файла";
                return false;
            }
            if(!hasCheckedDocuments) {
                ErrorText = "Не выбраны проекты для выгрузки";
                return false;
            }
            if(!hasPhases) {
                ErrorText = "В выбранных проектах отсутствует выбранная стадия";
                return false;
            }

            ErrorText = "";
            return true;
        }
    }
}

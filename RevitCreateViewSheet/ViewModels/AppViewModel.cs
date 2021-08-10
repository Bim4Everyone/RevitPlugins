using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AppViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private int _countCreateView;
        private string _albumBlueprints;

        public AppViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);

            CreateViewSheetCommand = new RelayCommand(CreateViewSheet, CanCreateViewSheet);
            CreateViewSheetsCommand = new RelayCommand(CreateViewSheets, CanCreateViewSheets);

            ViewSheets = new ObservableCollection<ViewSheetViewModel>();
            AlbumsBlueprints = new ObservableCollection<string>(_revitRepository.GetAlbumsBlueprints());
            FamilyInstances = new ObservableCollection<FamilyInstanceViewModel>(_revitRepository.GetTitleBlocks().Select(item => new FamilyInstanceViewModel(item)).OrderBy(item => item.Name));
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public int CountCreateView {
            get => _countCreateView;
            set => this.RaiseAndSetIfChanged(ref _countCreateView, value);
        }

        public string AlbumBlueprints {
            get => _albumBlueprints;
            set => this.RaiseAndSetIfChanged(ref _albumBlueprints, value);
        }

        public ICommand CreateViewSheetCommand { get; }
        public ICommand CreateViewSheetsCommand { get; }

        public ObservableCollection<string> AlbumsBlueprints { get; }
        public ObservableCollection<ViewSheetViewModel> ViewSheets { get; }
        public ObservableCollection<FamilyInstanceViewModel> FamilyInstances { get; }

        public void CreateViewSheet(object p) {
            ViewSheets.Add(new ViewSheetViewModel());
        }

        public bool CanCreateViewSheet(object p) {
            return _countCreateView > 0;
        }

        public void CreateViewSheets(object p) {

        }

        public bool CanCreateViewSheets(object p) {
            return ViewSheets.Count > 0 && ViewSheets.All(item => !string.IsNullOrEmpty(item.Name));
        }
    }
}

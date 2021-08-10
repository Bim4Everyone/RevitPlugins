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
        private readonly FamilyInstanceViewModel _defaultFamilyInstance;

        private string _errorText;
        private int _countCreateView;
        private string _albumBlueprints;
        private ViewSheetViewModel _viewSheet;

        public AppViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);

            RemoveViewSheetCommand = new RelayCommand(RemoveViewSheet, CanRemoveViewSheet);
            CreateViewSheetCommand = new RelayCommand(CreateViewSheet, CanCreateViewSheet);
            CreateViewSheetsCommand = new RelayCommand(CreateViewSheets, CanCreateViewSheets);

            ViewSheets = new ObservableCollection<ViewSheetViewModel>();
            AlbumsBlueprints = new ObservableCollection<string>(_revitRepository.GetAlbumsBlueprints());
            FamilyInstances = new ObservableCollection<FamilyInstanceViewModel>(_revitRepository.GetTitleBlocks().Select(item => new FamilyInstanceViewModel(item)).OrderBy(item => item.Name));

            CountCreateView = 1;
            AlbumBlueprints = AlbumsBlueprints.FirstOrDefault();

            _defaultFamilyInstance = FamilyInstances.FirstOrDefault(item => item.FamilyInstanceName.Equals("Создать типы по комплектам"));
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

        public ViewSheetViewModel ViewSheet {
            get => _viewSheet;
            set => this.RaiseAndSetIfChanged(ref _viewSheet, value);
        }

        public ICommand RemoveViewSheetCommand { get; }
        public ICommand CreateViewSheetCommand { get; }
        public ICommand CreateViewSheetsCommand { get; }

        public ObservableCollection<string> AlbumsBlueprints { get; }
        public ObservableCollection<ViewSheetViewModel> ViewSheets { get; }
        public ObservableCollection<FamilyInstanceViewModel> FamilyInstances { get; }

        public void RemoveViewSheet(object p) {
            ViewSheets.Remove((ViewSheetViewModel) p);
        }

        public bool CanRemoveViewSheet(object p) {
            return true;
        }

        public void CreateViewSheet(object p) {
            foreach(int index in Enumerable.Range(0, CountCreateView)) {
                ViewSheets.Add(new ViewSheetViewModel() { FamilyInstance = _defaultFamilyInstance });
            }
        }

        public bool CanCreateViewSheet(object p) {
            return _countCreateView > 0;
        }

        public void CreateViewSheets(object p) {

        }

        public bool CanCreateViewSheets(object p) {
            return ViewSheets.Count > 0 && ViewSheets.All(item => !string.IsNullOrEmpty(item.Name)) && ViewSheets.All(item => item.FamilyInstance != null);
        }
    }
}

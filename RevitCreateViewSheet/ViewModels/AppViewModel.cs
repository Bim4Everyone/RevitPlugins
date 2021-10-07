using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AppViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly TitleBlockViewModel _defaultTitleBlock;

        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private ViewSheetViewModel _viewSheet;

        public AppViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);

            RemoveViewSheetCommand = new RelayCommand(RemoveViewSheet, CanRemoveViewSheet);
            CreateViewSheetCommand = new RelayCommand(CreateViewSheet, CanCreateViewSheet);
            CreateViewSheetsCommand = new RelayCommand(CreateViewSheets, CanCreateViewSheets);

            ViewSheets = new ObservableCollection<ViewSheetViewModel>();
            AlbumsBlueprints = new ObservableCollection<string>(_revitRepository.GetAlbumsBlueprints());
            TitleBlocks = new ObservableCollection<TitleBlockViewModel>(_revitRepository.GetTitleBlocks().Select(item => new TitleBlockViewModel(item)).OrderBy(item => item.Name));

            CountCreateView = "1";
            AlbumBlueprints = _revitRepository.GetDefaultAlbum() ?? AlbumsBlueprints.FirstOrDefault();

            _defaultTitleBlock = TitleBlocks.FirstOrDefault(item => item.FamilyName.Equals("Создать типы по комплектам"));
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string CreateErrorText {
            get => _createErrorText;
            set => this.RaiseAndSetIfChanged(ref _createErrorText, value);
        }

        public string CountCreateView {
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
        public ObservableCollection<TitleBlockViewModel> TitleBlocks { get; }

        public void RemoveViewSheet(object p) {
            ViewSheets.Remove((ViewSheetViewModel) p);
        }

        public bool CanRemoveViewSheet(object p) {
            return true;
        }

        public void CreateViewSheet(object p) {
            foreach(int index in Enumerable.Range(0, int.Parse(CountCreateView))) {
                ViewSheets.Add(new ViewSheetViewModel() { TitleBlock = _defaultTitleBlock });
            }
        }

        public bool CanCreateViewSheet(object p) {
            if(!int.TryParse(_countCreateView, out _)) {
                CreateErrorText = "Количество листов должно быть числовым значением.";
                return false;
            }

            if(int.Parse(_countCreateView) <= 0) {
                CreateErrorText = "Количество листов должно быть не отрицательным.";
                return false;
            }

            CreateErrorText = null;
            return true;
        }

        public void CreateViewSheets(object p) {           
            int lastIndex = _revitRepository.GetLastViewSheetIndex(AlbumBlueprints);
            lastIndex++;
            using(var transaction = new Transaction(_revitRepository.Document)) {
                transaction.Start("Создание видов");

                foreach(var viewSheetViewModel in ViewSheets) {
                    ViewSheet viewSheet = _revitRepository.CreateViewSheet(viewSheetViewModel.FamilySymbol);

                    viewSheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, lastIndex.ToString());
                    viewSheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, AlbumBlueprints);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NAME, viewSheetViewModel.Name);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, $"{AlbumBlueprints}-{lastIndex++}");
                }

                transaction.Commit();
            }
        }

        public bool CanCreateViewSheets(object p) {
            if(string.IsNullOrEmpty(AlbumBlueprints)) {
                ErrorText = "Выберите альбом.";
                return false;
            }

            if(ViewSheets.Count == 0) {
                ErrorText = "Добавьте создаваемые листы.";
                return false;
            }

            if(!ViewSheets.All(item => !string.IsNullOrEmpty(item.Name))) {
                ErrorText = "У всех листов должно быть заполнено наименование.";
                return false;
            }

            if(!ViewSheets.All(item => item.TitleBlock != null)) {
                ErrorText = "У всех листов должна быть выбрана основная надпись.";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}

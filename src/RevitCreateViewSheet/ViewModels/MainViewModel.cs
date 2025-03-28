using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly SheetsSaver _sheetsSaver;
        private readonly ILocalizationService _localizationService;
        private readonly TitleBlockViewModel _defaultTitleBlock;

        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private SheetViewModel _viewSheet;

        public MainViewModel(
            RevitRepository revitRepository,
            SheetsSaver sheetsSaver,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _sheetsSaver = sheetsSaver ?? throw new System.ArgumentNullException(nameof(sheetsSaver));
            _localizationService = localizationService ?? throw new System.ArgumentNullException(nameof(localizationService));

            RemoveViewSheetCommand = new RelayCommand(RemoveViewSheet, CanRemoveViewSheet);
            CreateViewSheetCommand = new RelayCommand(CreateViewSheet, CanCreateViewSheet);
            CreateViewSheetsCommand = new RelayCommand(CreateViewSheets, CanCreateViewSheets);

            ViewSheets = new ObservableCollection<SheetViewModel>();
            AlbumsBlueprints = new ObservableCollection<string>(_revitRepository.GetAlbumsBlueprints());
            TitleBlocks = new ObservableCollection<TitleBlockViewModel>(_revitRepository.GetTitleBlocks().Select(item => new TitleBlockViewModel(item)).OrderBy(item => item.Name));

            CountCreateView = "1";
            AlbumBlueprints = _revitRepository.GetDefaultAlbum() ?? AlbumsBlueprints.FirstOrDefault();

            _defaultTitleBlock = TitleBlocks.FirstOrDefault();
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

        public SheetViewModel ViewSheet {
            get => _viewSheet;
            set => this.RaiseAndSetIfChanged(ref _viewSheet, value);
        }

        public ICommand RemoveViewSheetCommand { get; }
        public ICommand CreateViewSheetCommand { get; }
        public ICommand CreateViewSheetsCommand { get; }

        public ObservableCollection<string> AlbumsBlueprints { get; }
        public ObservableCollection<SheetViewModel> ViewSheets { get; }
        public ObservableCollection<TitleBlockViewModel> TitleBlocks { get; }

        public void RemoveViewSheet(object p) {
            ViewSheets.Remove((SheetViewModel) p);
        }

        public bool CanRemoveViewSheet(object p) {
            return true;
        }

        public void CreateViewSheet(object p) {
            foreach(int index in Enumerable.Range(0, int.Parse(CountCreateView))) {
                throw new System.NotImplementedException();
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
                    ViewSheet viewSheet = default;

                    viewSheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, lastIndex.ToString());
                    viewSheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, AlbumBlueprints);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NAME, viewSheetViewModel.Name);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, $"{AlbumBlueprints}-{lastIndex++}");
                    throw new System.NotImplementedException();
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

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private const int _maxSheetsCountToAdd = 1000;

        private readonly RevitRepository _revitRepository;
        private readonly SheetsSaver _sheetsSaver;
        private readonly ILocalizationService _localizationService;
        private readonly IProgressDialogFactory _progressDialogFactory;
        private readonly ObservableCollection<SheetViewModel> _allSheets;

        private TitleBlockViewModel _addSheetsTitleBlock;
        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private SheetViewModel _selectedSheet;

        public MainViewModel(
            RevitRepository revitRepository,
            SheetsSaver sheetsSaver,
            ILocalizationService localizationService,
            IProgressDialogFactory progressDialogFactory) {

            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _sheetsSaver = sheetsSaver
                ?? throw new System.ArgumentNullException(nameof(sheetsSaver));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));
            _progressDialogFactory = progressDialogFactory
                ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            RemoveViewSheetCommand = RelayCommand.Create<SheetViewModel>(RemoveViewSheet, CanRemoveViewSheet);
            AddViewSheetsCommand = RelayCommand.Create(AddViewSheets, CanAddViewSheets);

            var comparer = new LogicalStringComparer();
            Sheets = [];
            _allSheets = [];
            AllAlbumsBlueprints = [.. _revitRepository.GetAlbumsBlueprints()
                .OrderBy(item => item, comparer)];
            AllTitleBlocks = [.. _revitRepository.GetTitleBlocks()
                .Select(item => new TitleBlockViewModel(item))
                .OrderBy(item => item.Name, comparer)];
            AllViewPortTypes = [.. _revitRepository.GetViewPortTypes().Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(item => item.Name, comparer)];

            AddSheetsCount = "1";
            AddSheetsAlbumBlueprint = AllAlbumsBlueprints.FirstOrDefault();
            AddSheetsTitleBlock = AllTitleBlocks.FirstOrDefault();
        }


        public IProgressDialogFactory ProgressDialogFactory => _progressDialogFactory;

        public ICommand RemoveViewSheetCommand { get; }

        public ICommand AddViewSheetsCommand { get; }

        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string AddSheetsErrorText {
            get => _createErrorText;
            set => RaiseAndSetIfChanged(ref _createErrorText, value);
        }

        public string AddSheetsCount {
            get => _countCreateView;
            set => RaiseAndSetIfChanged(ref _countCreateView, value);
        }

        public string AddSheetsAlbumBlueprint {
            get => _albumBlueprints;
            set => RaiseAndSetIfChanged(ref _albumBlueprints, value);
        }

        public TitleBlockViewModel AddSheetsTitleBlock {
            get => _addSheetsTitleBlock;
            set => RaiseAndSetIfChanged(ref _addSheetsTitleBlock, value);
        }

        public SheetViewModel SelectedSheet {
            get => _selectedSheet;
            set => RaiseAndSetIfChanged(ref _selectedSheet, value);
        }

        public ObservableCollection<SheetViewModel> Sheets { get; }

        public ObservableCollection<string> AllAlbumsBlueprints { get; }

        public ObservableCollection<TitleBlockViewModel> AllTitleBlocks { get; }

        public ObservableCollection<ViewPortTypeViewModel> AllViewPortTypes { get; }


        private void LoadView() {
            var sheets = _revitRepository.GetSheetModels()
                .Select(s => new SheetViewModel(s))
                .OrderBy(s => s.AlbumBlueprint + s.SheetNumber, new LogicalStringComparer())
                .ToArray();
            for(int i = 0; i < sheets.Length; i++) {
                Sheets.Add(sheets[i]);
                _allSheets.Add(sheets[i]);
            }
        }

        private void AddViewSheets() {
            foreach(int index in Enumerable.Range(0, int.Parse(AddSheetsCount))) {
                throw new System.NotImplementedException();
            }
        }

        private bool CanAddViewSheets() {
            if(string.IsNullOrWhiteSpace(AddSheetsAlbumBlueprint)) {
                AddSheetsErrorText = "Выберите альбом.";
                return false;
            }

            if(AddSheetsTitleBlock is null) {
                AddSheetsErrorText = "Выберите штамп.";
                return false;
            }

            if(!int.TryParse(AddSheetsCount, out _)) {
                AddSheetsErrorText = "Количество листов должно быть числовым значением.";
                return false;
            }

            if(int.TryParse(AddSheetsCount, out int number) && number > _maxSheetsCountToAdd) {
                AddSheetsErrorText = "Количество листов должно быть числовым значением.";
                return false;
            }

            if(int.Parse(AddSheetsCount) <= 0) {
                AddSheetsErrorText = "Количество листов должно быть не отрицательным.";
                return false;
            }

            AddSheetsErrorText = null;
            return true;
        }

        private void RemoveViewSheet(SheetViewModel sheet) {
            if(sheet.IsPlaced) {
                sheet.SheetModel.MarkAsDeleted();
            } else {
                _allSheets.Remove(sheet);
            }
            Sheets.Remove(sheet);
        }

        private bool CanRemoveViewSheet(SheetViewModel sheet) {
            return sheet is not null;
        }

        private void AcceptView() {
            var sheets = _allSheets.Select(s => s.SheetModel).Where(s => s.State != EntityState.Unchanged).ToArray();
            if(sheets.Any()) {
                using(var progressDialogService = _progressDialogFactory.CreateDialog()) {
                    progressDialogService.StepValue = 1;
                    progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("TODO");
                    progressDialogService.MaxValue = sheets.Length;
                    var progress = progressDialogService.CreateProgress();
                    var ct = progressDialogService.CreateCancellationToken();
                    progressDialogService.Show();

                    _sheetsSaver.SaveSheets(sheets, progress, ct);
                }
            }
        }

        private bool CanAcceptView() {
            if(Sheets.Any(item => string.IsNullOrWhiteSpace(item.AlbumBlueprint))) {
                ErrorText = "У всех листов должно быть заполнен альбом.";
                return false;
            }

            if(Sheets.Any(item => string.IsNullOrWhiteSpace(item.SheetNumber))) {
                ErrorText = "У всех листов должно быть задан номер.";
                return false;
            }

            if(Sheets.Any(item => string.IsNullOrWhiteSpace(item.Name))) {
                ErrorText = "У всех листов должно быть заполнено наименование.";
                return false;
            }

            if(Sheets.Any(item => item.TitleBlock is null)) {
                ErrorText = "У всех листов должна быть выбрана основная надпись.";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}

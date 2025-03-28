using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.Revit.Comparators;
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
        private readonly IProgressDialogFactory _progressDialogFactory;

        private TitleBlockViewModel _addSheetsTitleBlock;
        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private SheetViewModel _viewSheet;

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
            RemoveViewSheetCommand = new RelayCommand(RemoveViewSheet, CanRemoveViewSheet);
            CreateViewSheetCommand = new RelayCommand(CreateViewSheet, CanCreateViewSheet);
            CreateViewSheetsCommand = new RelayCommand(CreateViewSheets, CanCreateViewSheets);

            AllSheets = new ObservableCollection<SheetViewModel>();
            AllAlbumsBlueprints = [.. _revitRepository.GetAlbumsBlueprints()];
            AllTitleBlocks = [.. _revitRepository.GetTitleBlocks()
                .Select(item => new TitleBlockViewModel(item))
                .OrderBy(item => item.Name, new LogicalStringComparer())];
            AllViewPortTypes = new ObservableCollection<ViewPortTypeViewModel>(//TODO нужна функция по получению типоразмеров видовых экранов);

            AddSheetsCount = "1";
            AddSheetsAlbumBlueprint = AllAlbumsBlueprints.FirstOrDefault();
            AddSheetsTitleBlock = AllTitleBlocks.FirstOrDefault();
        }


        public IProgressDialogFactory ProgressDialogFactory => _progressDialogFactory;

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string CreateErrorText {
            get => _createErrorText;
            set => this.RaiseAndSetIfChanged(ref _createErrorText, value);
        }

        public string AddSheetsCount {
            get => _countCreateView;
            set => this.RaiseAndSetIfChanged(ref _countCreateView, value);
        }

        public string AddSheetsAlbumBlueprint {
            get => _albumBlueprints;
            set => this.RaiseAndSetIfChanged(ref _albumBlueprints, value);
        }

        public TitleBlockViewModel AddSheetsTitleBlock {
            get => _addSheetsTitleBlock;
            set => RaiseAndSetIfChanged(ref _addSheetsTitleBlock, value);
        }

        public SheetViewModel ViewSheet {
            get => _viewSheet;
            set => this.RaiseAndSetIfChanged(ref _viewSheet, value);
        }

        public ICommand RemoveViewSheetCommand { get; }

        public ICommand CreateViewSheetCommand { get; }

        public ICommand CreateViewSheetsCommand { get; }

        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ObservableCollection<SheetViewModel> AllSheets { get; }

        public ObservableCollection<string> AllAlbumsBlueprints { get; }

        public ObservableCollection<TitleBlockViewModel> AllTitleBlocks { get; }

        public ObservableCollection<ViewPortTypeViewModel> AllViewPortTypes { get; }


        private void RemoveViewSheet(object p) {
            AllSheets.Remove((SheetViewModel) p);
        }

        private bool CanRemoveViewSheet(object p) {
            return true;
        }

        private void CreateViewSheet(object p) {
            foreach(int index in Enumerable.Range(0, int.Parse(AddSheetsCount))) {
                throw new System.NotImplementedException();
            }
        }

        private bool CanCreateViewSheet(object p) {
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

        private void CreateViewSheets(object p) {
            int lastIndex = _revitRepository.GetLastViewSheetIndex(AddSheetsAlbumBlueprint);
            lastIndex++;
            using(var transaction = new Transaction(_revitRepository.Document)) {
                transaction.Start("Создание видов");

                foreach(var viewSheetViewModel in AllSheets) {
                    ViewSheet viewSheet = default;

                    viewSheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, lastIndex.ToString());
                    viewSheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, AddSheetsAlbumBlueprint);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NAME, viewSheetViewModel.Name);
                    viewSheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, $"{AddSheetsAlbumBlueprint}-{lastIndex++}");
                    throw new System.NotImplementedException();
                }

                transaction.Commit();
            }
        }

        private bool CanCreateViewSheets(object p) {
            if(string.IsNullOrEmpty(AddSheetsAlbumBlueprint)) {
                ErrorText = "Выберите альбом.";
                return false;
            }

            if(AllSheets.Count == 0) {
                ErrorText = "Добавьте создаваемые листы.";
                return false;
            }

            if(!AllSheets.All(item => !string.IsNullOrEmpty(item.Name))) {
                ErrorText = "У всех листов должно быть заполнено наименование.";
                return false;
            }

            if(!AllSheets.All(item => item.TitleBlock != null)) {
                ErrorText = "У всех листов должна быть выбрана основная надпись.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadView() {
            var sheets = _revitRepository.GetSheetModels()
                .Select(s => new SheetViewModel(s))
                .OrderBy(s => s.AlbumBlueprint + s.SheetNumber, new LogicalStringComparer())
                .ToArray();
            for(int i = 0; i < sheets.Length; i++) {
                AllSheets.Add(sheets[i]);
            }
        }

        private void AcceptView() {
            var sheets = AllSheets.Select(s => s.SheetModel).Where(s => s.State != EntityState.Unchanged).ToArray();
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
            throw new NotImplementedException();
        }
    }
}

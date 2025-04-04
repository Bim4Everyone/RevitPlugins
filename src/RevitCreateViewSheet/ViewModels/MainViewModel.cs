using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

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
        private readonly ISheetItemsFactory _sheetItemsFactory;
        private TitleBlockViewModel _addSheetsTitleBlock;
        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private string _sheetsFilter;
        private SheetViewModel _selectedSheet;
        private CollectionViewSource _visibleSheets;

        public MainViewModel(
            RevitRepository revitRepository,
            SheetsSaver sheetsSaver,
            ILocalizationService localizationService,
            IProgressDialogFactory progressDialogFactory,
            ISheetItemsFactory sheetItemsFactory) {

            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _sheetsSaver = sheetsSaver
                ?? throw new System.ArgumentNullException(nameof(sheetsSaver));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));
            _progressDialogFactory = progressDialogFactory
                ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
            _sheetItemsFactory = sheetItemsFactory
                ?? throw new System.ArgumentNullException(nameof(sheetItemsFactory));
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            RemoveSheetCommand = RelayCommand.Create<SheetViewModel>(RemoveSheet, CanRemoveSheet);
            AddSheetsCommand = RelayCommand.Create(AddSheets, CanAddSheets);
            AddViewPortCommand = RelayCommand.Create(AddViewPort, CanAddSheetItem);
            AddScheduleCommand = RelayCommand.Create(AddSchedule, CanAddSheetItem);
            AddAnnotationCommand = RelayCommand.Create(AddAnnotation, CanAddSheetItem);

            var comparer = new LogicalStringComparer();
            AllSheets = [];
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

        public ICommand RemoveSheetCommand { get; }

        public ICommand AddSheetsCommand { get; }

        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand AddViewPortCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public string SheetsFilter {
            get => _sheetsFilter;
            set {
                RaiseAndSetIfChanged(ref _sheetsFilter, value);
                VisibleSheets.View.Refresh();
            }
        }

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

        public CollectionViewSource VisibleSheets {
            get => _visibleSheets;
            set => RaiseAndSetIfChanged(ref _visibleSheets, value);
        }

        public ObservableCollection<SheetViewModel> AllSheets { get; }

        public ObservableCollection<string> AllAlbumsBlueprints { get; }

        public ObservableCollection<TitleBlockViewModel> AllTitleBlocks { get; }

        public ObservableCollection<ViewPortTypeViewModel> AllViewPortTypes { get; }


        private void LoadView() {
            var sheets = _revitRepository.GetSheetModels()
                .Select(s => new SheetViewModel(s))
                .OrderBy(s => s.AlbumBlueprint + s.SheetNumber, new LogicalStringComparer())
                .ToArray();
            for(int i = 0; i < sheets.Length; i++) {
                AllSheets.Add(sheets[i]);
            }
            VisibleSheets = new CollectionViewSource() { Source = AllSheets };
            VisibleSheets.Filter += SheetsFilterHandler;
        }

        private void AddSheets() {
            var indexes = AllSheets.Where(s => s.SheetModel.State != EntityState.Deleted)
                .Select(s => new { IsNumber = int.TryParse(s.SheetNumber, out int number), Number = number })
                .Where(c => c.IsNumber)
                .ToArray();
            int lastIndex = indexes.Length > 0 ? indexes.Max(c => c.Number) : 0;
            var titleBlock = AddSheetsTitleBlock;
            var albumBlueprint = AddSheetsAlbumBlueprint;
            foreach(int index in Enumerable.Range(0, int.Parse(AddSheetsCount))) {
                ++lastIndex;
                var sheetModel = new SheetModel(titleBlock.TitleBlockSymbol) {
                    AlbumBlueprint = albumBlueprint,
                    SheetNumber = lastIndex.ToString(),
                    Name = $"{_localizationService.GetLocalizedString("TODO")} {lastIndex}"
                };
                var sheetViewModel = new SheetViewModel(sheetModel);
                AllSheets.Add(sheetViewModel);
            }
        }

        private bool CanAddSheets() {
            if(string.IsNullOrWhiteSpace(AddSheetsAlbumBlueprint)) {
                AddSheetsErrorText = "Выберите альбом.";
                return false;
            }

            if(!NamingUtils.IsValidName(AddSheetsAlbumBlueprint)) {
                AddSheetsErrorText = $"Некорректное название альбома '{AddSheetsAlbumBlueprint}'";
                return false;
            }

            if(AddSheetsTitleBlock is null) {
                AddSheetsErrorText = "Выберите штамп.";
                return false;
            }

            if(!int.TryParse(AddSheetsCount, out _)) {
                AddSheetsErrorText = "Количество листов должно быть целым числом.";
                return false;
            }

            if(int.TryParse(AddSheetsCount, out int number) && number > _maxSheetsCountToAdd) {
                AddSheetsErrorText = string.Format("Количество листов не должно быть больше {0}.", _maxSheetsCountToAdd);
                return false;
            }

            if(int.Parse(AddSheetsCount) <= 0) {
                AddSheetsErrorText = "Количество листов должно быть положительным числом.";
                return false;
            }

            AddSheetsErrorText = null;
            return true;
        }

        private void RemoveSheet(SheetViewModel sheet) {
            if(sheet.IsPlaced) {
                sheet.SheetModel.MarkAsDeleted();
            } else {
                AllSheets.Remove(sheet);
            }
            var editableView = VisibleSheets.View as IEditableCollectionView;
            if(editableView?.IsEditingItem ?? false) {
                editableView.CommitEdit();
            }
            VisibleSheets.View.Refresh();
        }

        private bool CanRemoveSheet(SheetViewModel sheet) {
            return sheet is not null;
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
            if(AllSheets.Any(item => string.IsNullOrWhiteSpace(item.AlbumBlueprint))) {
                ErrorText = "У всех листов должно быть заполнен альбом.";
                return false;
            }

            if(AllSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.AlbumBlueprint)) is SheetViewModel sheet) {
                ErrorText = $"У листа '{sheet.Name}' недопустимый альбом.";
                return false;
            }

            if(AllSheets.Any(item => string.IsNullOrWhiteSpace(item.SheetNumber))) {
                ErrorText = "У всех листов должно быть задан номер.";
                return false;
            }

            if(AllSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.SheetNumber)) is SheetViewModel sheet1) {
                ErrorText = $"У листа '{sheet1.Name}' недопустимый номер.";
                return false;
            }

            if(AllSheets.Any(item => string.IsNullOrWhiteSpace(item.Name))) {
                ErrorText = "У всех листов должно быть заполнено наименование.";
                return false;
            }

            if(AllSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.Name)) is SheetViewModel sheet2) {
                ErrorText = $"У листа '{sheet2.Name}' недопустимое название.";
                return false;
            }

            if(AllSheets.Any(item => item.TitleBlock is null)) {
                ErrorText = "У всех листов должна быть выбрана основная надпись.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SheetsFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is SheetViewModel sheetViewModel) {
                if(sheetViewModel.SheetModel.State == EntityState.Deleted) {
                    e.Accepted = false;
                    return;
                }
                if(!string.IsNullOrWhiteSpace(SheetsFilter)) {
                    if(!sheetViewModel.AlbumBlueprint.Contains(SheetsFilter)
                        && !sheetViewModel.SheetNumber.Contains(SheetsFilter)
                        && !sheetViewModel.Name.Contains(SheetsFilter)
                        && (!sheetViewModel.TitleBlock?.Name.Contains(SheetsFilter) ?? false)) {

                        e.Accepted = false;
                        return;
                    }
                }
            }
        }

        private void AddViewPort() {
            try {
                var viewPort = _sheetItemsFactory.CreateViewPort(SelectedSheet.SheetModel);
                SelectedSheet.SheetModel.AddViewPort(viewPort);
                SelectedSheet.AllViewPorts.Add(new ViewPortViewModel(viewPort));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddSchedule() {
            try {
                var schedule = _sheetItemsFactory.CreateSchedule(SelectedSheet.SheetModel);
                SelectedSheet.SheetModel.AddSchedule(schedule);
                SelectedSheet.AllSchedules.Add(new ScheduleViewModel(schedule));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddAnnotation() {
            try {
                var annotation = _sheetItemsFactory.CreateAnnotation(SelectedSheet.SheetModel);
                SelectedSheet.SheetModel.AddAnnotation(annotation);
                SelectedSheet.AllAnnotations.Add(new AnnotationViewModel(annotation));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private bool CanAddSheetItem() {
            return SelectedSheet is not null;
        }
    }
}

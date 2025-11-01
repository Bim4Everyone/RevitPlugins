using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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

        private readonly ObservableCollection<SheetViewModel> _sheets = [];
        private readonly RevitRepository _revitRepository;
        private readonly EntitiesHandler _sheetsHandler;
        private readonly EntitiesTracker _entitiesTracker;
        private readonly EntitySaverProvider _entitySaverProvider;
        private readonly PluginConfig _config;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IOpenFileDialogService _openFileService;
        private readonly ISaveFileDialogService _saveFileService;
        private readonly IProgressDialogFactory _progressFactory;
        private readonly SheetItemsFactory _sheetItemsFactory;
        private readonly LogicalStringComparer _comparer = new();
        private TitleBlockViewModel _addSheetsTitleBlock;
        private SheetFormatViewModel _addSheetsFormat;
        private OrientationViewModel _addSheetsOrientation;
        private string _errorText;
        private string _addSheetsErrorText;
        private string _addSheetsCount = "1";
        private string _albumBlueprints;
        private string _sheetsFilter;
        private bool _numberByMask = true;
        private string _numerationErrorText;
        private string _numerationStartNumber = "1";
        private string _numerationSelectedColumn;
        private SheetViewModel _selectedSheet;
        private CollectionViewSource _visibleSheets;
        private ObservableCollection<SheetViewModel> _selectedSheets = [];

        public MainViewModel(
            RevitRepository revitRepository,
            EntitiesHandler entitiesHandler,
            EntitiesTracker entitiesTracker,
            EntitySaverProvider entitySaverProvider,
            SheetItemsFactory sheetItemsFactory,
            PluginConfig config,
            ILocalizationService localizationService,
            IMessageBoxService messageBoxService,
            IOpenFileDialogService openFileDialogService,
            ISaveFileDialogService saveFileDialogService,
            IProgressDialogFactory progressDialogFactory) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _sheetsHandler = entitiesHandler ?? throw new ArgumentNullException(nameof(entitiesHandler));
            _entitiesTracker = entitiesTracker ?? throw new ArgumentNullException(nameof(entitiesTracker));
            _entitySaverProvider = entitySaverProvider ?? throw new ArgumentNullException(nameof(entitySaverProvider));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _openFileService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
            _saveFileService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
            _progressFactory = progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));
            _sheetItemsFactory = sheetItemsFactory ?? throw new ArgumentNullException(nameof(sheetItemsFactory));
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            RemoveSheetsCommand = RelayCommand.Create<ICollection<SheetViewModel>>(RemoveSheets, CanRemoveSheet);
            AddSheetsCommand = RelayCommand.Create(AddSheets, CanAddSheets);
            LoadSheetsCommand = RelayCommand.Create(LoadSheets);
            SaveSheetsCommand = RelayCommand.Create(SaveSheets, CanSaveSheets);
            NumberSheetsCommand = RelayCommand.Create<ICollection<SheetViewModel>>(NumberSheets, CanNumberSheets);

            AllAlbumsBlueprints = [.. _revitRepository.GetAlbumsBlueprints().OrderBy(item => item, _comparer)];
            AllTitleBlocks = [.. _revitRepository.GetTitleBlockSymbols()
                .Select(item => new TitleBlockViewModel(item))
                .OrderBy(item => item.Name, _comparer)];
            AllViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(item => item.Name, _comparer)];
            AllSheetFormats = SheetFormatViewModel.GetStandardSheetFormats();
            Orientations = [new OrientationViewModel(_localizationService, false),
                new OrientationViewModel(_localizationService, true)];

            NumerationColumns = [
                _localizationService.GetLocalizedString("MainWindow.AllSheets.CustomNumber"),
                _localizationService.GetLocalizedString("MainWindow.AllSheets.Number")];
            NumerationSelectedColumn = NumerationColumns.First();
        }

        public IProgressDialogFactory ProgressDialogFactory => _progressFactory;

        public IOpenFileDialogService OpenFileDialogService => _openFileService;

        public ISaveFileDialogService SaveFileDialogService => _saveFileService;

        public IMessageBoxService MessageBoxService => _messageBoxService;

        public ICommand RemoveSheetsCommand { get; }

        public ICommand AddSheetsCommand { get; }

        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand LoadSheetsCommand { get; }

        public ICommand SaveSheetsCommand { get; }

        public ICommand NumberSheetsCommand { get; }

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
            get => _addSheetsErrorText;
            set => RaiseAndSetIfChanged(ref _addSheetsErrorText, value);
        }

        public SheetFormatViewModel AddSheetsFormat {
            get => _addSheetsFormat;
            set => RaiseAndSetIfChanged(ref _addSheetsFormat, value);
        }

        public OrientationViewModel AddSheetsOrientation {
            get => _addSheetsOrientation;
            set => RaiseAndSetIfChanged(ref _addSheetsOrientation, value);
        }

        public string AddSheetsCount {
            get => _addSheetsCount;
            set => RaiseAndSetIfChanged(ref _addSheetsCount, value);
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
            set {
                if(!(_selectedSheet?.Equals(value) ?? false)) {
                    if(_selectedSheet is not null) {
                        _selectedSheet.PropertyChanged -= OnSelectedSheetChanged;
                    }
                    RaiseAndSetIfChanged(ref _selectedSheet, value);
                    if(_selectedSheet is not null) {
                        _selectedSheet.PropertyChanged += OnSelectedSheetChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Включает назначение системного номера листа в формате Альбом-Ш.Номер листа
        /// </summary>
        public bool NumberByMask {
            get => _numberByMask;
            set {
                if(value && ShowYesNoWarning(_localizationService.GetLocalizedString(
                    "MainWindow.AllSheets.Numeration.SyncSystemNumberByMask.Warning"))) {
                    NumerationSelectedColumn = NumerationColumns.First();
                    UpdateSheetsNumberByMask(value);
                    RaiseAndSetIfChanged(ref _numberByMask, value);
                } else if(!value) {
                    UpdateSheetsNumberByMask(value);
                    RaiseAndSetIfChanged(ref _numberByMask, value);
                }
            }
        }

        public string NumerationStartNumber {
            get => _numerationStartNumber;
            set => RaiseAndSetIfChanged(ref _numerationStartNumber, value);
        }

        public string NumerationErrorText {
            get => _numerationErrorText;
            set => RaiseAndSetIfChanged(ref _numerationErrorText, value);
        }

        public string NumerationSelectedColumn {
            get => _numerationSelectedColumn;
            set => RaiseAndSetIfChanged(ref _numerationSelectedColumn, value);
        }

        public IReadOnlyCollection<string> NumerationColumns { get; }

        public ObservableCollection<SheetViewModel> SelectedSheets {
            get => _selectedSheets;
            set => RaiseAndSetIfChanged(ref _selectedSheets, value);
        }

        public CollectionViewSource VisibleSheets {
            get => _visibleSheets;
            set => RaiseAndSetIfChanged(ref _visibleSheets, value);
        }


        public IReadOnlyCollection<string> AllAlbumsBlueprints { get; }

        public IReadOnlyCollection<TitleBlockViewModel> AllTitleBlocks { get; }

        public IReadOnlyCollection<ViewPortTypeViewModel> AllViewPortTypes { get; }

        public IReadOnlyCollection<SheetFormatViewModel> AllSheetFormats { get; }

        public IReadOnlyCollection<OrientationViewModel> Orientations { get; }


        private void LoadView() {
            var sheets = _revitRepository.GetSheetModels()
                .Select(s => new SheetViewModel(s, _entitiesTracker, _sheetItemsFactory, _localizationService))
                .OrderBy(s => s.AlbumBlueprint + s.SheetNumber, _comparer)
                .ToArray();
            for(int i = 0; i < sheets.Length; i++) {
                _sheets.Add(sheets[i]);
            }
            UpdateSchedulesCount();
            ((INotifyCollectionChanged) _entitiesTracker.AliveSchedules).CollectionChanged += OnSchedulesChanged;
            VisibleSheets = new CollectionViewSource() { Source = _sheets };
            VisibleSheets.Filter += SheetsFilterHandler;

            var settings = GetSettings();
            AddSheetsAlbumBlueprint = AllAlbumsBlueprints.FirstOrDefault();
            AddSheetsTitleBlock = AllTitleBlocks.FirstOrDefault(
                t => t.TitleBlockSymbol.Id == settings.AddSheetsTitleBlock) ?? AllTitleBlocks.FirstOrDefault();
        }

        private void OnSchedulesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateSchedulesCount();
        }

        private void UpdateSchedulesCount() {
            var dict = _entitiesTracker.AliveSchedules.GroupBy(s => s.Name).ToDictionary(g => g.Key, g => g.Count());
            foreach(var schedule in _sheets.SelectMany(s => s.Schedules).ToArray()) {
                schedule.CountOnSheets = dict.TryGetValue(schedule.Name, out int count) ? count : 1;
            }
        }

        private void AddSheets() {
            var indexes = _sheets
                .Select(s => new { IsNumber = int.TryParse(s.SheetCustomNumber, out int number), Number = number })
                .Where(c => c.IsNumber)
                .ToArray();
            int lastIndex = indexes.Length > 0 ? indexes.Max(c => c.Number) : 0;
            var titleBlock = AddSheetsTitleBlock;
            var albumBlueprint = AddSheetsAlbumBlueprint;
            var format = AddSheetsFormat;
            var orientation = AddSheetsOrientation;
            foreach(int index in Enumerable.Range(0, int.Parse(AddSheetsCount))) {
                ++lastIndex;
                var sheetModel = new SheetModel(titleBlock.TitleBlockSymbol, _entitySaverProvider.GetNewEntitySaver());
                var sheetViewModel = new SheetViewModel(
                    sheetModel, _entitiesTracker, _sheetItemsFactory, _localizationService) {
                    AlbumBlueprint = albumBlueprint,
                    SheetCustomNumber = lastIndex.ToString(),
                    Name = $"{_localizationService.GetLocalizedString("NewSheetTitle")} {lastIndex}",
                    SheetFormat = format,
                    Orientation = orientation,
                };
                _sheets.Add(sheetViewModel);
            }
        }

        private bool CanAddSheets() {
            if(string.IsNullOrWhiteSpace(AddSheetsAlbumBlueprint)) {
                AddSheetsErrorText =
                    _localizationService.GetLocalizedString("Errors.Validation.AlbumBlueprintNotSet");
                return false;
            }

            if(!NamingUtils.IsValidName(AddSheetsAlbumBlueprint)) {
                AddSheetsErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.InvalidAlbumBlueprintName"),
                    AddSheetsAlbumBlueprint);
                return false;
            }

            if(AddSheetsTitleBlock is null) {
                AddSheetsErrorText = _localizationService.GetLocalizedString("Errors.Validation.TitleBlockNotSet");
                return false;
            }

            if(!int.TryParse(AddSheetsCount, out _)) {
                AddSheetsErrorText =
                    _localizationService.GetLocalizedString("Errors.Validation.AddSheetsCountNotNumber");
                return false;
            }

            if(int.TryParse(AddSheetsCount, out int number) && number > _maxSheetsCountToAdd) {
                AddSheetsErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.AddSheetsMaxCount"),
                    _maxSheetsCountToAdd);
                return false;
            }

            if(int.Parse(AddSheetsCount) <= 0) {
                AddSheetsErrorText = _localizationService.GetLocalizedString("Errors.Validation.AddSheetsMinCount");
                return false;
            }

            AddSheetsErrorText = null;
            return true;
        }

        private void RemoveSheets(ICollection<SheetViewModel> sheets) {
            if(sheets.Any(s => s.SheetModel.TryGetExistId(out var sheetId)
                && _revitRepository.Document.ActiveView.Id == sheetId)) {
                ShowOkWarning(string.Format(
                    _localizationService.GetLocalizedString("Errors.CannotDeleteActiveViewSheet"),
                    _revitRepository.Document.ActiveView.Name));
                return;
            }
            if(ShowYesNoWarning(
                string.Format(_localizationService.GetLocalizedString("Warnings.SureDeleteSheets"), sheets.Count))) {
                foreach(SheetViewModel sheet in sheets.ToArray()) {
                    _entitiesTracker.AddToRemovedEntities(sheet.SheetModel);
                    _sheets.Remove(sheet);
                }
                var editableView = VisibleSheets.View as IEditableCollectionView;
                if(editableView?.IsEditingItem ?? false) {
                    editableView.CommitEdit();
                }
                VisibleSheets.View.Refresh();
            }
        }

        private bool CanRemoveSheet(ICollection<SheetViewModel> sheets) {
            return sheets is not null && sheets.Count > 0;
        }

        private void AcceptView() {
            SaveConfig();
            using(var progressDialogService = _progressFactory.CreateDialog()) {
                progressDialogService.StepValue = 50;
                progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("ProgressBarTitle");
                progressDialogService.MaxValue = _entitiesTracker.GetTrackedEntitiesCount();
                var progress = progressDialogService.CreateProgress();
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                string error = _sheetsHandler.HandleTrackedEntities(progress, ct);
                if(!string.IsNullOrWhiteSpace(error)) {
                    ShowOkWarning(error);
                }
            }
        }

        private bool CanAcceptView() {
            if(_sheets.Any(item => string.IsNullOrWhiteSpace(item.Name))) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.AnySheetNameEmpty");
                return false;
            }

            if(_sheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.Name)) is SheetViewModel sheet2) {
                ErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.AnySheetInvalidName"), sheet2.Name);
                return false;
            }

            var group = _sheets.GroupBy(item => item.SheetNumber)
                .FirstOrDefault(g => g.Count() > 1);
            if(group is not null) {
                ErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.DuplicatedSheetNumber"), group.Key);
                return false;
            }

            if(_sheets.Any(s => string.IsNullOrWhiteSpace(s.SheetNumber))) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.AnySheetNumberEmpty");
                return false;
            }

            if(_sheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.SheetNumber))
                is SheetViewModel sheetInvalidNumber) {
                ErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.SheetInvalidNumber"),
                    sheetInvalidNumber.Name, sheetInvalidNumber.SheetNumber);
                return false;
            }

            if(_sheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.AlbumBlueprint ?? string.Empty))
                is SheetViewModel sheetInvalidAlbum) {
                ErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.SheetInvalidAlbum"),
                    sheetInvalidAlbum.Name, sheetInvalidAlbum.AlbumBlueprint);
                return false;
            }

            if(_sheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.SheetCustomNumber ?? string.Empty))
                is SheetViewModel sheetInvalidSheetCustomNumber) {
                ErrorText = string.Format(
                    _localizationService.GetLocalizedString("Errors.Validation.SheetInvalidCustomNumber"),
                    sheetInvalidSheetCustomNumber.Name, sheetInvalidSheetCustomNumber.SheetCustomNumber);
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SheetsFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is SheetViewModel sheetViewModel) {
                if(!string.IsNullOrWhiteSpace(SheetsFilter)) {
                    var str = SheetsFilter.ToLower();
                    e.Accepted = sheetViewModel.AlbumBlueprint.ToLower().Contains(str)
                        || sheetViewModel.SheetNumber.ToLower().Contains(str)
                        || sheetViewModel.SheetCustomNumber.ToLower().Contains(str)
                        || sheetViewModel.Name.ToLower().Contains(str)
                        || (sheetViewModel.TitleBlock?.Name.ToLower().Contains(str) ?? false)
                        || sheetViewModel.IsPlacedStatus.ToLower().Contains(str);
                    return;
                }
            }
        }

        private void SaveConfig() {
            var settings = GetSettings();
            settings.AddSheetsTitleBlock = AddSheetsTitleBlock.TitleBlockSymbol.Id;
            _config.SaveProjectConfig();
        }

        private void LoadSheets() {
            var settings = GetSettings();
            if(_openFileService.ShowDialog(settings.FileDialogInitialDirectory ?? string.Empty)) {

                var sheetDtos = _config.Serializer.Deserialize<SheetsCreationDto>(File.ReadAllText(
                    _openFileService.File.FullName));
                var titleBlocks = AllTitleBlocks.Select(t => t.TitleBlockSymbol).ToArray();
                var newSheetViewModels = sheetDtos.Sheets?
                    .Select(s => new SheetViewModel(
                        s.CreateSheetModel(titleBlocks, _entitySaverProvider.GetNewEntitySaver()),
                        _entitiesTracker,
                        _sheetItemsFactory,
                        _localizationService))
                    ?? [];
                foreach(var sheetViewModel in newSheetViewModels) {
                    sheetViewModel.SheetNumberByMask = NumberByMask;
                    _sheets.Add(sheetViewModel);
                }

                settings.FileDialogInitialDirectory = Path.GetDirectoryName(_openFileService.File.FullName);
                SaveConfig();
            }
        }

        private void SaveSheets() {
            var settings = GetSettings();
            if(_saveFileService.ShowDialog(settings.FileDialogInitialDirectory ?? string.Empty,
                "sheets_config.json")) {

                var sheetDtos = new SheetsCreationDto(_sheets.Select(s => s.SheetModel));
                var path = _saveFileService.File.FullName;
                File.WriteAllText(path, _config.Serializer.Serialize(sheetDtos));

                settings.FileDialogInitialDirectory = Path.GetDirectoryName(path);
                SaveConfig();
            }
        }

        private RevitSettings GetSettings() {
            return _config.GetSettings(_revitRepository.Document)
                ?? _config.AddSettings(_revitRepository.Document);
        }

        private bool CanSaveSheets() {
            return _sheets.Count > 0;
        }

        private void OnSelectedSheetChanged(object sender, PropertyChangedEventArgs e) {
            if(string.IsNullOrWhiteSpace(e.PropertyName)) {
                return;
            }
            var prop = typeof(SheetViewModel).GetProperty(e.PropertyName);
            if(prop is null) {
                return;
            }
            if(prop.Name == nameof(SheetViewModel.AlbumBlueprint)
                || prop.Name == nameof(SheetViewModel.Name)
                || prop.Name == nameof(SheetViewModel.TitleBlock)
                || prop.Name == nameof(SheetViewModel.SheetFormat)
                || prop.Name == nameof(SheetViewModel.Orientation)) {
                // мультиредактирование только для названия, альбома, основной надписи, формата и ориентации
                var newValue = prop.GetValue(SelectedSheet);

                SheetViewModel[] sheets = [.. SelectedSheets];
                foreach(var item in sheets) {
                    if(!item.Equals(SelectedSheet)) {
                        prop.SetValue(item, newValue);
                    }
                }
            }
        }

        private void NumberSheets(ICollection<SheetViewModel> sheets) {
            var start = int.Parse(NumerationStartNumber);
            if(NumerationSelectedColumn == _localizationService.GetLocalizedString("MainWindow.AllSheets.Number")) {
                foreach(var sheet in sheets) {
                    sheet.SheetNumber = start.ToString();
                    start++;
                }
            } else {
                foreach(var sheet in sheets) {
                    sheet.SheetCustomNumber = start.ToString();
                    start++;
                }
            }
        }

        private bool CanNumberSheets(ICollection<SheetViewModel> sheets) {
            if(sheets is null || sheets.Count == 0) {
                NumerationErrorText = _localizationService.GetLocalizedString(
                        "MainWindow.AllSheets.Numeration.Errors.SelectedSheetsNotSet");
                return false;
            }

            if(string.IsNullOrWhiteSpace(NumerationStartNumber)) {
                NumerationErrorText = _localizationService.GetLocalizedString(
                        "MainWindow.AllSheets.Numeration.Errors.StartNumberNotSet");
                return false;
            }

            if(!int.TryParse(NumerationStartNumber, out int number) || number < 0) {
                NumerationErrorText = _localizationService.GetLocalizedString(
                        "MainWindow.AllSheets.Numeration.Errors.StartNumberInvalid");
                return false;
            }

            NumerationErrorText = null;
            return true;
        }

        /// <summary>
        /// Показывает предупреждение с OK кнопкой
        /// </summary>
        private void ShowOkWarning(string msg) {
            _messageBoxService.Show(
                msg,
                _localizationService.GetLocalizedString("Warnings.Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>
        /// Показывает предупреждение с Yes/No кнопками
        /// </summary>
        private bool ShowYesNoWarning(string msg) {
            return _messageBoxService.Show(
                msg,
                _localizationService.GetLocalizedString("Warnings.Title"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes;
        }

        private void UpdateSheetsNumberByMask(bool sheetNumberByMask) {
            foreach(var sheet in _sheets) {
                sheet.SheetNumberByMask = sheetNumberByMask;
            }
        }
    }
}

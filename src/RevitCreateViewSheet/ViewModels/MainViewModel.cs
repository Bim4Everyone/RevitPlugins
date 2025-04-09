using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private const int _maxSheetsCountToAdd = 1000;

        private readonly ObservableCollection<SheetViewModel> _allSheets;
        private readonly RevitRepository _revitRepository;
        private readonly SheetsSaver _sheetsSaver;
        private readonly IConfigSerializer _configSerializer;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IOpenFileDialogService _openFileService;
        private readonly ISaveFileDialogService _saveFileService;
        private readonly IProgressDialogFactory _progressFactory;
        private readonly ISheetItemsFactory _sheetItemsFactory;
        private TitleBlockViewModel _addSheetsTitleBlock;
        private string _errorText;
        private string _createErrorText;
        private string _countCreateView;
        private string _albumBlueprints;
        private string _sheetsFilter;
        private SheetViewModel _selectedSheet;
        private CollectionViewSource _visibleSheets;
        private ObservableCollection<SheetViewModel> _selectedSheets;

        public MainViewModel(
            RevitRepository revitRepository,
            SheetsSaver sheetsSaver,
            IConfigSerializer configSerializer,
            ILocalizationService localizationService,
            IMessageBoxService messageBoxService,
            IOpenFileDialogService openFileDialogService,
            ISaveFileDialogService saveFileDialogService,
            IProgressDialogFactory progressDialogFactory,
            ISheetItemsFactory sheetItemsFactory) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _sheetsSaver = sheetsSaver ?? throw new ArgumentNullException(nameof(sheetsSaver));
            _configSerializer = configSerializer ?? throw new ArgumentNullException(nameof(configSerializer));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
            _openFileService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
            _saveFileService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));
            _progressFactory = progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));
            _sheetItemsFactory = sheetItemsFactory ?? throw new ArgumentNullException(nameof(sheetItemsFactory));
            _selectedSheets = [];
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            RemoveSheetCommand = RelayCommand.Create<ICollection<SheetViewModel>>(RemoveSheet, CanRemoveSheet);
            AddSheetsCommand = RelayCommand.Create(AddSheets, CanAddSheets);
            AddViewPortCommand = RelayCommand.Create(AddViewPort, CanAddSheetItem);
            AddScheduleCommand = RelayCommand.Create(AddSchedule, CanAddSheetItem);
            AddAnnotationCommand = RelayCommand.Create(AddAnnotation, CanAddSheetItem);
            LoadSheetsCommand = RelayCommand.Create(LoadSheets);
            SaveSheetsCommand = RelayCommand.Create(SaveSheets, CanSaveSheets);

            var comparer = new LogicalStringComparer();
            _allSheets = [];
            AllAlbumsBlueprints = [.. _revitRepository.GetAlbumsBlueprints().OrderBy(item => item, comparer)];
            AllTitleBlocks = [.. _revitRepository.GetTitleBlocks()
                .Select(item => new TitleBlockViewModel(item))
                .OrderBy(item => item.Name, comparer)];
            AllViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(item => item.Name, comparer)];

            AddSheetsCount = "1";
            AddSheetsAlbumBlueprint = AllAlbumsBlueprints.FirstOrDefault();
            AddSheetsTitleBlock = AllTitleBlocks.FirstOrDefault();
        }

        public IProgressDialogFactory ProgressDialogFactory => _progressFactory;

        public IOpenFileDialogService OpenFileDialogService => _openFileService;

        public ISaveFileDialogService SaveFileDialogService => _saveFileService;

        public IMessageBoxService MessageBoxService => _messageBoxService;

        public ICommand RemoveSheetCommand { get; }

        public ICommand AddSheetsCommand { get; }

        public ICommand LoadViewCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand AddViewPortCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public ICommand LoadSheetsCommand { get; }

        public ICommand SaveSheetsCommand { get; }

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


        private void LoadView() {
            var sheets = _revitRepository.GetSheetModels()
                .Select(s => new SheetViewModel(s))
                .OrderBy(s => s.AlbumBlueprint + s.SheetNumber, new LogicalStringComparer())
                .ToArray();
            for(int i = 0; i < sheets.Length; i++) {
                _allSheets.Add(sheets[i]);
            }
            VisibleSheets = new CollectionViewSource() { Source = _allSheets };
            VisibleSheets.Filter += SheetsFilterHandler;
        }

        private void AddSheets() {
            var indexes = _allSheets.Where(s => s.SheetModel.State != EntityState.Deleted)
                .Select(s => new { IsNumber = int.TryParse(s.SheetCustomNumber, out int number), Number = number })
                .Where(c => c.IsNumber)
                .ToArray();
            int lastIndex = indexes.Length > 0 ? indexes.Max(c => c.Number) : 0;
            var titleBlock = AddSheetsTitleBlock;
            var albumBlueprint = AddSheetsAlbumBlueprint;
            foreach(int index in Enumerable.Range(0, int.Parse(AddSheetsCount))) {
                ++lastIndex;
                var sheetModel = new SheetModel(titleBlock.TitleBlockSymbol);
                var sheetViewModel = new SheetViewModel(sheetModel) {
                    AlbumBlueprint = albumBlueprint,
                    SheetCustomNumber = lastIndex.ToString(),
                    Name = $"{_localizationService.GetLocalizedString("TODO")} {lastIndex}"
                };
                _allSheets.Add(sheetViewModel);
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

        private void RemoveSheet(ICollection<SheetViewModel> sheets) {
            if(_messageBoxService.Show(
                $"Вы действительно хотите удалить листы ({sheets.Count} шт.)?",
                "Предупреждение",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes) {

                foreach(SheetViewModel sheet in sheets) {
                    if(sheet.IsPlaced) {
                        sheet.SheetModel.MarkAsDeleted();
                    } else {
                        _allSheets.Remove(sheet);
                    }
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
            var sheets = _allSheets.Select(s => s.SheetModel).ToArray();
            if(sheets.Any()) {
                using(var progressDialogService = _progressFactory.CreateDialog()) {
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
            var notDeletedSheets = GetNotDeletedSheets();
            if(notDeletedSheets.Any(item => string.IsNullOrWhiteSpace(item.Name))) {
                ErrorText = "У всех листов должно быть заполнено наименование.";
                return false;
            }

            if(notDeletedSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.Name)) is SheetViewModel sheet2) {
                ErrorText = $"У листа '{sheet2.Name}' недопустимое название.";
                return false;
            }

            var group = notDeletedSheets.GroupBy(item => item.SheetNumber)
                .FirstOrDefault(g => g.Count() > 1);
            if(group is not null) {
                ErrorText = $"У листов повторяется номер {group.Key}.";
                return false;
            }

            if(notDeletedSheets.Any(s => string.IsNullOrWhiteSpace(s.SheetNumber))) {
                ErrorText = $"У всех листов должен быть заполнен системный номер";
                return false;
            }

            if(notDeletedSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.SheetNumber))
                is SheetViewModel sheetInvalidNumber) {
                ErrorText = $"У листа '{sheetInvalidNumber.Name}' недопустимый системный номер.";
                return false;
            }

            if(notDeletedSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.AlbumBlueprint ?? string.Empty))
                is SheetViewModel sheetInvalidAlbum) {
                ErrorText = $"У листа '{sheetInvalidAlbum.Name}' недопустимый альбом.";
                return false;
            }

            if(notDeletedSheets.FirstOrDefault(s => !NamingUtils.IsValidName(s.SheetCustomNumber ?? string.Empty))
                is SheetViewModel sheetInvalidSheetCustomNumber) {
                ErrorText = $"У листа '{sheetInvalidSheetCustomNumber.Name}' недопустимый Ш.Номер листа.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private ICollection<SheetViewModel> GetNotDeletedSheets() {
            return [.. _allSheets.Where(s => s.SheetModel.State != EntityState.Deleted)];
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
                        && !sheetViewModel.SheetCustomNumber.Contains(SheetsFilter)
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
                // Особенности размещения видов на листах в ревите:
                //   - на одном листе нельзя разместить несколько экземпляров одного и того же вида
                //   - легенды можно размещать на листах повторно на других листах
                var viewPort = _sheetItemsFactory.CreateViewPort(
                    SelectedSheet.SheetModel,
                    GetNotDeletedSheets()
                    .SelectMany(s => s.GetViewPorts())
                    .Where(v => v.ViewPortModel.View.ViewType != ViewType.Legend)
                    .Union(SelectedSheet.GetViewPorts())
                    .Select(v => v.ViewPortModel.View)
                    .ToArray());
                SelectedSheet.SheetModel.AddViewPort(viewPort);
                SelectedSheet.AddView(new ViewPortViewModel(viewPort));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddSchedule() {
            try {
                var schedule = _sheetItemsFactory.CreateSchedule(SelectedSheet.SheetModel);
                SelectedSheet.SheetModel.AddSchedule(schedule);
                SelectedSheet.AddSchedule(new ScheduleViewModel(schedule));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddAnnotation() {
            try {
                var annotation = _sheetItemsFactory.CreateAnnotation(SelectedSheet.SheetModel);
                SelectedSheet.SheetModel.AddAnnotation(annotation);
                SelectedSheet.AddAnnotation(new AnnotationViewModel(annotation));
            } catch(OperationCanceledException) {
                return;
            }
        }

        private bool CanAddSheetItem() {
            return SelectedSheet is not null;
        }

        private void LoadSheets() {
            var config = PluginConfig.GetPluginConfig(_configSerializer);
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);

            if(_openFileService.ShowDialog(
                config.GetSettings(_revitRepository.Document)?.FileDialogInitialDirectory ?? string.Empty)) {

                var sheetDtos = _configSerializer.Deserialize<SheetsCreationDto>(File.ReadAllText(
                    _openFileService.File.FullName));
                var titleBlocks = AllTitleBlocks.Select(t => t.TitleBlockSymbol).ToArray();
                var newSheetViewModels = sheetDtos.Sheets?
                    .Select(s => new SheetViewModel(s.CreateSheetModel(titleBlocks)))
                    ?? [];
                foreach(var sheetViewModel in newSheetViewModels) {
                    _allSheets.Add(sheetViewModel);
                }

                settings.FileDialogInitialDirectory = Path.GetDirectoryName(_openFileService.File.FullName);
                config.SaveProjectConfig();
            }
        }

        private void SaveSheets() {
            var config = PluginConfig.GetPluginConfig(_configSerializer);
            var settings = config.GetSettings(_revitRepository.Document)
                ?? config.AddSettings(_revitRepository.Document);

            if(_saveFileService.ShowDialog(
                config.GetSettings(_revitRepository.Document)?.FileDialogInitialDirectory ?? string.Empty,
                "sheets_config.json")) {

                var sheetDtos = new SheetsCreationDto(GetNotDeletedSheets().Select(s => s.SheetModel));
                var path = _saveFileService.File.FullName;
                File.WriteAllText(path, _configSerializer.Serialize(sheetDtos));

                settings.FileDialogInitialDirectory = Path.GetDirectoryName(path);
                config.SaveProjectConfig();
            }
        }

        private bool CanSaveSheets() {
            return GetNotDeletedSheets().Count > 0;
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
                || prop.Name == nameof(SheetViewModel.TitleBlock)) {
                // мультиредактирование только для названия, альбома и основной надписи
                var newValue = prop.GetValue(SelectedSheet);

                SheetViewModel[] sheets = [.. SelectedSheets];
                foreach(var item in sheets) {
                    if(!item.Equals(SelectedSheet)) {
                        prop.SetValue(item, newValue);
                    }
                }
            }
        }
    }
}

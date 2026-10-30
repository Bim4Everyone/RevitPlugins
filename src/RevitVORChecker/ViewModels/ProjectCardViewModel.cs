using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.ViewModels;

/// <summary>
/// Окно «Данные проекта»: сведения из ЗнП и нормы армирования в виде табл. 2.6.4.
/// </summary>
internal class ProjectCardViewModel : BaseViewModel {
    /// <summary>
    /// Группы строк табл. 2.6.4, которые должны быть заполнены для корпусов.
    /// </summary>
    private static readonly string[][] _residentialGroups = {
        new[] { "K1", "K2" }, new[] { "K3" }, new[] { "K4", "K5" }, new[] { "K6", "K7" }, new[] { "K8" },
        new[] { "K9" }, new[] { "K10" }, new[] { "K11" }
    };

    private readonly ILocalizationService _localizationService;
    private string _name = string.Empty;
    private string _atsNumber = string.Empty;
    private string _assignmentDate = string.Empty;
    private string _stage = string.Empty;
    private int _storeysIndex;
    private int _foundationRuleIndex;
    private string _clashVolumeDecimals = string.Empty;
    private string _lowRiseBlocks = string.Empty;
    private string _highlightHint = string.Empty;
    private string? _errorText;

    public ProjectCardViewModel(
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        IOpenFileDialogService openFileDialogService) {

        _localizationService = localizationService;
        MessageBoxService = messageBoxService;
        OpenFileDialogService = openFileDialogService;
        FillDefaultTableCommand = RelayCommand.Create(FillDefaultTable);
        ImportProjectFileCommand = RelayCommand.Create(ImportProjectFile);
        AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        StoreysOptionTitles = StoreysOptions.Titles.ToList();
        FoundationRuleTitles = new List<string> {
            _localizationService.GetLocalizedString("ProjectCard.FoundationToBuilding"),
            _localizationService.GetLocalizedString("ProjectCard.FoundationToParking")
        };
        SetData(ProjectCardStorage.CreateDefault());
    }

    public IMessageBoxService MessageBoxService { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    public ICommand FillDefaultTableCommand { get; }

    public ICommand ImportProjectFileCommand { get; }

    /// <summary>
    /// Проверка ввода (кнопка «Сохранить» недоступна при ошибке).
    /// </summary>
    public ICommand AcceptViewCommand { get; }

    public List<string> AtsOptions { get; } = new() { "3.0", "5.0" };

    public List<string> StageOptions { get; } = new() { "П", "Р" };

    public List<string> StoreysOptionTitles { get; }

    public List<string> FoundationRuleTitles { get; }

    public ObservableCollection<ReinforcementRowViewModel> Rows { get; } = new();

    /// <summary>
    /// Данные проекта после сохранения.
    /// </summary>
    public ProjectCard? Result { get; private set; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string AtsNumber {
        get => _atsNumber;
        set => RaiseAndSetIfChanged(ref _atsNumber, value);
    }

    public string AssignmentDate {
        get => _assignmentDate;
        set => RaiseAndSetIfChanged(ref _assignmentDate, value);
    }

    public string Stage {
        get => _stage;
        set => RaiseAndSetIfChanged(ref _stage, value);
    }

    public int StoreysIndex {
        get => _storeysIndex;
        set {
            RaiseAndSetIfChanged(ref _storeysIndex, value);
            OnPropertyChanged(nameof(IsLowRiseBlocksEnabled));
            UpdateHighlight();
        }
    }

    public int FoundationRuleIndex {
        get => _foundationRuleIndex;
        set => RaiseAndSetIfChanged(ref _foundationRuleIndex, value);
    }

    public string ClashVolumeDecimals {
        get => _clashVolumeDecimals;
        set => RaiseAndSetIfChanged(ref _clashVolumeDecimals, value);
    }

    /// <summary>
    /// Корпуса до 9 этажей при разной этажности (через запятую).
    /// </summary>
    public string LowRiseBlocks {
        get => _lowRiseBlocks;
        set => RaiseAndSetIfChanged(ref _lowRiseBlocks, value);
    }

    public bool IsLowRiseBlocksEnabled => Storeys == StoreysOption.Mixed;

    public string HighlightHint {
        get => _highlightHint;
        set => RaiseAndSetIfChanged(ref _highlightHint, value);
    }

    public string? ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private StoreysOption Storeys => (StoreysOption) Math.Max(0, StoreysIndex);

    /// <summary>
    /// Заполняет окно данными проекта.
    /// </summary>
    public void SetData(ProjectCard card) {
        Name = card.Name ?? "";
        AtsNumber = card.AtsNumber ?? "5.0";
        AssignmentDate = card.AssignmentDate ?? "";
        Stage = (card.Stage ?? "П").StartsWith("Р") ? "Р" : "П";
        LowRiseBlocks = string.Join(", ", card.LowRiseBlocks ?? new List<string>());
        FoundationRuleIndex = card.FoundationSlabRule == ProjectCard.ParkingFoundationRule ? 1 : 0;
        ClashVolumeDecimals = card.ClashVolumeDecimals.ToString();
        StoreysOption storeys = DetectStoreys(card);
        StoreysIndex = (int) storeys;
        FillTable(ReinforcementNorms.CreateTable(card, storeys));
    }

    /// <summary>
    /// Проверяет ввод, спрашивает про незаполненные нормы и собирает данные проекта.
    /// </summary>
    /// <returns>true — данные собраны в <see cref="Result"/>.</returns>
    public bool TryAccept() {
        if(!CanAcceptView()) {
            return false;
        }

        // строки с неверными значениями отсеяны проверкой ввода
        var table = Rows.Select(row => row.ToTableRow()).OfType<ReinforcementTableRow>().ToList();
        var lowRiseBlocks = ParseLowRiseBlocks();
        List<string> emptyNorms = FindEmptyNorms(table);
        if(emptyNorms.Count > 0) {
            string message = string.Format(
                _localizationService.GetLocalizedString("ProjectCard.EmptyNormsQuestion"),
                string.Join(", ", emptyNorms));
            MessageBoxResult answer = MessageBoxService.Show(
                message,
                _localizationService.GetLocalizedString("ProjectCard.Title"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Cancel);
            if(answer != MessageBoxResult.OK) {
                return false;
            }
        }

        bool isMixed = Storeys == StoreysOption.Mixed;
        Result = new ProjectCard {
            Name = Name.Trim(),
            AtsNumber = (AtsNumber ?? "").Trim(),
            AssignmentDate = AssignmentDate.Trim(),
            Stage = Stage ?? "П",
            Storeys = StoreysOptions.GetTitle(Storeys),
            FoundationSlabRule = FoundationRuleIndex == 1
                ? ProjectCard.ParkingFoundationRule
                : ProjectCard.KnowledgeBaseFoundationRule,
            ClashVolumeDecimals = int.Parse(ClashVolumeDecimals.Trim()),
            ReinforcementRules = ReinforcementNorms.CreateRules(table, Storeys, lowRiseBlocks),
            ReinforcementTable = ReinforcementNorms.ToTableValues(table),
            LowRiseBlocks = isMixed ? lowRiseBlocks : new List<string>()
        };
        return true;
    }

    /// <summary>
    /// Этажность по сохранённым данным; для старых данных — по тексту или по правилам «7–9» отдельных корпусов.
    /// </summary>
    private static StoreysOption DetectStoreys(ProjectCard card) {
        string storeys = card.Storeys ?? "";
        int index = Array.IndexOf(StoreysOptions.Titles, storeys);
        if(index >= 0) {
            return (StoreysOption) index;
        }

        bool hasLowRiseBlockRules = card.ReinforcementRules.Any(rule =>
            (rule.Note ?? "").Contains("7–9") && (rule.Blocks ?? new List<string>()).Any(block => block.StartsWith("Корпус ")));
        if(hasLowRiseBlockRules || card.LowRiseBlocks?.Count > 0) {
            if(!(card.LowRiseBlocks?.Count > 0)) {
                card.LowRiseBlocks = card.ReinforcementRules
                    .Where(rule => (rule.Note ?? "").Contains("7–9"))
                    .SelectMany(rule => rule.Blocks ?? new List<string>())
                    .Where(block => block.StartsWith("Корпус") && block != "Корпус")
                    .Distinct()
                    .ToList();
            }

            return StoreysOption.Mixed;
        }

        if(storeys.Contains("до 9") || storeys.Contains("7–9") || storeys.Contains("7-9")) {
            return StoreysOption.UpTo9;
        }

        return storeys.Contains("18") ? StoreysOption.From18 : StoreysOption.From10To17;
    }

    private void FillTable(List<ReinforcementTableRow> table) {
        Rows.Clear();
        string residentialTitle = _localizationService.GetLocalizedString("ProjectCard.ResidentialGroup");
        string parkingTitle = _localizationService.GetLocalizedString("ProjectCard.ParkingGroup");
        foreach(ReinforcementTableRow row in table.Where(row => !row.IsParking)) {
            Rows.Add(new ReinforcementRowViewModel(row, residentialTitle));
        }

        foreach(ReinforcementTableRow row in table.Where(row => row.IsParking)) {
            Rows.Add(new ReinforcementRowViewModel(row, parkingTitle));
        }

        UpdateHighlight();
    }

    /// <summary>
    /// Подсвечивает столбец, по которому будет проверяться армирование корпусов при выбранной этажности.
    /// </summary>
    private void UpdateHighlight() {
        bool isMixed = Storeys == StoreysOption.Mixed;
        bool usesFirstColumn = Storeys == StoreysOption.UpTo9;
        foreach(ReinforcementRowViewModel row in Rows) {
            row.IsFirstValueUsed = row.IsParking || isMixed || usesFirstColumn;
            row.IsSecondValueUsed = !row.IsParking && (isMixed || !usesFirstColumn);
        }

        string hintKey = Storeys switch {
            StoreysOption.From18 => "ProjectCard.HintEighteenPlus",
            StoreysOption.Mixed => "ProjectCard.HintMixed",
            _ => "ProjectCard.Hint"
        };
        HighlightHint = _localizationService.GetLocalizedString(hintKey);
    }

    private void FillDefaultTable() {
        MessageBoxResult answer = MessageBoxService.Show(
            _localizationService.GetLocalizedString("ProjectCard.FillDefaultQuestion"),
            _localizationService.GetLocalizedString("ProjectCard.Title"),
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question,
            MessageBoxResult.Cancel);
        if(answer == MessageBoxResult.OK) {
            FillTable(ReinforcementNorms.CreateDefaultTable());
        }
    }

    private void ImportProjectFile() {
        if(!OpenFileDialogService.ShowDialog()) {
            return;
        }

        try {
            SetData(JsonFiles.Load<ProjectCard>(OpenFileDialogService.File.FullName));
        } catch(Exception ex) {
            MessageBoxService.Show(
                ex.Message,
                _localizationService.GetLocalizedString("ProjectCard.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
        }
    }

    private List<string> ParseLowRiseBlocks() {
        return (LowRiseBlocks ?? "")
            .Split(',', ';')
            .Select(block => block.Trim())
            .Where(block => block.Length > 0)
            .ToList();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrWhiteSpace(Name)) {
            ErrorText = _localizationService.GetLocalizedString("ProjectCard.NoName");
            return false;
        }

        bool isDate = DateTime.TryParseExact(
            (AssignmentDate ?? "").Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
        if(!isDate) {
            ErrorText = _localizationService.GetLocalizedString("ProjectCard.BadDate");
            return false;
        }

        if(!int.TryParse((ClashVolumeDecimals ?? "").Trim(), out int decimals) || decimals < 0 || decimals > 6) {
            ErrorText = _localizationService.GetLocalizedString("ProjectCard.BadDecimals");
            return false;
        }

        if(Storeys == StoreysOption.Mixed && ParseLowRiseBlocks().Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("ProjectCard.NoLowRiseBlocks");
            return false;
        }

        ReinforcementRowViewModel badRow = Rows.FirstOrDefault(row => row.ToTableRow() == null);
        if(badRow != null) {
            string group = badRow.IsParking
                ? _localizationService.GetLocalizedString("ProjectCard.ParkingGroup")
                : _localizationService.GetLocalizedString("ProjectCard.ResidentialGroup");
            ErrorText = string.Format(
                _localizationService.GetLocalizedString("ProjectCard.BadValue"),
                group,
                badRow.Number);
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Нормы, которые не заданы для выбранной этажности: армирование этих конструкций не будет проверяться.
    /// </summary>
    private List<string> FindEmptyNorms(List<ReinforcementTableRow> table) {
        bool isMixed = Storeys == StoreysOption.Mixed;
        bool[] columns = isMixed ? new[] { true, false } : new[] { Storeys == StoreysOption.UpTo9 };
        var emptyNorms = new List<string>();
        foreach(bool isLowRise in columns) {
            foreach(string[] group in _residentialGroups) {
                var rows = group.Select(key => table.First(row => row.Key == key));
                bool hasValue = rows.Any(row => (isLowRise ? row.FirstValue : row.SecondValue).HasValue);
                if(!hasValue) {
                    string column = isMixed ? (isLowRise ? " (7–9 эт.)" : " (10–17 эт.)") : "";
                    emptyNorms.Add("корпуса п." + string.Join("/", group.Select(key => key.Substring(1))) + column);
                }
            }
        }

        emptyNorms.AddRange(table
            .Where(row => row.IsParking && !row.FirstValue.HasValue)
            .Select(row => "паркинг п." + row.Number));
        return emptyNorms;
    }
}

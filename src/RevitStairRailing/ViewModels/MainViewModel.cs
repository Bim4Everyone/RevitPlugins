using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitStairRailing.Models;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Главное окно: состав и размеры ограждения, сторона и отступ от траектории, сборка.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private FramingTypeViewModel _postType;
    private FramingTypeViewModel _handrailType;
    private FramingTypeViewModel _rodType;
    private FramingTypeViewModel _plateType;
    private FramingTypeViewModel _anchorType;
    private bool _hasPlates;
    private bool _hasAnchors;
    private string _postHeight;
    private string _maxPostStep;
    private string _postRotation;
    private string _rodOffsets;
    private string _plateLength;
    private string _anchorLength;
    private bool _isLeftSide;
    private string _edgeOffset;
    private bool _createAssembly;
    private string _assemblyNamePrefix;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек: окно закрывается, затем выбираются линии и строится ограждение.
    /// </summary>
    public ICommand AcceptViewCommand { get; }

    /// <summary>
    /// Параметры ограждения после нажатия «Построить».
    /// </summary>
    public RailingParameters Parameters { get; private set; }

    /// <summary>
    /// Схема пролёта ограждения.
    /// </summary>
    public RailingPreviewViewModel Preview { get; } = new();

    public double PreviewWidth => RailingPreviewViewModel.CanvasWidth;

    public double PreviewHeight => RailingPreviewViewModel.CanvasHeight;

    /// <summary>
    /// Типоразмеры «Каркаса несущего», размещаемые по линии.
    /// </summary>
    public List<FramingTypeViewModel> FramingTypes { get; private set; } = new();

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public FramingTypeViewModel PostType {
        get => _postType;
        set => RaiseAndSetIfChanged(ref _postType, value);
    }

    public FramingTypeViewModel HandrailType {
        get => _handrailType;
        set => RaiseAndSetIfChanged(ref _handrailType, value);
    }

    public FramingTypeViewModel RodType {
        get => _rodType;
        set => RaiseAndSetIfChanged(ref _rodType, value);
    }

    public FramingTypeViewModel PlateType {
        get => _plateType;
        set => RaiseAndSetIfChanged(ref _plateType, value);
    }

    public FramingTypeViewModel AnchorType {
        get => _anchorType;
        set => RaiseAndSetIfChanged(ref _anchorType, value);
    }

    public bool HasPlates {
        get => _hasPlates;
        set => SetAndUpdatePreview(ref _hasPlates, value);
    }

    public bool HasAnchors {
        get => _hasAnchors;
        set => SetAndUpdatePreview(ref _hasAnchors, value);
    }

    /// <summary>
    /// Высота стоек от траектории до оси поручня, мм.
    /// </summary>
    public string PostHeight {
        get => _postHeight;
        set => SetAndUpdatePreview(ref _postHeight, value);
    }

    /// <summary>
    /// Наибольший шаг стоек в плане, мм.
    /// </summary>
    public string MaxPostStep {
        get => _maxPostStep;
        set => SetAndUpdatePreview(ref _maxPostStep, value);
    }

    /// <summary>
    /// Угол поворота поперечного сечения стоек, градусы.
    /// </summary>
    public string PostRotation {
        get => _postRotation;
        set => RaiseAndSetIfChanged(ref _postRotation, value);
    }

    /// <summary>
    /// Расстояния от оси поручня вниз до прутков, мм, через «;».
    /// </summary>
    public string RodOffsets {
        get => _rodOffsets;
        set => SetAndUpdatePreview(ref _rodOffsets, value);
    }

    public string PlateLength {
        get => _plateLength;
        set => SetAndUpdatePreview(ref _plateLength, value);
    }

    public string AnchorLength {
        get => _anchorLength;
        set => SetAndUpdatePreview(ref _anchorLength, value);
    }

    /// <summary>
    /// Ограждение слева от линий по ходу подъёма; иначе — справа.
    /// </summary>
    public bool IsLeftSide {
        get => _isLeftSide;
        set {
            RaiseAndSetIfChanged(ref _isLeftSide, value);
            OnPropertyChanged(nameof(IsRightSide));
        }
    }

    public bool IsRightSide {
        get => !_isLeftSide;
        set => IsLeftSide = !value;
    }

    /// <summary>
    /// Отступ оси ограждения от линий траектории (края марша) в плане, мм.
    /// </summary>
    public string EdgeOffset {
        get => _edgeOffset;
        set => RaiseAndSetIfChanged(ref _edgeOffset, value);
    }

    public bool CreateAssembly {
        get => _createAssembly;
        set => RaiseAndSetIfChanged(ref _createAssembly, value);
    }

    public string AssemblyNamePrefix {
        get => _assemblyNamePrefix;
        set => RaiseAndSetIfChanged(ref _assemblyNamePrefix, value);
    }

    private void LoadView() {
        FramingTypes = _revitRepository.GetFramingTypes().Select(symbol => new FramingTypeViewModel(symbol)).ToList();
        OnPropertyChanged(nameof(FramingTypes));
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
        Parameters = new RailingParameters {
            PostTypeId = PostType.Id,
            HandrailTypeId = HandrailType.Id,
            RodTypeId = RodType?.Id,
            PlateTypeId = HasPlates ? PlateType.Id : null,
            AnchorTypeId = HasAnchors ? AnchorType.Id : null,
            PostHeight = ParseNumber(PostHeight).Value,
            MaxPostStep = ParseNumber(MaxPostStep).Value,
            PostRotationDegrees = ParseNumber(PostRotation).Value,
            RodOffsets = ParseRodOffsets(RodOffsets),
            PlateLength = HasPlates ? ParseNumber(PlateLength).Value : 0,
            AnchorLength = HasAnchors ? ParseNumber(AnchorLength).Value : 0,
            Side = IsLeftSide ? RailingSide.Left : RailingSide.Right,
            EdgeOffset = ParseNumber(EdgeOffset).Value,
            CreateAssembly = CreateAssembly,
            AssemblyNamePrefix = (AssemblyNamePrefix ?? "").Trim()
        };
    }

    /// <summary>
    /// Проверка ввода в порядке полей окна сверху вниз.
    /// </summary>
    private bool CanAcceptView() {
        double? postHeight = ParseNumber(PostHeight);
        List<double> rodOffsets = ParseRodOffsets(RodOffsets);
        string errorKey = PostType == null ? "MainWindow.NoPostType"
            : !(postHeight > 0) ? "MainWindow.BadPostHeight"
            : !(ParseNumber(MaxPostStep) > 0) ? "MainWindow.BadPostStep"
            : ParseNumber(PostRotation) == null ? "MainWindow.BadPostRotation"
            : HandrailType == null ? "MainWindow.NoHandrailType"
            : rodOffsets == null || rodOffsets.Any(offset => offset <= 0 || offset >= postHeight)
                ? "MainWindow.BadRodOffsets"
            : rodOffsets.Count > 0 && RodType == null ? "MainWindow.NoRodType"
            : HasPlates && PlateType == null ? "MainWindow.NoPlateType"
            : HasPlates && !(ParseNumber(PlateLength) > 0) ? "MainWindow.BadPlateLength"
            : HasAnchors && AnchorType == null ? "MainWindow.NoAnchorType"
            : HasAnchors && !(ParseNumber(AnchorLength) > 0) ? "MainWindow.BadAnchorLength"
            : !(ParseNumber(EdgeOffset) >= 0) ? "MainWindow.BadEdgeOffset"
            : CreateAssembly && string.IsNullOrWhiteSpace(AssemblyNamePrefix) ? "MainWindow.NoAssemblyName"
            : null;
        ErrorText = errorKey != null ? _localizationService.GetLocalizedString(errorKey) : null;
        return errorKey == null;
    }

    private void LoadConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document) ?? new RevitSettings();

        PostType = FindType(settings.PostType, "Труба_ГОСТ 32931", "30х20х3");
        HandrailType = FindType(settings.HandrailType, "87341", "40х3");
        RodType = FindType(settings.RodType, "Стержень арматурный", "12 А240");
        PlateType = FindType(settings.PlateType, "Пластина_ГОСТ 19903", "-4х40");
        AnchorType = FindType(settings.AnchorType, "Клиновой анкер", "");
        HasPlates = settings.HasPlates;
        HasAnchors = settings.HasAnchors;
        PostHeight = settings.PostHeight;
        MaxPostStep = settings.MaxPostStep;
        PostRotation = settings.PostRotation;
        RodOffsets = settings.RodOffsets;
        PlateLength = settings.PlateLength;
        AnchorLength = settings.AnchorLength;
        IsLeftSide = settings.Side == RailingSide.Left;
        EdgeOffset = settings.EdgeOffset;
        CreateAssembly = settings.CreateAssembly;
        AssemblyNamePrefix = settings.AssemblyNamePrefix;
    }

    private void SaveConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document)
                                 ?? _pluginConfig.AddSettings(_revitRepository.Document);

        settings.PostType = PostType?.Name;
        settings.HandrailType = HandrailType?.Name;
        settings.RodType = RodType?.Name;
        settings.PlateType = PlateType?.Name;
        settings.AnchorType = AnchorType?.Name;
        settings.HasPlates = HasPlates;
        settings.HasAnchors = HasAnchors;
        settings.PostHeight = PostHeight;
        settings.MaxPostStep = MaxPostStep;
        settings.PostRotation = PostRotation;
        settings.RodOffsets = RodOffsets;
        settings.PlateLength = PlateLength;
        settings.AnchorLength = AnchorLength;
        settings.Side = IsLeftSide ? RailingSide.Left : RailingSide.Right;
        settings.EdgeOffset = EdgeOffset;
        settings.CreateAssembly = CreateAssembly;
        settings.AssemblyNamePrefix = AssemblyNamePrefix;
        _pluginConfig.SaveProjectConfig();
    }

    /// <summary>
    /// Типоразмер из настроек; если его нет — типовой из шаблона КМ по частям имён семейства и типа.
    /// </summary>
    private FramingTypeViewModel FindType(string savedName, string familyNamePart, string typeNamePart) {
        return FramingTypes.FirstOrDefault(type => type.Name == savedName)
               ?? FramingTypes.FirstOrDefault(type => type.FamilyName.Contains(familyNamePart)
                                                      && type.TypeName.Contains(typeNamePart));
    }

    private void SetAndUpdatePreview<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
        RaiseAndSetIfChanged(ref field, value, propertyName);
        UpdatePreview();
    }

    private void UpdatePreview() {
        Preview.Update(
            ParseNumber(PostHeight) ?? 0,
            ParseNumber(MaxPostStep) ?? 0,
            ParseRodOffsets(RodOffsets) ?? new List<double>(),
            HasPlates ? ParseNumber(PlateLength) : null,
            HasAnchors ? ParseNumber(AnchorLength) : null);
    }

    /// <summary>
    /// Число из поля ввода (точка или запятая); null — не число.
    /// </summary>
    private static double? ParseNumber(string text) {
        string normalized = (text ?? "").Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;
    }

    /// <summary>
    /// Расстояния до прутков через «;» по возрастанию; пусто — прутков нет; null — есть не число.
    /// </summary>
    private static List<double> ParseRodOffsets(string text) {
        var offsets = new List<double>();
        foreach(string part in (text ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
            if(string.IsNullOrWhiteSpace(part)) {
                continue;
            }

            double? offset = ParseNumber(part);
            if(offset == null) {
                return null;
            }

            offsets.Add(offset.Value);
        }

        return offsets.OrderBy(offset => offset).ToList();
    }
}

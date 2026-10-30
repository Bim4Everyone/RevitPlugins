using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitStairRailing.Models;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Главное окно: состав и размеры ограждения, крепление стоек, сторона и отступ от траектории, сборка.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string? _errorText;
    private FramingTypeViewModel? _postType;
    private FramingTypeViewModel? _handrailType;
    private FramingTypeViewModel? _rodType;
    private string _postHeight = string.Empty;
    private string _maxPostStep = string.Empty;
    private string _postRotation = string.Empty;
    private string _rodOffsets = string.Empty;
    private PostMount _flightMount;
    private PostMount _landingMount;
    private PlateFamilyViewModel? _plateFamily;
    private string _basePlateThickness = string.Empty;
    private string _basePlateLength = string.Empty;
    private string _basePlateWidth = string.Empty;
    private bool _hasAnchors;
    private FramingTypeViewModel? _anchorType;
    private AnchorLayout _anchorLayout;
    private string _anchorEdgeDistance = string.Empty;
    private string _cheekThickness = string.Empty;
    private string _cheekWidth = string.Empty;
    private string _cheekHeight = string.Empty;
    private string _cheekGap = string.Empty;
    private CheekOrientation _cheekOrientation;
    private bool _isLeftSide;
    private string _flightEdgeOffset = string.Empty;
    private string _landingEdgeOffset = string.Empty;
    private bool _createAssembly;
    private string _assemblyNamePrefix = string.Empty;
    private string? _activeField;

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

        MountOptions = new List<OptionViewModel<PostMount>> {
            new(PostMount.Top, GetText("Mount.Top")),
            new(PostMount.Side, GetText("Mount.Side"))
        };
        AnchorLayoutOptions = new List<OptionViewModel<AnchorLayout>> {
            new(AnchorLayout.Triangle, GetText("AnchorLayout.Triangle")),
            new(AnchorLayout.Corners, GetText("AnchorLayout.Corners")),
            new(AnchorLayout.Diagonal, GetText("AnchorLayout.Diagonal"))
        };
        CheekOrientationOptions = new List<OptionViewModel<CheekOrientation>> {
            new(CheekOrientation.AlongRailing, GetText("CheekOrientation.Along")),
            new(CheekOrientation.AcrossRailing, GetText("CheekOrientation.Across"))
        };

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
    public RailingParameters? Parameters { get; private set; }

    /// <summary>
    /// Схема ограждения: участок марша и участок площадки.
    /// </summary>
    public RailingPreviewViewModel Preview { get; } = new();

    public double PreviewWidth => RailingPreviewViewModel.CanvasWidth;

    public double PreviewHeight => RailingPreviewViewModel.CanvasHeight;

    /// <summary>
    /// Типоразмеры «Каркаса несущего», размещаемые по линии.
    /// </summary>
    public List<FramingTypeViewModel> FramingTypes { get; private set; } = new();

    /// <summary>
    /// Типоразмеры «Каркаса несущего», размещаемые в точке, — анкеры.
    /// </summary>
    public List<FramingTypeViewModel> AnchorTypes { get; private set; } = new();

    public List<PlateFamilyViewModel> PlateFamilies { get; private set; } = new();

    public List<OptionViewModel<PostMount>> MountOptions { get; }

    public List<OptionViewModel<AnchorLayout>> AnchorLayoutOptions { get; }

    public List<OptionViewModel<CheekOrientation>> CheekOrientationOptions { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string? ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public FramingTypeViewModel? PostType {
        get => _postType;
        set => RaiseAndSetIfChanged(ref _postType, value);
    }

    public FramingTypeViewModel? HandrailType {
        get => _handrailType;
        set => RaiseAndSetIfChanged(ref _handrailType, value);
    }

    public FramingTypeViewModel? RodType {
        get => _rodType;
        set => RaiseAndSetIfChanged(ref _rodType, value);
    }

    /// <summary>
    /// Высота от траектории до оси поручня, мм.
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
    /// Угол поворота поперечного сечения стоек относительно направления ограждения, градусы.
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

    public PostMount FlightMount {
        get => _flightMount;
        set {
            SetAndUpdatePreview(ref _flightMount, value);
            OnMountChanged();
        }
    }

    public PostMount LandingMount {
        get => _landingMount;
        set {
            SetAndUpdatePreview(ref _landingMount, value);
            OnMountChanged();
        }
    }

    /// <summary>
    /// Часть схемы поля окна, в котором сейчас ввод (<see cref="PreviewFields"/>); она выделяется на схеме.
    /// </summary>
    public string? ActiveField {
        get => _activeField;
        set => SetAndUpdatePreview(ref _activeField, value);
    }

    /// <summary>
    /// Хотя бы на одном виде участков стойки крепятся сверху — нужны опорные пластины.
    /// </summary>
    public bool UsesTopMount => FlightMount == PostMount.Top || LandingMount == PostMount.Top;

    /// <summary>
    /// Хотя бы на одном виде участков стойки крепятся сбоку — нужны щёки.
    /// </summary>
    public bool UsesSideMount => FlightMount == PostMount.Side || LandingMount == PostMount.Side;

    public PlateFamilyViewModel? PlateFamily {
        get => _plateFamily;
        set => RaiseAndSetIfChanged(ref _plateFamily, value);
    }

    public string BasePlateThickness {
        get => _basePlateThickness;
        set => SetAndUpdatePreview(ref _basePlateThickness, value);
    }

    /// <summary>
    /// Длина опорной пластины вдоль ограждения, мм.
    /// </summary>
    public string BasePlateLength {
        get => _basePlateLength;
        set => SetAndUpdatePreview(ref _basePlateLength, value);
    }

    /// <summary>
    /// Ширина опорной пластины поперёк ограждения, мм.
    /// </summary>
    public string BasePlateWidth {
        get => _basePlateWidth;
        set => SetAndUpdatePreview(ref _basePlateWidth, value);
    }

    public bool HasAnchors {
        get => _hasAnchors;
        set => SetAndUpdatePreview(ref _hasAnchors, value);
    }

    public FramingTypeViewModel? AnchorType {
        get => _anchorType;
        set => RaiseAndSetIfChanged(ref _anchorType, value);
    }

    public AnchorLayout AnchorLayout {
        get => _anchorLayout;
        set => SetAndUpdatePreview(ref _anchorLayout, value);
    }

    /// <summary>
    /// Расстояние от оси анкера до края опорной пластины, мм.
    /// </summary>
    public string AnchorEdgeDistance {
        get => _anchorEdgeDistance;
        set => SetAndUpdatePreview(ref _anchorEdgeDistance, value);
    }

    public string CheekThickness {
        get => _cheekThickness;
        set => SetAndUpdatePreview(ref _cheekThickness, value);
    }

    public string CheekWidth {
        get => _cheekWidth;
        set => SetAndUpdatePreview(ref _cheekWidth, value);
    }

    public string CheekHeight {
        get => _cheekHeight;
        set => SetAndUpdatePreview(ref _cheekHeight, value);
    }

    /// <summary>
    /// Зазор между щёками — размер стойки между ними, мм.
    /// </summary>
    public string CheekGap {
        get => _cheekGap;
        set => SetAndUpdatePreview(ref _cheekGap, value);
    }

    public CheekOrientation CheekOrientation {
        get => _cheekOrientation;
        set => SetAndUpdatePreview(ref _cheekOrientation, value);
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
    /// Отступ наружного края опоры от края марша на маршах, мм.
    /// </summary>
    public string FlightEdgeOffset {
        get => _flightEdgeOffset;
        set => RaiseAndSetIfChanged(ref _flightEdgeOffset, value);
    }

    /// <summary>
    /// Отступ наружного края опоры от края площадки на площадках, мм.
    /// </summary>
    public string LandingEdgeOffset {
        get => _landingEdgeOffset;
        set => RaiseAndSetIfChanged(ref _landingEdgeOffset, value);
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
        AnchorTypes = _revitRepository.GetPointFramingTypes().Select(symbol => new FramingTypeViewModel(symbol)).ToList();
        PlateFamilies = _revitRepository.GetPlateFamilies().Select(family => new PlateFamilyViewModel(family)).ToList();
        OnPropertyChanged(nameof(FramingTypes));
        OnPropertyChanged(nameof(AnchorTypes));
        OnPropertyChanged(nameof(PlateFamilies));
        LoadConfig();
    }

    private void AcceptView() {
        // команда доступна только после проверки ввода (CanAcceptView): типы выбраны, числа разобраны
        if(PostType is null || HandrailType is null || PlateFamily is null) {
            return;
        }

        SaveConfig();
        TryParseRodOffsets(RodOffsets, out List<double> rodOffsets);
        Parameters = new RailingParameters {
            PostTypeId = PostType.Id,
            HandrailTypeId = HandrailType.Id,
            RodTypeId = rodOffsets.Count > 0 ? RodType?.Id : null,
            PostHeight = ParseValidNumber(PostHeight),
            MaxPostStep = ParseValidNumber(MaxPostStep),
            PostRotationDegrees = ParseValidNumber(PostRotation),
            RodOffsets = rodOffsets,
            FlightMount = FlightMount,
            LandingMount = LandingMount,
            PlateFamilyId = PlateFamily.Id,
            BasePlate = UsesTopMount
                ? new PlateSize(ParseValidNumber(BasePlateThickness), ParseValidNumber(BasePlateWidth))
                : new PlateSize(0, 0),
            BasePlateLength = UsesTopMount ? ParseValidNumber(BasePlateLength) : 0,
            AnchorTypeId = UsesTopMount && HasAnchors ? AnchorType?.Id : null,
            AnchorLayout = AnchorLayout,
            AnchorEdgeDistance = UsesTopMount && HasAnchors ? ParseValidNumber(AnchorEdgeDistance) : 0,
            Cheek = UsesSideMount
                ? new PlateSize(ParseValidNumber(CheekThickness), ParseValidNumber(CheekWidth))
                : new PlateSize(0, 0),
            CheekHeight = UsesSideMount ? ParseValidNumber(CheekHeight) : 0,
            CheekGap = UsesSideMount ? ParseValidNumber(CheekGap) : 0,
            CheekOrientation = CheekOrientation,
            Side = IsLeftSide ? RailingSide.Left : RailingSide.Right,
            FlightEdgeOffset = ParseValidNumber(FlightEdgeOffset),
            LandingEdgeOffset = ParseValidNumber(LandingEdgeOffset),
            CreateAssembly = CreateAssembly,
            AssemblyNamePrefix = AssemblyNamePrefix.Trim()
        };
    }

    /// <summary>
    /// Проверка ввода в порядке полей окна сверху вниз.
    /// </summary>
    private bool CanAcceptView() {
        string? errorKey = GetInputErrorKey();
        ErrorText = errorKey != null ? GetText(errorKey) : null;
        return errorKey == null;
    }

    private string? GetInputErrorKey() {
        double? postHeight = ParseNumber(PostHeight);
        bool areRodOffsetsNumbers = TryParseRodOffsets(RodOffsets, out List<double> rodOffsets);
        if(PostType == null) {
            return "MainWindow.NoPostType";
        }

        if(!(postHeight > 0)) {
            return "MainWindow.BadPostHeight";
        }

        if(!(ParseNumber(MaxPostStep) > 0)) {
            return "MainWindow.BadPostStep";
        }

        if(ParseNumber(PostRotation) == null) {
            return "MainWindow.BadPostRotation";
        }

        if(HandrailType == null) {
            return "MainWindow.NoHandrailType";
        }

        if(!areRodOffsetsNumbers || rodOffsets.Any(offset => offset <= 0 || offset >= postHeight)) {
            return "MainWindow.BadRodOffsets";
        }

        if(rodOffsets.Count > 0 && RodType == null) {
            return "MainWindow.NoRodType";
        }

        if(PlateFamily == null) {
            return "MainWindow.NoPlateFamily";
        }

        if(UsesTopMount) {
            double? plateLength = ParseNumber(BasePlateLength);
            double? plateWidth = ParseNumber(BasePlateWidth);
            if(!(ParseNumber(BasePlateThickness) > 0) || !(plateLength > 0) || !(plateWidth > 0)) {
                return "MainWindow.BadBasePlate";
            }

            if(HasAnchors && AnchorType == null) {
                return "MainWindow.NoAnchorType";
            }

            double? edgeDistance = ParseNumber(AnchorEdgeDistance);
            if(HasAnchors && !(edgeDistance >= 0 && edgeDistance <= Math.Min(plateLength.Value, plateWidth.Value) / 2)) {
                return "MainWindow.BadAnchorEdge";
            }
        }

        if(UsesSideMount
           && (!(ParseNumber(CheekThickness) > 0) || !(ParseNumber(CheekWidth) > 0)
               || !(ParseNumber(CheekHeight) > 0) || !(ParseNumber(CheekGap) > 0))) {
            return "MainWindow.BadCheeks";
        }

        // отрицательный отступ (опора за краем) — только у крепления сверху на пластине
        if(!IsEdgeOffsetValid(FlightEdgeOffset, FlightMount) || !IsEdgeOffsetValid(LandingEdgeOffset, LandingMount)) {
            return "MainWindow.BadEdgeOffset";
        }

        return CreateAssembly && string.IsNullOrWhiteSpace(AssemblyNamePrefix) ? "MainWindow.NoAssemblyName" : null;
    }

    private void LoadConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document) ?? new RevitSettings();

        // типы по умолчанию — фрагменты имён семейств и типоразмеров шаблона КМ (данные модели, не текст
        // интерфейса); если таких нет, берётся первый подходящий тип
        PostType = FindType(FramingTypes, settings.PostType, "Труба_ГОСТ 32931", "30х20х3");
        HandrailType = FindType(FramingTypes, settings.HandrailType, "87341", "40х3");
        RodType = FindType(FramingTypes, settings.RodType, "Стержень арматурный", "12 А240");
        AnchorType = FindType(AnchorTypes, settings.AnchorType, "анкер", "");
        PlateFamily = PlateFamilies.FirstOrDefault(family => family.Name == settings.PlateFamily)
                      ?? PlateFamilies.FirstOrDefault(family => family.Name.Contains("Пластина_ГОСТ 19903"))
                      ?? PlateFamilies.FirstOrDefault();
        PostHeight = settings.PostHeight;
        MaxPostStep = settings.MaxPostStep;
        PostRotation = settings.PostRotation;
        RodOffsets = settings.RodOffsets;
        FlightMount = settings.FlightMount;
        LandingMount = settings.LandingMount;
        BasePlateThickness = settings.BasePlateThickness;
        BasePlateLength = settings.BasePlateLength;
        BasePlateWidth = settings.BasePlateWidth;
        HasAnchors = settings.HasAnchors;
        AnchorLayout = settings.AnchorLayout;
        AnchorEdgeDistance = settings.AnchorEdgeDistance;
        CheekThickness = settings.CheekThickness;
        CheekWidth = settings.CheekWidth;
        CheekHeight = settings.CheekHeight;
        CheekGap = settings.CheekGap;
        CheekOrientation = settings.CheekOrientation;
        IsLeftSide = settings.Side == RailingSide.Left;
        FlightEdgeOffset = settings.FlightEdgeOffset;
        LandingEdgeOffset = settings.LandingEdgeOffset;
        CreateAssembly = settings.CreateAssembly;
        AssemblyNamePrefix = settings.AssemblyNamePrefix;
    }

    private void SaveConfig() {
        RevitSettings settings = _pluginConfig.GetSettings(_revitRepository.Document)
                                 ?? _pluginConfig.AddSettings(_revitRepository.Document);

        settings.PostType = PostType?.Name;
        settings.HandrailType = HandrailType?.Name;
        settings.RodType = RodType?.Name;
        settings.AnchorType = AnchorType?.Name;
        settings.PlateFamily = PlateFamily?.Name;
        settings.PostHeight = PostHeight;
        settings.MaxPostStep = MaxPostStep;
        settings.PostRotation = PostRotation;
        settings.RodOffsets = RodOffsets;
        settings.FlightMount = FlightMount;
        settings.LandingMount = LandingMount;
        settings.BasePlateThickness = BasePlateThickness;
        settings.BasePlateLength = BasePlateLength;
        settings.BasePlateWidth = BasePlateWidth;
        settings.HasAnchors = HasAnchors;
        settings.AnchorLayout = AnchorLayout;
        settings.AnchorEdgeDistance = AnchorEdgeDistance;
        settings.CheekThickness = CheekThickness;
        settings.CheekWidth = CheekWidth;
        settings.CheekHeight = CheekHeight;
        settings.CheekGap = CheekGap;
        settings.CheekOrientation = CheekOrientation;
        settings.Side = IsLeftSide ? RailingSide.Left : RailingSide.Right;
        settings.FlightEdgeOffset = FlightEdgeOffset;
        settings.LandingEdgeOffset = LandingEdgeOffset;
        settings.CreateAssembly = CreateAssembly;
        settings.AssemblyNamePrefix = AssemblyNamePrefix;
        _pluginConfig.SaveProjectConfig();
    }

    /// <summary>
    /// Типоразмер из настроек; если его нет — типовой из шаблона КМ по частям имён семейства и типа.
    /// </summary>
    private static FramingTypeViewModel? FindType(
        List<FramingTypeViewModel> types,
        string? savedName,
        string familyNamePart,
        string typeNamePart) {

        return types.FirstOrDefault(type => type.Name == savedName)
               ?? types.FirstOrDefault(type => type.FamilyName.Contains(familyNamePart)
                                               && type.TypeName.Contains(typeNamePart));
    }

    private static bool IsEdgeOffsetValid(string text, PostMount mount) {
        double? offset = ParseNumber(text);
        return offset != null && (offset >= 0 || mount == PostMount.Top);
    }

    private void OnMountChanged() {
        OnPropertyChanged(nameof(UsesTopMount));
        OnPropertyChanged(nameof(UsesSideMount));
    }

    private void SetAndUpdatePreview<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        RaiseAndSetIfChanged(ref field, value, propertyName);
        UpdatePreview();
    }

    private void UpdatePreview() {
        Preview.Update(new RailingPreviewSizes {
            PostHeight = ParseNumber(PostHeight),
            PostStep = ParseNumber(MaxPostStep),
            RodOffsets = TryParseRodOffsets(RodOffsets, out List<double> rodOffsets) ? rodOffsets : new List<double>(),
            FlightMount = FlightMount,
            LandingMount = LandingMount,
            ActiveField = ActiveField,
            PlateThickness = ParseNumber(BasePlateThickness),
            PlateLength = ParseNumber(BasePlateLength),
            PlateWidth = ParseNumber(BasePlateWidth),
            HasAnchors = HasAnchors,
            AnchorLayout = AnchorLayout,
            AnchorEdgeDistance = ParseNumber(AnchorEdgeDistance),
            CheekThickness = ParseNumber(CheekThickness),
            CheekWidth = ParseNumber(CheekWidth),
            CheekHeight = ParseNumber(CheekHeight),
            CheekGap = ParseNumber(CheekGap),
            CheekOrientation = CheekOrientation
        });
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }

    /// <summary>
    /// Число из поля, уже прошедшего проверку ввода.
    /// </summary>
    private static double ParseValidNumber(string text) {
        return ParseNumber(text) ?? 0;
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
    /// Расстояния до прутков через «;» по возрастанию; пусто — прутков нет.
    /// </summary>
    /// <returns>false — среди значений есть не число (тогда список — только разобранные числа).</returns>
    private static bool TryParseRodOffsets(string text, out List<double> offsets) {
        offsets = new List<double>();
        bool isValid = true;
        foreach(string part in (text ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
            if(string.IsNullOrWhiteSpace(part)) {
                continue;
            }

            double? offset = ParseNumber(part);
            if(offset == null) {
                isValid = false;
                continue;
            }

            offsets.Add(offset.Value);
        }

        offsets.Sort();
        return isValid;
    }
}

namespace RevitCheckingLevels.Models.LevelParser;
/// <summary>
/// Префиксы блоков здания.
/// </summary>
internal class BlockType : PrefixName {
    /// <summary>
    /// Конструирует объект блока здания.
    /// </summary>
    /// <param name="name">Наименование типа уровня.</param>
    protected BlockType(string name)
        : base(name) {

    }

    /// <summary>
    /// Секция.
    /// </summary>
    public static readonly BlockType Section = new("С");

    /// <summary>
    /// Корпус.
    /// </summary>
    public static readonly BlockType Body = new("К");

    /// <summary>
    /// Парковка.
    /// </summary>
    public static readonly BlockType Parking = new("ПРК");

    /// <summary>
    /// Пристройка.
    /// </summary>
    public static readonly BlockType Outbuilding = new("ПРС");

    /// <summary>
    /// Дошкольная образовательная организация.
    /// </summary>
    public static readonly BlockType Kindergarten = new("ДОО");

    /// <summary>
    /// Школа
    /// </summary>
    public static readonly BlockType School = new("Ш");

    /// <summary>
    /// Образовательный комплекс.
    /// </summary>
    public static readonly BlockType EducationalComplex = new("ОК");

    /// <summary>
    /// Храм.
    /// </summary>
    public static readonly BlockType Temple = new("ХРМ");

    /// <summary>
    /// Физкультурно-оздоровительный комплекс.
    /// </summary>
    public static readonly BlockType SportsComplex = new("ФОК");

    /// <summary>
    /// Бизнес-центр.
    /// </summary>
    public static readonly BlockType BusinessCenter = new("БЦ");

    /// <summary>
    /// Музей.
    /// </summary>
    public static readonly BlockType Museum = new("МУЗ");

    /// <summary>
    /// Многофункциональный центр.
    /// </summary>
    public static readonly BlockType MultifunctionalCenter = new("МФЦ");
}
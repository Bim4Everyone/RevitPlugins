namespace RevitCheckingLevels.Models.LevelParser {
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
        public static readonly BlockType Section = new BlockType("С");

        /// <summary>
        /// Корпус.
        /// </summary>
        public static readonly BlockType Body = new BlockType("К");

        /// <summary>
        /// Парковка.
        /// </summary>
        public static readonly BlockType Parking = new BlockType("ПРК");

        /// <summary>
        /// Пристройка.
        /// </summary>
        public static readonly BlockType Outbuilding = new BlockType("ПРС");

        /// <summary>
        /// Дошкольная образовательная организация.
        /// </summary>
        public static readonly BlockType Kindergarten = new BlockType("ДОО");

        /// <summary>
        /// Школа
        /// </summary>
        public static readonly BlockType School = new BlockType("Ш");

        /// <summary>
        /// Образовательный комплекс.
        /// </summary>
        public static readonly BlockType EducationalComplex = new BlockType("ОК");

        /// <summary>
        /// Храм.
        /// </summary>
        public static readonly BlockType Temple = new BlockType("ХРМ");

        /// <summary>
        /// Физкультурно-оздоровительный комплекс.
        /// </summary>
        public static readonly BlockType SportsComplex = new BlockType("ФОК");

        /// <summary>
        /// Бизнес-центр.
        /// </summary>
        public static readonly BlockType BusinessCenter = new BlockType("БЦ");

        /// <summary>
        /// Музей.
        /// </summary>
        public static readonly BlockType Museum = new BlockType("МУЗ");

        /// <summary>
        /// Многофункциональный центр.
        /// </summary>
        public static readonly BlockType MultifunctionalCenter = new BlockType("МФЦ");
    }
}
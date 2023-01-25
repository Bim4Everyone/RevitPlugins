namespace RevitCheckingLevels.Models.LevelParser {
    /// <summary>
    /// Префиксы типа уровня.
    /// </summary>
    internal class LevelType : PrefixName {
        /// <summary>
        /// Конструирует объект типа уровня.
        /// </summary>
        /// <param name="name">Наименование типа уровня.</param>
        protected LevelType(string name)
            : base(name) {

        }

        /// <summary>
        /// Кровля. Префикс "К".
        /// </summary>
        public static readonly LevelType TopLevel = new LevelType("К");

        /// <summary>
        /// Надземный этаж. Без префикса.
        /// </summary>
        public static readonly LevelType BasicLevel = new LevelType(string.Empty);

        /// <summary>
        /// Подземный этаж. Префикс "П".
        /// </summary>
        public static readonly LevelType UndergroundLevel = new LevelType("П");

        /// <summary>
        /// Диспетчерская. Префикс "Д".
        /// </summary>
        public static readonly LevelType ControlRoom = new LevelType("Д");

        /// <summary>
        /// Технический этаж. Префикс "Т".
        /// </summary>
        public static readonly LevelType TechnicalLevel = new LevelType("Т");

        /// <summary>
        /// Техподполье. Префикс "ТХ".
        /// </summary>
        public static readonly LevelType TechUnderground = new LevelType("ТХ");
    }
}
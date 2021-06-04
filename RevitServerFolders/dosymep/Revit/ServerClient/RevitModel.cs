namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Модель Revit (.rvt файл)
    /// </summary>
    public class RevitModel : RevitResponse {
        /// <summary>
        /// Размер модели (измеряется в байтах).
        /// </summary>
        public long ModelSize { get; set; }

        /// <summary>
        /// Размер вспомогательных данных (измеряется в байтах)
        /// </summary>
        public long SupportSize { get; set; }

        /// <summary>
        /// Наименование файла.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Номер версии файла Revit, в которую была внесена последняя модификация модели.
        /// </summary>
        public int ProductVersion { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
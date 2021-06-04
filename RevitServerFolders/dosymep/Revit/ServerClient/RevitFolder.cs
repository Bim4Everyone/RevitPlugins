namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Папка Revit.
    /// </summary>
    public class RevitFolder : RevitResponse {
        /// <summary>
        /// Размер папки (измеряется в байтах)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Наименование папки.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Признак содержания информации в папке.
        /// </summary>
        public bool HasContents { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
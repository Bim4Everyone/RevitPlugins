using System;

namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Информация об элементе.
    /// </summary>
    public class DirectoryInfo : RevitResponse {
        /// <summary>
        /// Полный путь до элемента.
        /// </summary>
        public string Path { get; set; }

        //public DateTime DateCreated { get; set; }
        //public DateTime DateModified { get; set; }

        /// <summary>
        /// Размер элемента (измеряется в байтах).
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Размер модели (измеряется в байтах).
        /// </summary>
        public long ModelSize { get; set; }

        /// <summary>
        /// Количество моделей в папке.
        /// </summary>
        public int ModelCount { get; set; }

        /// <summary>
        /// Количество папок в папке.
        /// </summary>
        public int FolderCount { get; set; }

        /// <summary>
        /// Признак существования папки.
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// Признак папки.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Имя пользователя последнего изменившего элемент.
        /// </summary>
        public string LastModifiedBy { get; set; }
    }
}
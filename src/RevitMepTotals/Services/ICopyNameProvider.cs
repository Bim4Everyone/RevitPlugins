namespace RevitMepTotals.Services {
    internal interface ICopyNameProvider {
        /// <summary>
        /// Возвращает название для копии объекта в формате "oldName - копия (1)"
        /// </summary>
        /// <param name="name">Название копируемого объекта</param>
        /// <param name="existingNames">Массив названий соседних сущностей</param>
        /// <returns>Название копии объекта</returns>
        string CreateCopyName(string name, string[] existingNames);
    }
}

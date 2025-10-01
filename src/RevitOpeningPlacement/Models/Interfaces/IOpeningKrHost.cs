namespace RevitOpeningPlacement.Models.Interfaces;
/// <summary>
/// Хост входящего задания на отверстие в КР или хост чистового отверстия КР - элемент из активного файла КР, 
/// в теле которого расположено входящее задание на отверстие из связанного файла с заданиями на отверстия.
/// </summary>
internal interface IOpeningKrHost : IOpeningHost {
    /// <summary>
    /// Значение параметра хоста "обр_ФОП_Раздел проекта"
    /// </summary>
    string KrModelPart { get; }
}

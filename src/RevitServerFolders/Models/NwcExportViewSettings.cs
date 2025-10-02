namespace RevitServerFolders.Models;
/// <summary>
/// Настройки вида для экспорта в NWC
/// </summary>
internal class NwcExportViewSettings {
    /// <summary>
    /// Путь к файлу, в котором находится шаблон вида для экспорта в NWC
    /// </summary>
    public string RvtFilePath { get; set; }

    /// <summary>
    /// Название шаблона вида, который надо применить к экспортируемому в NWC виду
    /// </summary>
    public string ViewTemplateName { get; set; }

    /// <summary>
    /// Рабочие наборы, в названиях которых содержится подстрока из коллекции, должны быть скрыты на экспортируемом виде
    /// </summary>
    public string[] WorksetHideTemplates { get; set; } = ["вспомогательные", "скрытые", "скрыть"];
}

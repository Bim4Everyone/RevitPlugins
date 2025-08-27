namespace RevitSleeves.Services.Core;
internal interface IDocumentChecker {
    /// <summary>
    /// Проверяет документ на возможность запуска плагина
    /// </summary>
    void CheckDocument();
}

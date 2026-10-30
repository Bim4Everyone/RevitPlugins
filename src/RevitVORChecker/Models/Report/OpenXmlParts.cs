namespace RevitVORChecker.Models.Report;

/// <summary>
/// Служебные части пакета .docx реестра: типы содержимого, связи, стили, настройки.
/// </summary>
internal static class OpenXmlParts {
    private const string _xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";

    private const string _relationshipsNamespace =
        "http://schemas.openxmlformats.org/package/2006/relationships";

    private const string _documentRelationshipType =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public static string ContentTypes =>
        _xmlHeader
        + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
        + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
        + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
        + "<Default Extension=\"png\" ContentType=\"image/png\"/>"
        + "<Override PartName=\"/word/document.xml\" "
        + "ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>"
        + "<Override PartName=\"/word/styles.xml\" "
        + "ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml\"/>"
        + "<Override PartName=\"/word/settings.xml\" "
        + "ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml\"/>"
        + "</Types>";

    public static string PackageRelations =>
        _xmlHeader
        + $"<Relationships xmlns=\"{_relationshipsNamespace}\">"
        + $"<Relationship Id=\"rId1\" Type=\"{_documentRelationshipType}/officeDocument\" Target=\"word/document.xml\"/>"
        + "</Relationships>";

    /// <summary>
    /// Стили: шрифт Calibri 12 пт без интервалов после абзаца.
    /// </summary>
    public static string Styles =>
        _xmlHeader
        + "<w:styles xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">"
        + "<w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii=\"Calibri\" w:eastAsia=\"Calibri\" w:hAnsi=\"Calibri\" "
        + "w:cs=\"Calibri\"/><w:sz w:val=\"24\"/><w:szCs w:val=\"24\"/><w:lang w:val=\"ru-RU\" w:eastAsia=\"ru-RU\"/>"
        + "</w:rPr></w:rPrDefault><w:pPrDefault><w:pPr><w:spacing w:after=\"0\" w:line=\"240\" w:lineRule=\"auto\"/>"
        + "</w:pPr></w:pPrDefault></w:docDefaults>"
        + "<w:style w:type=\"paragraph\" w:default=\"1\" w:styleId=\"Normal\"><w:name w:val=\"Normal\"/><w:qFormat/></w:style>"
        + "<w:style w:type=\"table\" w:default=\"1\" w:styleId=\"TableNormal\"><w:name w:val=\"Normal Table\"/>"
        + "<w:tblPr><w:tblInd w:w=\"0\" w:type=\"dxa\"/><w:tblCellMar><w:top w:w=\"0\" w:type=\"dxa\"/>"
        + "<w:left w:w=\"108\" w:type=\"dxa\"/><w:bottom w:w=\"0\" w:type=\"dxa\"/><w:right w:w=\"108\" w:type=\"dxa\"/>"
        + "</w:tblCellMar></w:tblPr></w:style>"
        + "</w:styles>";

    public static string Settings =>
        _xmlHeader
        + "<w:settings xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">"
        + "<w:defaultTabStop w:val=\"708\"/><w:characterSpacingControl w:val=\"doNotCompress\"/>"
        + "<w:compat><w:compatSetting w:name=\"compatibilityMode\" w:uri=\"http://schemas.microsoft.com/office/word\" "
        + "w:val=\"15\"/></w:compat></w:settings>";

    public static string DocumentRelations(IEnumerable<(string RelationId, string Target)> images) {
        string imageRelations = string.Concat(images.Select(image =>
            $"<Relationship Id=\"{image.RelationId}\" Type=\"{_documentRelationshipType}/image\" Target=\"{image.Target}\"/>"));
        return _xmlHeader
               + $"<Relationships xmlns=\"{_relationshipsNamespace}\">"
               + $"<Relationship Id=\"rIdStyles\" Type=\"{_documentRelationshipType}/styles\" Target=\"styles.xml\"/>"
               + $"<Relationship Id=\"rIdSettings\" Type=\"{_documentRelationshipType}/settings\" Target=\"settings.xml\"/>"
               + imageRelations
               + "</Relationships>";
    }

    /// <summary>
    /// Документ: альбомный A3, поля 12,7 мм.
    /// </summary>
    public static string WrapDocument(string body) {
        return _xmlHeader
               + "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\" "
               + "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" "
               + "xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\" "
               + "xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" "
               + "xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\">"
               + "<w:body>" + body
               + "<w:sectPr><w:pgSz w:w=\"23811\" w:h=\"16838\" w:orient=\"landscape\"/>"
               + "<w:pgMar w:top=\"720\" w:right=\"720\" w:bottom=\"720\" w:left=\"720\" w:header=\"708\" "
               + "w:footer=\"708\" w:gutter=\"0\"/></w:sectPr>"
               + "</w:body></w:document>";
    }
}

using System.Text;

namespace RevitVORChecker.Models.Report;

/// <summary>
/// Фрагменты разметки WordprocessingML для реестра.
/// </summary>
internal static class WordXml {
    private const string SectionFill = "DBE5F1";
    private const string BorderColor = "000000";

    public static string Escape(string text) {
        return (text ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    /// <summary>
    /// Абзац; переводы строк в тексте — разрывы строки.
    /// </summary>
    /// <param name="size">Размер шрифта в половинах пункта; 0 — по стилю.</param>
    public static string Paragraph(string text, bool bold = false, int size = 0) {
        string runProperties = "";
        if(bold || size > 0) {
            runProperties = "<w:rPr>" + (bold ? "<w:b/><w:bCs/>" : "")
                            + (size > 0 ? $"<w:sz w:val=\"{size}\"/><w:szCs w:val=\"{size}\"/>" : "") + "</w:rPr>";
        }

        var runs = new StringBuilder();
        string[] lines = (text ?? "").Split('\n');
        for(int index = 0; index < lines.Length; index++) {
            if(index > 0) {
                runs.Append("<w:r>" + runProperties + "<w:br/></w:r>");
            }

            runs.Append("<w:r>" + runProperties + "<w:t xml:space=\"preserve\">" + Escape(lines[index]) + "</w:t></w:r>");
        }

        return "<w:p>" + runs + "</w:p>";
    }

    /// <summary>
    /// Абзац «Подпись: значение» с полужирной подписью.
    /// </summary>
    public static string LabeledParagraph(string label, string value) {
        return "<w:p><w:r><w:rPr><w:b/><w:bCs/></w:rPr><w:t>" + Escape(label) + "</w:t></w:r>"
               + "<w:r><w:t xml:space=\"preserve\">: " + Escape(value) + "</w:t></w:r></w:p>";
    }

    public static string TableStart(IEnumerable<int> widths) {
        string borders = string.Concat(new[] { "top", "left", "bottom", "right", "insideH", "insideV" }
            .Select(side => $"<w:{side} w:val=\"single\" w:sz=\"4\" w:space=\"0\" w:color=\"{BorderColor}\"/>"));
        return "<w:tbl><w:tblPr><w:tblW w:w=\"5000\" w:type=\"pct\"/><w:tblBorders>" + borders + "</w:tblBorders>"
               + "<w:tblLayout w:type=\"fixed\"/><w:tblCellMar><w:left w:w=\"115\" w:type=\"dxa\"/>"
               + "<w:right w:w=\"115\" w:type=\"dxa\"/></w:tblCellMar></w:tblPr><w:tblGrid>"
               + string.Concat(widths.Select(width => $"<w:gridCol w:w=\"{width * 4}\"/>")) + "</w:tblGrid>";
    }

    /// <summary>
    /// Строка заголовков таблицы (повторяется на каждой странице).
    /// </summary>
    public static string HeaderRow(IEnumerable<string> titles, IEnumerable<int> widths) {
        var cells = titles.Zip(widths, (title, width) => Cell(Paragraph(title, bold: true, size: 20), width, null));
        return "<w:tr><w:trPr><w:tblHeader/><w:cantSplit/></w:trPr>" + string.Concat(cells) + "</w:tr>";
    }

    /// <summary>
    /// Строка заголовка раздела: заливка, название во втором столбце.
    /// </summary>
    public static string SectionRow(string title, IEnumerable<int> widths) {
        var cells = widths.Select((width, index) =>
            Cell(Paragraph(index == 1 ? title : "", bold: index == 1), width, SectionFill));
        return "<w:tr><w:trPr><w:cantSplit/></w:trPr>" + string.Concat(cells) + "</w:tr>";
    }

    /// <summary>
    /// Строка таблицы: содержимое первых ячеек, остальные пустые.
    /// </summary>
    public static string Row(IList<string> contents, IEnumerable<int> widths) {
        var cells = widths.Select((width, index) =>
            Cell(index < contents.Count ? contents[index] : Paragraph(""), width, null));
        return "<w:tr>" + string.Concat(cells) + "</w:tr>";
    }

    /// <summary>
    /// Вложенная таблица расхождений (мелкий шрифт, заголовок с заливкой).
    /// </summary>
    public static string NestedTable(List<string> header, List<List<string>> rows) {
        string Cell(string text, bool isHeader) {
            string shading = isHeader ? "<w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"EDEDED\"/>" : "";
            return "<w:tc><w:tcPr>" + shading + "</w:tcPr><w:p><w:pPr><w:spacing w:before=\"0\" w:after=\"0\"/></w:pPr>"
                   + "<w:r><w:rPr>" + (isHeader ? "<w:b/>" : "") + "<w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/></w:rPr>"
                   + "<w:t xml:space=\"preserve\">" + Escape(text) + "</w:t></w:r></w:p></w:tc>";
        }

        string borders = string.Concat(new[] { "top", "left", "bottom", "right", "insideH", "insideV" }
            .Select(side => $"<w:{side} w:val=\"single\" w:sz=\"4\" w:color=\"808080\"/>"));
        var table = new StringBuilder("<w:tbl><w:tblPr><w:tblW w:w=\"5000\" w:type=\"pct\"/><w:tblBorders>" + borders
                                      + "</w:tblBorders><w:tblLayout w:type=\"autofit\"/></w:tblPr><w:tblGrid>"
                                      + string.Concat(header.Select(_ => "<w:gridCol/>")) + "</w:tblGrid>");
        table.Append("<w:tr><w:trPr><w:tblHeader/></w:trPr>" + string.Concat(header.Select(title => Cell(title, true))) + "</w:tr>");
        foreach(List<string> row in rows) {
            table.Append("<w:tr>" + string.Concat(row.Select(value => Cell(value, false))) + "</w:tr>");
        }

        return table + "</w:tbl>";
    }

    /// <summary>
    /// Абзац с рисунком PNG.
    /// </summary>
    public static string ImageParagraph(string relationId, int pictureId, long widthEmu, long heightEmu, string name) {
        return "<w:p><w:r><w:drawing><wp:inline distT=\"0\" distB=\"0\" distL=\"0\" distR=\"0\">"
               + $"<wp:extent cx=\"{widthEmu}\" cy=\"{heightEmu}\"/><wp:docPr id=\"{pictureId}\" name=\"{name}\"/>"
               + "<a:graphic><a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\">"
               + $"<pic:pic><pic:nvPicPr><pic:cNvPr id=\"{pictureId}\" name=\"{name}\"/><pic:cNvPicPr/></pic:nvPicPr>"
               + $"<pic:blipFill><a:blip r:embed=\"{relationId}\"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill>"
               + $"<pic:spPr><a:xfrm><a:off x=\"0\" y=\"0\"/><a:ext cx=\"{widthEmu}\" cy=\"{heightEmu}\"/></a:xfrm>"
               + "<a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom></pic:spPr></pic:pic>"
               + "</a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>";
    }

    private static string Cell(string content, int width, string fill) {
        string shading = fill != null ? $"<w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"{fill}\"/>" : "";
        return $"<w:tc><w:tcPr><w:tcW w:w=\"{width}\" w:type=\"pct\"/>{shading}<w:vAlign w:val=\"center\"/></w:tcPr>"
               + content + "</w:tc>";
    }
}

using System;

using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Revit;

public class DocumentModel {
    public DocumentModel(Document document) {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        IsLink = false;
    }

    public DocumentModel(RevitLinkInstance link) {
        Link = link ?? throw new ArgumentNullException(nameof(link));
        Document = Link.GetLinkDocument();
        IsLink = true;
    }

    public string Name => Document.Title;

    public Document Document { get; }

    public bool IsLink { get; }

    public RevitLinkInstance Link { get; }
}

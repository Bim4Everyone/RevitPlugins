using System;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Services;
internal class ElementStatusService {
    private readonly Document _document;

    public ElementStatusService(Document document) {
        _document = document;
    }

    public bool IsElementOccupied(RevitElement revitElement, Action<string> onOccupied) {
        if(!_document.IsWorkshared) {
            return false;
        }

        var elementId = revitElement.Element.Id;
        var status = WorksharingUtils.GetCheckoutStatus(_document, elementId);

        if(status is CheckoutStatus.OwnedByOtherUser) {
            var info = WorksharingUtils.GetWorksharingTooltipInfo(_document, elementId);
            onOccupied(info.Owner);
            return true;
        }
        return false;
    }

    public bool IsDeletedOrUpdatedInCentral(RevitElement revitElement) {
        if(!_document.IsWorkshared) {
            return false;
        }
        var elementId = revitElement.Element.Id;
        var status = WorksharingUtils.GetModelUpdatesStatus(_document, elementId);

        return status is ModelUpdatesStatus.DeletedInCentral || status == ModelUpdatesStatus.UpdatedInCentral;
    }
}

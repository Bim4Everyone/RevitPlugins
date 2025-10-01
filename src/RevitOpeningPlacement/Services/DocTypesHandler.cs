using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services;
internal class DocTypesHandler : IDocTypesHandler {
    private readonly IBimModelPartsService _bimModelPartsService;

    public DocTypesHandler(IBimModelPartsService bimModelPartsService) {
        _bimModelPartsService = bimModelPartsService
            ?? throw new ArgumentNullException(nameof(bimModelPartsService));
    }


    public ICollection<BimModelPart> GetBimModelParts(DocTypeEnum docType) {
        return docType switch {
            DocTypeEnum.AR => new BimModelPart[] { BimModelPart.ARPart },
            DocTypeEnum.KR => new BimModelPart[] { BimModelPart.KRPart, BimModelPart.KMPart },
            DocTypeEnum.MEP => new BimModelPart[] {
                    BimModelPart.OVPart,
                    BimModelPart.ITPPart,
                    BimModelPart.HCPart,
                    BimModelPart.VKPart,
                    BimModelPart.EOMPart,
                    BimModelPart.EGPart,
                    BimModelPart.SSPart,
                    BimModelPart.VNPart,
                    BimModelPart.KVPart,
                    BimModelPart.OTPart,
                    BimModelPart.DUPart,
                    BimModelPart.VSPart,
                    BimModelPart.KNPart,
                    BimModelPart.PTPart,
                    BimModelPart.EOPart,
                    BimModelPart.EMPart },
            DocTypeEnum.KOORD => new BimModelPart[] { BimModelPart.KOORDPart },
            DocTypeEnum.NotDefined => Array.Empty<BimModelPart>(),
            _ => throw new InvalidOperationException(),
        };
    }

    public DocTypeEnum GetDocType(RevitLinkType linkType) {
        return linkType is null ? throw new ArgumentNullException(nameof(linkType)) : GetDocType(linkType.Name);
    }

    public DocTypeEnum GetDocType(Document document) {
        return document is null ? throw new ArgumentNullException(nameof(document)) : GetDocType(document.Title);
    }

    private DocTypeEnum GetDocType(string documentName) {
        if(string.IsNullOrWhiteSpace(documentName)) {
            throw new ArgumentException(nameof(documentName));
        }

        if(_bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.AR))) {
            return DocTypeEnum.AR;
        }

        if(_bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.KR))) {
            return DocTypeEnum.KR;
        }

        return _bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.MEP))
            ? DocTypeEnum.MEP
            : _bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.KOORD))
            ? DocTypeEnum.KOORD
            : DocTypeEnum.NotDefined;
    }
}

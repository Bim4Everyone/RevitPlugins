using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.Services {
    internal class BimModelPartsHandler : IDocTypesHandler {
        private readonly IBimModelPartsService _bimModelPartsService;

        public BimModelPartsHandler(IBimModelPartsService bimModelPartsService) {
            _bimModelPartsService = bimModelPartsService
                ?? throw new ArgumentNullException(nameof(bimModelPartsService));
        }


        public ICollection<BimModelPart> GetBimModelParts(DocTypeEnum docType) {
            switch(docType) {
                case DocTypeEnum.AR:
                    return new BimModelPart[] { BimModelPart.ARPart };
                case DocTypeEnum.KR:
                    return new BimModelPart[] { BimModelPart.KRPart, BimModelPart.KMPart };
                case DocTypeEnum.MEP:
                    return new BimModelPart[] {
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
                        BimModelPart.EMPart };
                case DocTypeEnum.KOORD:
                    return new BimModelPart[] { BimModelPart.KOORDPart };
                case DocTypeEnum.NotDefined:
                    return Array.Empty<BimModelPart>();
                default:
                    throw new InvalidOperationException();
            }
        }

        public DocTypeEnum GetDocType(RevitLinkType linkType) {
            if(linkType is null) {
                throw new ArgumentNullException(nameof(linkType));
            }

            return GetDocType(linkType.Name);
        }

        public DocTypeEnum GetDocType(Document document) {
            if(document is null) {
                throw new ArgumentNullException(nameof(document));
            }

            return GetDocType(document.Title);
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

            if(_bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.MEP))) {
                return DocTypeEnum.MEP;
            }

            if(_bimModelPartsService.InAnyBimModelParts(documentName, GetBimModelParts(DocTypeEnum.KOORD))) {
                return DocTypeEnum.KOORD;
            }

            return DocTypeEnum.NotDefined;
        }
    }
}

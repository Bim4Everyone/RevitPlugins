using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements {
    internal class DocumentsProcessor : IDocumentsProcessor {
        private readonly RevitRepository _revitRepository;
        private readonly IDataExporter _dataExporter;
        private readonly IDirectoryProvider _directoryProvider;

        public DocumentsProcessor(RevitRepository revitRepository, IDataExporter dataExporter, IDirectoryProvider directoryProvider) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _dataExporter = dataExporter ?? throw new ArgumentNullException(nameof(dataExporter));
            _directoryProvider = directoryProvider ?? throw new ArgumentNullException(nameof(directoryProvider));
        }


        public void ProcessDocuments(ICollection<IDocument> documents) {
            var exportDirectory = _directoryProvider.GetDirectory();

            IList<IDocumentData> data = new List<IDocumentData>();
            foreach(IDocument documentToProcess in documents) {
                using(Document document = _revitRepository.Application.OpenDocumentFile(documentToProcess.Path)) {

                    DocumentData documentData = new DocumentData(document.Title);
                    documentData.AddDuctData(GetDuctData(document));
                    documentData.AddPipeData(GetPipeData(document));
                    documentData.AddDuctInsulationData(GetDuctInsulationData(document));
                    documentData.AddPipeInsulationData(GetPipeInsulationData(document));
                    data.Add(documentData);

                    document.Close();
                }
            }

            _dataExporter.ExportData(exportDirectory, data);
        }




        private ICollection<IDuctData> GetDuctData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetDucts(document)
                .GroupBy(duct => new {
                    TypeName = _revitRepository.GetDuctTypeName(duct),
                    Size = _revitRepository.GetDuctSize(document, duct),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, duct)
                })
                .Select(group => new DuctData(group.Key.TypeName, group.Key.Size, group.Key.Name) {
                    Length = group.Sum(duct => _revitRepository.GetMepCurveElementLength(document, duct))
                })
                .ToArray();
        }

        private ICollection<IPipeData> GetPipeData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetPipes(document)
                .GroupBy(pipe => new {
                    TypeName = _revitRepository.GetPipeTypeName(pipe),
                    Size = _revitRepository.GetPipeSize(document, pipe),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, pipe)
                })
                .Select(group => new PipeData(group.Key.TypeName, group.Key.Size, group.Key.Name) {
                    Length = group.Sum(pipe => _revitRepository.GetMepCurveElementLength(document, pipe))
                })
                .ToArray();
        }

        private ICollection<IDuctInsulationData> GetDuctInsulationData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetDuctInsulations(document)
                .GroupBy(ductInsulation => new {
                    TypeName = _revitRepository.GetDuctInsulationTypeName(document, ductInsulation),
                    DuctSize = _revitRepository.GetDuctInsulationSize(document, ductInsulation),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, ductInsulation),
                    Thickness = _revitRepository.GetDuctInsulationThickness(document, ductInsulation)
                })
                .Select(group => new DuctInsulationData(group.Key.TypeName, group.Key.DuctSize, group.Key.Name) {
                    Thickness = group.Key.Thickness,
                    Length = group.Sum(ductInsulation => _revitRepository.GetMepCurveElementLength(document, ductInsulation)),
                    Area = group.Sum(ductInsulation => _revitRepository.GetMepCurveElementArea(document, ductInsulation))
                })
                .ToArray();
        }

        private ICollection<IPipeInsulationData> GetPipeInsulationData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetPipeInsulations(document)
                .GroupBy(pipeInsulation => new {
                    TypeName = _revitRepository.GetPipeInsulationTypeName(document, pipeInsulation),
                    PipeSize = _revitRepository.GetPipeInsulationSize(document, pipeInsulation),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, pipeInsulation),
                    Thickness = _revitRepository.GetPipeInsulationThickness(document, pipeInsulation)
                })
                .Select(group => new PipeInsulationData(group.Key.TypeName, group.Key.PipeSize, group.Key.Name) {
                    Thickness = group.Key.Thickness,
                    Length = group.Sum(pipeInsulation => _revitRepository.GetMepCurveElementLength(document, pipeInsulation)),
                    Area = group.Sum(pipeInsulation => _revitRepository.GetMepCurveElementArea(document, pipeInsulation))
                })
                .ToArray();
        }
    }
}

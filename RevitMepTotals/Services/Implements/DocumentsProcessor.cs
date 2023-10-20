using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements {
    internal class DocumentsProcessor : IDocumentsProcessor {
        private readonly RevitRepository _revitRepository;


        public DocumentsProcessor(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public IList<IDocumentData> ProcessDocuments(
            ICollection<IDocument> documents,
            out string errorMessage,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            IList<IDocumentData> data = new List<IDocumentData>();
            List<string> errors = new List<string>();

            OpenOptions options = GetOpenOptions();
            int i = 0;
            foreach(IDocument documentToProcess in documents) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i++);
                ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(documentToProcess.Path);
                try {
                    using(Document document = _revitRepository.Application.OpenDocumentFile(modelPath, options)) {
                        data.Add(GetDocumentData(document));
                        try {
                            document.Close(false);
                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            continue;
                        }
                    }
                } catch(Autodesk.Revit.Exceptions.CannotOpenBothCentralAndLocalException) {
                    errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать, т.к. он уже открыт.");
                } catch(Autodesk.Revit.Exceptions.CorruptModelException) {
                    errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать, " +
                        $"т.к. он создан в более поздней версии или в нем слишком много ошибок.");
                } catch(Autodesk.Revit.Exceptions.FileAccessException) {
                    errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать.");
                } catch(Autodesk.Revit.Exceptions.FileNotFoundException) {
                    errors.Add($"Документ \'{documentToProcess.Path}\' удален");
                } catch(Autodesk.Revit.Exceptions.InsufficientResourcesException) {
                    throw new OperationCanceledException("У компьютера недостаточно ресурсов, чтобы открыть модель, ");
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать");
                }
            }
            errorMessage = errors.Count > 0 ? string.Join(Environment.NewLine, errors) : string.Empty;
            return data;
        }

        private OpenOptions GetOpenOptions() {
            OpenOptions options = new OpenOptions() {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets
            };
            options.SetOpenWorksetsConfiguration(
                new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets));
            return options;
        }

        private IDocumentData GetDocumentData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            DocumentData documentData = new DocumentData(document.Title);
            documentData.AddDuctData(GetDuctData(document));
            documentData.AddPipeData(GetPipeData(document));
            documentData.AddDuctInsulationData(GetDuctInsulationData(document));
            documentData.AddPipeInsulationData(GetPipeInsulationData(document));
            return documentData;
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
                    Length = group.Sum(
                        ductInsulation => _revitRepository.GetMepCurveElementLength(document, ductInsulation)),
                    Area = group.Sum(
                        ductInsulation => _revitRepository.GetMepCurveElementArea(document, ductInsulation))
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
                    Length = group.Sum(
                        pipeInsulation => _revitRepository.GetMepCurveElementLength(document, pipeInsulation)),
                    Area = group.Sum(
                        pipeInsulation => _revitRepository.GetMepCurveElementArea(document, pipeInsulation))
                })
                .ToArray();
        }
    }
}

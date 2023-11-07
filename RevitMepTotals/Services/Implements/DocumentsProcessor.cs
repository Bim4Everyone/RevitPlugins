using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit.FileInfo;

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

            List<string> errors = new List<string>();
            ICollection<IDocument> notConflictedDocs = GetNotConflictedDocuments(documents, out string errorDocs);
            if(!string.IsNullOrWhiteSpace(errorDocs)) {
                errors.Add(errorDocs);
            }
            IList<IDocumentData> data = new List<IDocumentData>();
            OpenOptions options = GetOpenOptions();
            int i = 0;
            foreach(IDocument documentToProcess in notConflictedDocs) {
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
                    var test = new RevitFileInfo(documentToProcess.Path).BasicFileInfo.AppInfo.Format;
                    if(new RevitFileInfo(documentToProcess.Path).BasicFileInfo.AppInfo.Format
                        != _revitRepository.Application.VersionNumber) {
                        errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать, " +
                            $"т.к. он создан в более поздней версии.");
                    } else {
                        errors.Add($"Документ \'{documentToProcess.Path}\' нельзя обработать, " +
                            $"т.к. в нем слишком много ошибок.");
                    }
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
            documentData.AddPipeInsulationData(GetPipeInsulationData(document));
            return documentData;
        }

        /// <summary>
        /// Проверяет документы на конфликты имен и возвращает коллекцию документов из заданной коллекции,
        /// которые НЕ образуют конфликты.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="errorMessage">Сообщение об ошибке, или пустая строка, если ошибок нет</param>
        /// <returns></returns>
        private ICollection<IDocument> GetNotConflictedDocuments(
            ICollection<IDocument> data,
            out string errorMessage) {

            var docsWithNameConflicts = data
                .GroupBy(doc => string.Concat(doc.Name.Take(31)))
                .Where(group => group.Count() > 1)
                .SelectMany(group => group.ToArray())
                .ToArray();
            if(docsWithNameConflicts.Length > 0) {
                errorMessage = $"Документы:\n" +
                    $"{string.Join(Environment.NewLine, docsWithNameConflicts.Select(doc => doc.Name))}\n" +
                    $"нельзя выгрузить за один раз, т.к. они образуют конфликты имен";
            } else {
                errorMessage = string.Empty;
            }
            return data.Except(docsWithNameConflicts).ToArray();
        }

        private ICollection<IDuctData> GetDuctData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetDucts(document)
                .GroupBy(duct => new {
                    SystemName = _revitRepository.GetMepCurveElementMepSystemName(document, duct),
                    TypeName = _revitRepository.GetDuctTypeName(duct),
                    Size = GetStandardSizeFormat(_revitRepository.GetDuctSize(document, duct)),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, duct)
                })
                .Select(group =>
                new DuctData(group.Key.SystemName, group.Key.TypeName, group.Key.Size, group.Key.Name) {
                    Length = group.Sum(duct => _revitRepository.GetMepCurveElementLength(document, duct))
                })
                .ToArray();
        }

        private ICollection<IPipeData> GetPipeData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetPipes(document)
                .GroupBy(pipe => new {
                    SystemName = _revitRepository.GetMepCurveElementMepSystemName(document, pipe),
                    TypeName = _revitRepository.GetPipeTypeName(pipe),
                    Size = GetStandardSizeFormat(_revitRepository.GetPipeSize(document, pipe)),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, pipe)
                })
                .Select(group =>
                new PipeData(group.Key.SystemName, group.Key.TypeName, group.Key.Size, group.Key.Name) {
                    Length = group.Sum(pipe => _revitRepository.GetMepCurveElementLength(document, pipe))
                })
                .ToArray();
        }

        private ICollection<IPipeInsulationData> GetPipeInsulationData(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return _revitRepository
                .GetPipeInsulations(document)
                .GroupBy(pipeInsulation => new {
                    SystemName = _revitRepository.GetMepCurveElementMepSystemName(document, pipeInsulation),
                    TypeName = _revitRepository.GetPipeInsulationTypeName(document, pipeInsulation),
                    PipeSize = GetStandardSizeFormat(_revitRepository.GetPipeInsulationSize(document, pipeInsulation)),
                    Name = _revitRepository.GetMepCurveElementSharedName(document, pipeInsulation),
                    Thickness = _revitRepository.GetPipeInsulationThickness(document, pipeInsulation)
                })
                .Select(group =>
                new PipeInsulationData(group.Key.SystemName, group.Key.TypeName, group.Key.PipeSize, group.Key.Name) {
                    Thickness = group.Key.Thickness,
                    Length = group.Sum(
                        pipeInsulation => _revitRepository.GetMepCurveElementLength(document, pipeInsulation))
                })
                .ToArray();
        }

        /// <summary>
        /// Преобразует строку в формате 650x1000 к 1000x650б то есть наибольшее число вначале.
        /// Если формат поданной строки другой, вернется эта же строка.
        /// Символ 'x' может быть RUS или ENG
        /// </summary>
        /// <param name="sizeToFormat">Строка для преобразования</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetStandardSizeFormat(string sizeToFormat) {
            if(string.IsNullOrWhiteSpace(sizeToFormat)) {
                throw new ArgumentNullException(nameof(sizeToFormat));
            }

            var match = Regex.Match(sizeToFormat, @"^(\d+)(x|х)(\d+)$"); // например, 1000x650, где x или RUS, или ENG
            if(match.Success) {
                int width = int.Parse(match.Groups[1].Value); // первая цифра
                string splitter = match.Groups[2].Value; // x
                int height = int.Parse(match.Groups[3].Value); // вторая цифра
                return width > height ? sizeToFormat : (height + splitter + width); // наибольшая цифра в начале
            } else {
                return sizeToFormat;
            }
        }
    }
}

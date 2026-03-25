using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitVolumeModifier.Handler;
using RevitVolumeModifier.Services;

namespace RevitVolumeModifier.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly ExternalRevitHandler _handler;
    private readonly GeomObjectFactory _geomObjectFactory;
    private readonly DirectShapeObjectFactory _directShapeObjectFactory;
    private readonly ParamSetter _paramSetter;
    private readonly SolidService _solidService;

    public RevitRepository(
        UIApplication uiApplication,
        ILocalizationService localizationService,
        ExternalRevitHandler externalRevitHandler,
        GeomObjectFactory geomObjectFactory,
        DirectShapeObjectFactory directShapeObjectFactory,
        ParamSetter paramSetter,
        SolidService solidService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _handler = externalRevitHandler;
        _geomObjectFactory = geomObjectFactory;
        _directShapeObjectFactory = directShapeObjectFactory;
        _paramSetter = paramSetter;
        _solidService = solidService;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Метод объединения элементов
    /// </summary>
    public Task<bool> Join(ICollection<ElementId> elementIds, IEnumerable<ParamModel> paramModels) {
        return ExecuteOperationAsync(elementIds, paramModels, "RevitRepository.TransactionNameJoin", ProcessJoin);
    }

    /// <summary>
    /// Метод разрезания элементов по горизонтальной точке
    /// </summary>
    public Task<bool> DivideByHorizontalPointAsync(
        ICollection<ElementId> elementIds,
        Reference reference,
        IEnumerable<ParamModel> paramModels) {

        var plane = _solidService.GetHorizontalPlane(reference);
        return DivideElementsAsync(elementIds, [plane], paramModels);
    }

    /// <summary>
    /// Метод разрезания элементов по вертикальной точке
    /// </summary>
    public Task<bool> DivideByVerticalPointAsync(
        ICollection<ElementId> elementIds,
        Reference reference,
        IEnumerable<ParamModel> paramModels) {

        var plane = _solidService.GetVerticalPlane(Document, reference);
        return DivideElementsAsync(elementIds, [plane], paramModels);
    }

    /// <summary>
    /// Метод разрезания элементов по граням
    /// </summary>
    public Task<bool> DivideByFacesAsync(
        ICollection<ElementId> elementIds,
        IList<Reference> faces,
        IEnumerable<ParamModel> paramModels) {

        var planes = faces
            .Select(f => _solidService.GetPlaneFromFace(Document, f))
            .Where(p => p != null);

        return DivideElementsAsync(elementIds, planes, paramModels);
    }

    /// <summary>
    /// Метод вырезания элементов
    /// </summary>
    public Task<bool> CutAsync(
        ICollection<ElementId> elementIds,
        ICollection<ElementId> elementIdsToCut,
        IEnumerable<ParamModel> paramModels) {

        return ExecuteOperationAsync(elementIds, paramModels, "RevitRepository.TransactionNameCut", elements =>
            ProcessCut(elements, elementIdsToCut));
    }

    // Основной метод обработки
    private Task<bool> ExecuteOperationAsync(
        ICollection<ElementId> elementIds,
        IEnumerable<ParamModel> paramModels,
        string transactionNameKey,
        Func<List<Element>, OperationResult> operation) {

        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;
            using var t = doc.StartTransaction(_localizationService.GetLocalizedString(transactionNameKey));

            try {
                var elements = GetElements(doc, elementIds);
                if(!elements.Any()) {
                    tcs.SetResult(false);
                    return;
                }

                var result = operation(elements);

                if(result == null || !result.Items.Any()) {
                    t.RollBack();
                    UIApplication.ActiveUIDocument.Selection
                        .SetElementIds([.. elements.Select(e => e.Id)]);
                    tcs.SetResult(false);
                    return;
                }

                foreach(var item in result.Items) {
                    var source = doc.GetElement(item.SourceId);
                    if(source == null) {
                        continue;
                    }

                    _paramSetter.SetParams(source, [item.Shape], paramModels);
                }

                List<ElementId> elementsToSelect;

                if(result.ElementsToDelete.Any()) {
                    doc.Delete(result.ElementsToDelete);

                    elementsToSelect = result.Items
                        .Select(i => i.Shape.DirectShape.Id)
                        .ToList();
                } else {
                    elementsToSelect = elements.Select(e => e.Id).ToList();
                }

                UIApplication.ActiveUIDocument.Selection.SetElementIds(elementsToSelect);

                t.Commit();
                tcs.SetResult(result.Success);
            } catch {
                t.RollBack();
                tcs.SetResult(false);
                throw;
            }
        });

        return tcs.Task;
    }

    // Метод объединения элементов
    private OperationResult ProcessJoin(List<Element> elements) {
        var solids = _solidService.JoinElementSolids(elements, out bool success);
        if(!solids.Any()) {
            return new OperationResult { Success = false };
        }

        var geom = _geomObjectFactory.GetGeomObject([.. solids]);
        if(geom == null) {
            return new OperationResult { Success = false };
        }

        var ds = _directShapeObjectFactory.GetDirectShapeObject(geom, Document);
        return ds == null
            ? new OperationResult { Success = false }
            : new OperationResult {
                Success = success,
                Items = [
                new() {
                    SourceId = elements.First().Id,
                    Shape = ds
                }
            ],
                ElementsToDelete = elements.Select(e => e.Id).ToList()
            };
    }

    // Метод вырезания элементов
    private OperationResult ProcessCut(List<Element> elements, ICollection<ElementId> elementIdsToCut) {
        var elementsToCut = GetElements(Document, elementIdsToCut);
        if(!elementsToCut.Any()) {
            return new OperationResult { Success = false };
        }

        var resultDict = _solidService.CutElementSolids(
            elements, elementsToCut,
            out bool success,
            out var elementsToDelete);

        if(!resultDict.Any()) {
            return new OperationResult { Success = false };
        }

        var items = new List<OperationResultItem>();

        foreach(var kvp in resultDict) {
            var sourceId = kvp.Key;

            var geom = _geomObjectFactory.GetGeomObject([.. kvp.Value]);
            if(geom == null) {
                continue;
            }

            var ds = _directShapeObjectFactory.GetDirectShapeObject(geom, Document);
            if(ds == null) {
                continue;
            }

            items.Add(new OperationResultItem {
                SourceId = sourceId,
                Shape = ds
            });
        }

        return new OperationResult {
            Success = success,
            Items = items,
            ElementsToDelete = elementsToDelete
        };
    }

    // Метод деления элементов
    private Task<bool> DivideElementsAsync(
        ICollection<ElementId> elementIds,
        IEnumerable<DividePlane> planes,
        IEnumerable<ParamModel> paramModels) {

        return ExecuteOperationAsync(
            elementIds,
            paramModels,
            "RevitRepository.TransactionNameDivide",
            elements => ProcessDivide(elements, planes));
    }

    // Метод деления элементов
    private OperationResult ProcessDivide(List<Element> elements, IEnumerable<DividePlane> planes) {
        var planeList = planes?.ToList() ?? [];
        if(planeList.Count == 0) {
            return new OperationResult { Success = false };
        }

        var items = new List<OperationResultItem>();
        var toDelete = new List<ElementId>();

        foreach(var element in elements) {
            var parts = element.GetSolids()
                .Where(s => s != null && s.Volume > SolidService.VolumeEpsilon)
                .ToList();

            if(parts.Count == 0) {
                continue;
            }

            bool elementWasDivided = false;

            foreach(var plane in planeList) {
                var newParts = new List<Solid>();

                foreach(var part in parts) {
                    var pos = _solidService.DivideSolidSafe(part, plane.PositivePlane);
                    var neg = _solidService.DivideSolidSafe(part, plane.NegativePlane);

                    double vPos = pos?.Volume ?? 0;
                    double vNeg = neg?.Volume ?? 0;

                    if(vPos > SolidService.VolumeEpsilon && vNeg > SolidService.VolumeEpsilon) {
                        newParts.Add(pos);
                        newParts.Add(neg);
                        elementWasDivided = true;
                    } else {
                        newParts.Add(part);
                    }
                }

                parts = newParts;
            }

            if(!elementWasDivided) {
                continue;
            }

            foreach(var solid in parts.Where(s => s != null && s.Volume > SolidService.VolumeEpsilon)) {
                var geom = _geomObjectFactory.GetGeomObject([solid]);
                if(geom == null) {
                    continue;
                }

                var ds = _directShapeObjectFactory.GetDirectShapeObject(geom, Document);
                if(ds == null) {
                    continue;
                }

                items.Add(new OperationResultItem {
                    SourceId = element.Id,
                    Shape = ds
                });
            }

            if(items.Any(i => i.SourceId == element.Id)) {
                toDelete.Add(element.Id);
            }
        }

        return new OperationResult {
            Success = items.Any(),
            Items = items,
            ElementsToDelete = toDelete.Distinct().ToList()
        };
    }

    // Метод получения элементов
    private List<Element> GetElements(Document document,
        ICollection<ElementId> elementIds) {

        var filter = new DirectShapeOrInPlaceFilter(document);

        return elementIds
            .Select(document.GetElement)
            .Where(el => el != null && filter.AllowElement(el))
            .ToList();
    }
}

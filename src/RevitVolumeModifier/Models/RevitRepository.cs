using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitVolumeModifier.Enums;
using RevitVolumeModifier.Handler;

namespace RevitVolumeModifier.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ExternalRevitHandler _handler;

    public RevitRepository(
        UIApplication uiApplication,
        ILocalizationService localizationService,
        SystemPluginConfig systemPluginConfig,
        ExternalRevitHandler externalRevitHandler) {

        UIApplication = uiApplication;
        _localizationService = localizationService;
        _systemPluginConfig = systemPluginConfig;
        _handler = externalRevitHandler;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public Task<bool> Join(ICollection<ElementId> elements) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;
            try {
                string transactionName = _localizationService.GetLocalizedString("SetCoordParamsProcessor.TransactionName");
                using var t = doc.StartTransaction(transactionName);

                // TODO: логика разделения

                t.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }

    public Task<bool> DivideByHorizontalPointAsync(ICollection<ElementId> elements, XYZ point) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;

            try {
                using var tx = new Transaction(doc, "DivideHorizontal");
                tx.Start();

                // TODO: логика разделения

                tx.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }

    public Task<bool> DivideByVerticalPointAsync(ICollection<ElementId> elements, XYZ point) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;

            try {
                using var tx = new Transaction(doc, "DivideVirtical");
                tx.Start();

                // TODO: логика разделения

                tx.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }

    public Task<bool> DivideByThreePointsAsync(ICollection<ElementId> elements, XYZ point1, XYZ point2, XYZ point3) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;

            try {
                using var tx = new Transaction(doc, "DivideThreePoints");
                tx.Start();

                // TODO: логика разделения

                tx.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }

    public Task<bool> DivideByFacesAsync(ICollection<ElementId> elements, IList<Reference> faces) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;

            try {
                using var tx = new Transaction(doc, "DivideByFaces");
                tx.Start();

                // TODO: логика разделения

                tx.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }

    public Task<bool> CutAsync(ICollection<ElementId> elements, ICollection<ElementId> elementsToCut, bool saveVolume) {
        var tcs = new TaskCompletionSource<bool>();

        _handler.Raise(app => {
            var doc = app.ActiveUIDocument.Document;

            try {
                using var tx = new Transaction(doc, "Cut");
                tx.Start();

                // TODO: логика разделения

                tx.Commit();

                tcs.SetResult(true);
            } catch(Exception) {
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
    }


    public IEnumerable<Element> GetSelectionElements() {
        var selectionIds = ActiveUIDocument.GetSelectedElements();
        return selectionIds
            .Where(ElementMatchesCategory);
    }

    // Метод фильтрации элементов по категории
    private bool ElementMatchesCategory(Element element) {
        var category = element.Category?.GetBuiltInCategory();
        return category is not null && category.Value == _systemPluginConfig.ModelCategory;
    }

    public IEnumerable<string> GetElementsValues(ICollection<ElementId> selection, ParamModel param) {
        if(selection == null || !selection.Any() || param?.RevitParam == null) {
            return [_localizationService.GetLocalizedString("RevitRepository.NoParamValue")];
        }

        string paramName = param.RevitParam.Name;

        return selection
            .Select(elementId => {
                var element = Document.GetElement(elementId);
                return element == null || !element.IsExistsParam(paramName)
                    ? null
                    : param.ParamType switch {
                        ParamType.VolumeParam =>
                            element.GetParamValueOrDefault<double>(paramName).ToString(),

                        ParamType.FloorDEParam =>
                            element.GetParamValueOrDefault<double>(paramName).ToString(),

                        _ =>
                            element.GetParamValueOrDefault<string>(paramName)
                    };
            })
            .Where(str => !string.IsNullOrWhiteSpace(str))
            .Distinct();
    }

    public string GetVolume(ICollection<ElementId> selection, ParamModel param) {
        if(selection == null || !selection.Any() || param?.RevitParam == null) {
            return _localizationService.GetLocalizedString("RevitRepository.NoParamValue");
        }

        string paramName = param.RevitParam.Name;

        var volumes = selection
            .Select(elementId => {
                var element = Document.GetElement(elementId);
                return element == null || !element.IsExistsParam(paramName)
                    ? 0
                    : element.GetParamValueOrDefault<double>(paramName);
            });

        double sumVolumes = volumes.Sum();
        return sumVolumes > 0
            ? sumVolumes.ToString()
            : _localizationService.GetLocalizedString("RevitRepository.NoParamValue");
    }
}

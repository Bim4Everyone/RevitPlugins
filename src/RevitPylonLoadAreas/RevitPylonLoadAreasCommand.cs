using System;
using System.Globalization;
using System.Linq;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.Revit;
using Ninject;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Drawing;
using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas;

/// <summary>
/// Команда плагина "Грузовые площади": считает площади Вороного для пилонов
/// и рисует их на активном виде detail-линиями.
///
/// В v1 окно UI не используется: настройки берутся из <see cref="PluginConfig.CreateDefault"/>,
/// взаимодействие с пользователем сводится к трем последовательным PickObjects:
/// плиты → пилоны → стены.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitPylonLoadAreasCommand : BasePluginCommand {
    public RevitPylonLoadAreasCommand() {
        PluginName = "Грузовые площади";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>().ToSelf().InSingletonScope();
        kernel.Bind<PluginConfig>().ToConstant(PluginConfig.CreateDefault());
        kernel.Bind<PylonLoadAreasProcessor>().ToSelf();

        var repository = kernel.Get<RevitRepository>();
        var processor = kernel.Get<PylonLoadAreasProcessor>();

        var activeView = repository.ActiveView;
        if(activeView == null || !CanHostDetailLines(activeView)) {
            ShowError("Активный вид не поддерживает detail-линии (нужен план, разрез или фасад).");
            throw new OperationCanceledException();
        }

        var floors = repository.PickFloors("Выберите плиты для расчета грузовых площадей");
        if(floors.Count == 0) {
            throw new OperationCanceledException();
        }

        var pylons = repository.PickStructuralColumns("Выберите несущие колонны (пилоны)");
        if(pylons.Count == 0) {
            throw new OperationCanceledException();
        }

        var walls = TryPickWalls(repository);

        var result = processor.Run(floors, pylons, walls);

        using(var t = new Transaction(repository.Document, "Грузовые площади")) {
            t.Start();
            var drawer = new DetailLineDrawer(repository.Document);
            drawer.Draw(activeView, result.SlabElevation, result.ByPylon.Values);
            foreach(var pylonData in result.ByPylon) {
                repository.Document.GetElement(pylonData.Key)
                    .SetParamValue(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                        UnitUtils.ConvertFromInternalUnits(pylonData.Value.TotalArea, UnitTypeId.SquareMeters)
                            .ToString(CultureInfo.CurrentCulture));
            }
            t.Commit();
        }

        ShowSummary(result, processor.Warnings.Count);
    }

    private static System.Collections.Generic.IReadOnlyList<Wall> TryPickWalls(RevitRepository repository) {
        try {
            return repository.PickWalls(
                "Выберите стены под плитой (ESC чтобы пропустить)");
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            return Array.Empty<Wall>();
        }
    }

    private static bool CanHostDetailLines(View view) {
        return view.ViewType == ViewType.FloorPlan
            || view.ViewType == ViewType.CeilingPlan
            || view.ViewType == ViewType.EngineeringPlan
            || view.ViewType == ViewType.AreaPlan
            || view.ViewType == ViewType.Section
            || view.ViewType == ViewType.Elevation
            || view.ViewType == ViewType.Detail
            || view.ViewType == ViewType.DraftingView;
    }

    private static void ShowSummary(PylonLoadAreasResult result, int warningCount) {
        double totalSqM = result.ByPylon.Values.Sum(a => GeometryTolerance.SqFeetToSqMeters(a.TotalArea));
        var sb = new StringBuilder();
        sb.AppendLine($"Обработано пилонов: {result.ByPylon.Count}");
        sb.AppendLine($"Суммарная площадь: {totalSqM:0.00} м²");
        if(warningCount > 0) {
            sb.AppendLine($"Предупреждений: {warningCount} (см. журнал).");
        }
        TaskDialog.Show("Грузовые площади", sb.ToString());
    }

    private static void ShowError(string message) {
        TaskDialog.Show("Грузовые площади", message);
    }
}

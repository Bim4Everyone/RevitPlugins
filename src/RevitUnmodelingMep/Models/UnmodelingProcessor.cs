using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitUnmodelingMep.Models;

internal class UnmodelingProcessor {
    private readonly Document _doc;
    private WorksetId _ws_id;
    public UnmodelingProcessor(Document doc) {
        _doc = doc;
    }

    public void StartupChecks() {

        List<RevitParam> revitParams = [
            SharedParamsConfig.Instance.EconomicFunction,
                        SharedParamsConfig.Instance.VISSystemName,
                        SharedParamsConfig.Instance.BuildingWorksBlock,
                        SharedParamsConfig.Instance.BuildingWorksSection,
                        SharedParamsConfig.Instance.BuildingWorksLevel,
                        SharedParamsConfig.Instance.VISSettings
            ];

        ProjectParameters projectParameters = ProjectParameters.Create(_doc.Application);
        projectParameters.SetupRevitParams(_doc, revitParams);

        CheckWorksets();




    }

    private void CheckWorksets() {
        string targetName = "99_Немоделируемые элементы";
        string warningText = "Рабочий набор \"99_Немоделируемые элементы\" на данный момент отображается на всех видах.\n\n" +
                "Откройте диспетчер рабочих наборов и снимите галочку с параметра \"Видимый на всех видах\".\n\n" +
                "В данном рабочем наборе будут создаваться немоделируемые элементы и требуется исключить их видимость.";
        string warningCaption = "Рабочие наборы";

        if(WorksetTable.IsWorksetNameUnique(_doc, targetName)) {
            using(var t = _doc.StartTransaction("Настройка рабочих наборов")) {

                Workset newWS = Workset.Create(_doc, targetName);
                _ws_id = newWS.Id;

                MessageBox.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);

                t.Commit();
            }
        } else {
            List<Workset> userWSs = new FilteredWorksetCollector(_doc).OfKind(WorksetKind.UserWorkset).ToList();

            foreach (Workset ws in userWSs) {
                if(ws.Name == targetName) {
                    _ws_id = ws.Id;
                    if(ws.IsVisibleByDefault) {
                        MessageBox.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
        }
    }
}

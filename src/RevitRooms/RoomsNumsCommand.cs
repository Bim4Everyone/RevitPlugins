using System;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitRooms.Models;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms;
[Transaction(TransactionMode.Manual)]
public class RoomsNumsCommand : BasePluginCommand {
    public RoomsNumsCommand() {
        PluginName = "Нумерация помещений с приоритетом";
    }

    protected override void Execute(UIApplication uiApplication) {
        bool isChecked = new CheckProjectParams(uiApplication)
            .CopyProjectParams()
            .CopyKeySchedules()
            .CheckKeySchedules()
            .GetIsChecked();

        if(!isChecked) {
            throw new OperationCanceledException();
        }

        var window = new RoomsNumsWindows() { Title = PluginName };
        window.DataContext = new RoomNumsViewModel(new RevitRepository(uiApplication), window);
        Notification(window);
    }
}
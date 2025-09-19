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
public class RoomsCommand : BasePluginCommand {
    public RoomsCommand() {
        PluginName = "Квартирография Стадии П";
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

        var viewModel = new RoomsViewModel(new RevitRepository(uiApplication));
        var window = new RoomsWindow() { Title = PluginName, DataContext = viewModel };
        Notification(window);
    }
}

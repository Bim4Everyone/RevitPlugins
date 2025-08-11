using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitExportSpecToExcel.Models;

namespace RevitExportSpecToExcel.ViewModels;

internal class ScheduleViewModel : BaseViewModel {
    private readonly string _name;
    private readonly OpenStatus _openStatus;
    private readonly ViewSchedule _schedule;
    private bool _isChecked;

    public ScheduleViewModel(ViewSchedule schedule, OpenStatus status) {
        _schedule = schedule;
        _name = schedule.Name;
        _openStatus = status;

        if(_openStatus == OpenStatus.ActiveView) {
            IsChecked = true;
        }
    }

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public ViewSchedule Schedule => _schedule;

    public string Name => _name;
    public OpenStatus OpenStatus => _openStatus;
}


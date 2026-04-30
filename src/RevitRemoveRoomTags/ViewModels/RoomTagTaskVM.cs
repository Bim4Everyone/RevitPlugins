using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitRemoveRoomTags.Models;
internal class RoomTagTaskVM : BaseViewModel, IDataErrorInfo {
    private double _xOffset = 0;
    private string _xOffsetAsStr = "0";
    private double _yOffset = 0;
    private string _yOffsetAsStr = "0";
    private bool _removeTags = false;

    public RoomTagTaskVM() { }

    public ObservableCollection<RoomTag> RoomTags { get; } = [];

    public double XOffset {
        get => _xOffset;
        set => RaiseAndSetIfChanged(ref _xOffset, value);
    }

    public string XOffsetAsStr {
        get => _xOffsetAsStr;
        set => RaiseAndSetIfChanged(ref _xOffsetAsStr, value);
    }

    public double YOffset {
        get => _yOffset;
        set => RaiseAndSetIfChanged(ref _yOffset, value);
    }

    public string YOffsetAsStr {
        get => _yOffsetAsStr;
        set => RaiseAndSetIfChanged(ref _yOffsetAsStr, value);
    }

    public bool RemoveTags {
        get => _removeTags;
        set => RaiseAndSetIfChanged(ref _removeTags, value);
    }


    public string this[string columnName] {
        get {
            string error = string.Empty;
            switch(columnName) {
                case "XOffsetAsStr":
                    if(!double.TryParse(XOffsetAsStr, out double tempX)) {
                        error = "Ошибка в заполнении смещения по X";
                    }
                    XOffset = tempX;
                    break;
                case "YOffsetAsStr":
                    if(!double.TryParse(YOffsetAsStr, out double tempY)) {
                        error = "Ошибка в заполнении смещения по Y";
                    }
                    YOffset = tempY;
                    break;
            }
            return error;
        }
    }
    public string Error => throw new NotImplementedException();
}

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB.Architecture;

namespace RevitRemoveRoomTags.Models;
internal class RoomTagTaskHelper : IDataErrorInfo {

    public RoomTagTaskHelper() { }

    public ICollection<RoomTag> RoomTags { get; } = [];

    public double XOffset { get; set; } = 0;
    public string XOffsetAsStr { get; set; } = "0";
    public double YOffset { get; set; } = 0;
    public string YOffsetAsStr { get; set; } = "0";

    public bool RemoveTags { get; set; } = false;


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

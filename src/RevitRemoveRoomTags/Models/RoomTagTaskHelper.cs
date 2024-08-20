using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

namespace RevitRemoveRoomTags.Models {
    internal class RoomTagTaskHelper : IDataErrorInfo {

        public RoomTagTaskHelper() { }

        public ICollection<RoomTag> RoomTags { get; } = new List<RoomTag>();

        public double XOffset { get; set; } = 0;
        public string XOffsetAsStr { get; set; } = "0";
        public double YOffset { get; set; } = 0;
        public string YOffsetAsStr { get; set; } = "0";

        public bool RemoveTags { get; set; } = false;


        public string this[string columnName] {
            get {
                string error = String.Empty;
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
        public string Error {
            get { throw new NotImplementedException(); }
        }
    }
}

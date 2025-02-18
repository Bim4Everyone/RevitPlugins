using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models {
    internal class PublicArea : RoomGroup {
        private readonly PublicAreasSettings _settings;

        private readonly string _roomPosition;
        private readonly bool _isUnderground;

        public PublicArea(IEnumerable<RoomElement> rooms, 
                          DeclarationSettings settings, 
                          RoomParamProvider paramProvider)
            : base(rooms, settings, paramProvider) {
            _settings = (PublicAreasSettings) settings;

            _roomPosition = _paramProvider.GetRoomsPosition(this);

            if(_roomPosition.IndexOf("подземный", StringComparison.OrdinalIgnoreCase) >= 0) {
                _isUnderground = true;
            }
        }

        public string DeclarationNumber => 
            _paramProvider.GetTwoParamsWithHyphen(_firstRoom, _settings.AddPrefixToNumber);

        public override string Department => _paramProvider.GetDepartment(_firstRoom, "МОП");
        public override double AreaMain {
            get {
                if(_isOneRoomGroup) {
                    return _firstRoom.Area;
                } else {
                    return base.AreaMain;
                }
            }
        }

        public string GroupName => _firstRoom.Name;

        public string RoomPosition => _roomPosition;
        public bool IsUnderground => _isUnderground;
    }
}

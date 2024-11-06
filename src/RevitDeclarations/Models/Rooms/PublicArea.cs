using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models
{
    internal class PublicArea : RoomGroup {
        private readonly PublicAreasSettings _settings;

        public PublicArea(IEnumerable<RoomElement> rooms, DeclarationSettings settings)
            : base(rooms, settings) {
            _settings = (PublicAreasSettings)settings;
        }

        public override string Number {
            get {
                if(_settings.AddPrefixToNumber) {
                    return $"{base.Number}-{_firstRoom.Number}";
                } else {
                    return base.Number;
                }
            }
        }

        public override double AreaMain {
            get {
                if(_isOneRoomGroup) {
                    return _firstRoom.Area;
                } else {
                    return AreaMain;
                }
            }
        }

        public string GroupName => _firstRoom.Name;


        public string RoomPosition {
            get {
                return $"{Building}-{Section}-{Level}";
            }
        }
    }
}
